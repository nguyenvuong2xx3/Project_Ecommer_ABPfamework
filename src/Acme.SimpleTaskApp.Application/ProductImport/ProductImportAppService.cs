using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.UploadFile;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using OfficeOpenXml.Drawing; // Đảm bảo có using này
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductImport
{
	public interface IProductImportAppService : IApplicationService
	{
		Task<ImportResult> ImportProductsWithImagesAsync(IFormFile excelFile);
	}

	public class ProductImportAppService : ApplicationService, IProductImportAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<ProductVariant> _productVariantRepository;
		private readonly IRepository<ProductImage> _productImageRepository;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IUploadFileAppService _uploadFileAppService;

		public ProductImportAppService(
			IRepository<Product> productRepository,
			IRepository<ProductVariant> productVariantRepository,
			IRepository<ProductImage> productImageRepository,
			IRepository<Category> categoryRepository,
			IUploadFileAppService uploadFileAppService)
		{
			_productRepository = productRepository;
			_productVariantRepository = productVariantRepository;
			_productImageRepository = productImageRepository;
			_categoryRepository = categoryRepository;
			_uploadFileAppService = uploadFileAppService;
		}

		[UnitOfWork]
		public async Task<ImportResult> ImportProductsWithImagesAsync(IFormFile excelFile)
		{
			ExcelPackage.License.SetNonCommercialPersonal("ImportForDoAn");
			var result = new ImportResult();
			var errorDetails = new List<string>();
			int successCount = 0;
			int totalVariantCount = 0; // Đếm tổng số biến thể thay vì sản phẩm

			// Dùng để lưu trữ dữ liệu đọc từ Excel,
			// vì MemoryStream sẽ bị dispose sau khi đọc
			List<ProductImportItem> productDataList;

			try
			{
				// ***** BẮT ĐẦU SỬA LỖI *****
				// Stream từ IFormFile (excelFile.OpenReadStream()) thường không "seekable"
				// (không thể tua tới/lui), nhưng EPPlus cần một stream "seekable" để
				// đọc định dạng file .xlsx (vốn là một file Zip).
				// Lỗi 'ReadTimeout' là triệu chứng của vấn đề này.
				//
				// GIẢI PHÁP: Copy stream của file upload vào một MemoryStream (luôn "seekable").

				using (var memoryStream = new MemoryStream())
				{
					// 1. Copy stream của file upload vào MemoryStream
					await excelFile.CopyToAsync(memoryStream);

					// 2. Tua MemoryStream về vị trí đầu để EPPlus có thể đọc
					memoryStream.Position = 0;

					// 3. Extract product data từ MemoryStream
					// Phương thức ExtractProductDataWithImages sẽ tự động
					// dispose 'memoryStream' khi 'ExcelPackage' bên trong nó được dispose.
					productDataList = await ExtractProductDataWithImages(memoryStream);
				}

				// ***** KẾT THÚC SỬA LỖI *****


				if (productDataList == null || productDataList.Count == 0)
				{
					result.IsSuccess = false;
					result.ErrorMessage = "File Excel không chứa dữ liệu hoặc định dạng không hợp lệ. (Đã copy vào MemoryStream)";
					return result;
				}

				// 4. Loop qua từng product (Logic này giữ nguyên)
				foreach (var productData in productDataList)
				{
					totalVariantCount += productData.Variants.Count; // Tăng tổng số biến thể
					try
					{
						// Validate dữ liệu cơ bản (đã được kiểm tra trong Extract)
						var existingCategory = await _categoryRepository.FirstOrDefaultAsync(
							c => c.Name == productData.CategoryName);
						if (existingCategory == null)
						{
							errorDetails.Add($"Sản phẩm '{productData.Name}': Tên danh mục '{productData.CategoryName}' không tồn tại");
							continue;
						}

						var existingProduct = await _productRepository.FirstOrDefaultAsync(p => p.Name == productData.Name);

						Product product;

						if (existingProduct != null)
						{
							// Update product
							existingProduct.Name = productData.Name;
							existingProduct.Description = productData.Description;
							existingProduct.CategoryId = existingCategory.Id;
							existingProduct.Screen = productData.Screen;
							existingProduct.Processor = productData.Processor;
							existingProduct.CameraSystem = productData.CameraSystem;
							existingProduct.Battery = productData.Battery;

							product = await _productRepository.UpdateAsync(existingProduct);
						}
						else
						{
							// Tạo product mới
							product = new Product
							{
								Name = productData.Name,
								Description = productData.Description,
								CategoryId = existingCategory.Id,
								Screen = productData.Screen,
								Processor = productData.Processor,
								CameraSystem = productData.CameraSystem,
								Battery = productData.Battery,
								CreationTime = DateTime.Now
							};

							product = await _productRepository.InsertAsync(product);
						}

						// 5. Import ảnh sản phẩm (Col M) - (Giữ nguyên)
						if (existingProduct != null)
						{
							await _productImageRepository.DeleteAsync(pi => pi.ProductId == product.Id && pi.ProductVariantId == null);
						}
						await SaveAndLinkImagesAsync(product.Id, null, productData.MainImages, product.Name);

						// 6. Import variants (Giữ nguyên)
						if (productData.Variants != null && productData.Variants.Count > 0)
						{
							int variantIndex = 0;
							foreach (var variantData in productData.Variants)
							{
								try
								{
									variantIndex++;

									var existingVariant = await _productVariantRepository.FirstOrDefaultAsync(
										v => v.ProductId == product.Id
											&& v.Color == variantData.Color
											&& v.Storage == variantData.Storage
											&& v.Ram == variantData.Ram);

									ProductVariant variant;

									if (existingVariant != null)
									{
										// Update variant
										existingVariant.Color = variantData.Color;
										existingVariant.Storage = variantData.Storage;
										existingVariant.Ram = variantData.Ram;
										existingVariant.Price = variantData.Price;
										existingVariant.StockQuantity = variantData.StockQuantity;

										variant = await _productVariantRepository.UpdateAsync(existingVariant);

										// Xóa ảnh cũ của variant
										await _productImageRepository.DeleteAsync(pi => pi.ProductVariantId == variant.Id);
									}
									else
									{
										// Tạo variant mới
										variant = new ProductVariant
										{
											ProductId = product.Id,
											Color = variantData.Color,
											Storage = variantData.Storage,
											Ram = variantData.Ram,
											Price = variantData.Price,
											StockQuantity = variantData.StockQuantity,
											CreationTime = DateTime.Now
										};

										variant = await _productVariantRepository.InsertAsync(variant);
									}

									// 7. Import ảnh của variant (Col N) - (Giữ nguyên)
									await SaveAndLinkImagesAsync(
										product.Id,
										variant.Id,
										variantData.Images,
										$"{product.Name} - {variantData.Color}");

									successCount++;
								}
								catch (Exception ex)
								{
									errorDetails.Add(
										$"Sản phẩm '{productData.Name}', Variant {variantIndex} ({variantData.Color}): {ex.Message}");
								}
							}
						}
						else
						{
							errorDetails.Add($"Sản phẩm '{productData.Name}': Không có biến thể nào được định nghĩa.");
						}
					}
					catch (Exception ex)
					{
						errorDetails.Add($"Sản phẩm '{productData.Name}': {ex.Message}");
					}
				}

				await CurrentUnitOfWork.SaveChangesAsync();

				result.IsSuccess = true;
				result.SuccessCount = successCount;
				result.TotalCount = totalVariantCount; // Kết quả dựa trên số biến thể
				result.Message = $"Đã import thành công {successCount}/{totalVariantCount} biến thể sản phẩm.";

				if (errorDetails.Count > 0)
				{
					result.Warnings = errorDetails;
					result.WarningCount = errorDetails.Count;
				}
			}
			catch (Exception ex)
			{
				result.IsSuccess = false;

				// Thêm kiểm tra lỗi file hỏng (thường là InvalidDataException khi đọc Zip)
				if (ex is InvalidDataException || (ex.InnerException != null && ex.InnerException is InvalidDataException))
				{
					result.ErrorMessage = $"Lỗi đọc file Excel: File không đúng định dạng .xlsx hoặc bị hỏng. Hãy chắc chắn rằng bạn đã tải đúng file template.";
				}
				else
				{
					result.ErrorMessage = $"Lỗi nghiêm trọng khi import file Excel: {ex.Message}";
				}

				result.ErrorDetails = ex.StackTrace;
			}

			return result;
		}

		/// <summary>
		/// Import ảnh, liên kết với ProductId và (tùy chọn) VariantId
		/// </summary>
		private async Task SaveAndLinkImagesAsync(int productId, int? variantId, List<byte[]> images, string altTextPrefix)
		{
			if (images == null || images.Count == 0)
			{
				// Nếu là variant và không có ảnh, thêm ảnh mặc định
				if (variantId.HasValue)
				{
					// Kiểm tra xem có ảnh chung của sản phẩm không
					var productHasImages = await _productImageRepository.CountAsync(pi => pi.ProductId == productId && pi.ProductVariantId == null);

					// Nếu sản phẩm CŨNG không có ảnh chung, mới thêm ảnh default
					if (productHasImages == 0)
					{
						var defaultImage = new ProductImage
						{
							ProductVariantId = variantId,
							ProductId = productId,
							ImageUrl = "/img/products/default.png", // Đường dẫn ảnh mặc định
							SortOrder = 0,
							AltText = "Ảnh mặc định"
						};
						await _productImageRepository.InsertAsync(defaultImage);
					}
				}
				// Nếu là ảnh sản phẩm (variantId == null) và không có ảnh,
				// thì không làm gì cả.
				return;
			}

			// Lưu tất cả ảnh
			for (int i = 0; i < images.Count; i++)
			{
				try
				{
					var imageBytes = images[i];
					// Tạo tên file an toàn
					var safePrefix = string.Join("-", altTextPrefix.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
					var fileName = $"{safePrefix}_{Guid.NewGuid().ToString().Substring(0, 8)}.jpg";

					// Lưu ảnh vào server
					var imageUrl = await SaveImageToServerAsync(imageBytes, fileName);

					// Tạo record ProductImage
					var productImage = new ProductImage
					{
						ProductVariantId = variantId, // Sẽ là null nếu đây là ảnh chung của sản phẩm
						ProductId = productId,        // Luôn có
						ImageUrl = imageUrl,
						SortOrder = i,
						AltText = $"{altTextPrefix} - Hình {i + 1}"
					};

					await _productImageRepository.InsertAsync(productImage);
				}
				catch (Exception ex)
				{
					throw new UserFriendlyException(
						$"Lỗi khi lưu ảnh {i + 1} cho {altTextPrefix}: {ex.Message}");
				}
			}
		}

		/// <summary>
		/// Lưu byte array ảnh lên server
		/// </summary>
		private async Task<string> SaveImageToServerAsync(byte[] imageBytes, string fileName)
		{
			try
			{
				// Tạo MemoryStream từ bytes
				using (var memoryStream = new MemoryStream(imageBytes))
				{
					// Tạo fake IFormFile
					var formFile = new FakeFormFile(memoryStream, fileName);
					var imageUrl = await _uploadFileAppService.UploadImageAsync(formFile, "products/import");
					return imageUrl;
				}
			}
			catch (Exception ex)
			{
				throw new UserFriendlyException($"Không thể lưu ảnh: {ex.Message}");
			}
		}

		/// <summary>
		/// (ĐÃ CẬP NHẬT) Extract product data từ Excel file kèm ảnh nhúng
		/// Logic này sẽ gom nhóm nhiều hàng thuộc về 1 sản phẩm
		/// </summary>
		private async Task<List<ProductImportItem>> ExtractProductDataWithImages(Stream excelStream)
		{

			var result = new List<ProductImportItem>();
			ProductImportItem currentProduct = null;

			using (var package = new ExcelPackage(excelStream)) // ✅ Đúng
			{
				var worksheet = package.Workbook.Worksheets[0];
				var rowCount = worksheet.Dimension?.Rows ?? 0;

				// 1. Extract tất cả ảnh nhúng và map chúng vào (hàng, cột)
				var imagesMap = new Dictionary<(int Row, int Col), List<byte[]>>();
				if (worksheet.Drawings != null)
				{
					foreach (var drawing in worksheet.Drawings.OfType<ExcelPicture>())
					{
						int row = drawing.From.Row;
						int col = drawing.From.Column;

						var imageBytes = drawing.Image.ImageBytes;

						if (imageBytes != null && imageBytes.Length > 0)
						{
							if (!imagesMap.ContainsKey((row, col)))
							{
								imagesMap[(row, col)] = new List<byte[]>();
							}
							imagesMap[(row, col)].Add(imageBytes);
						}
					}
				}

				// 2. Đọc từng hàng (bắt đầu từ hàng 2)
				for (int row = 2; row <= rowCount; row++)
				{
					// Lấy giá trị thông tin sản phẩm
					var name = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
					var categoryName = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

					// Lấy giá trị thông tin biến thể
					var color = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
					var ram = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
					var storage = worksheet.Cells[row, 10].Value?.ToString()?.Trim();

					// --- LOGIC MỚI: TỔNG HỢP ẢNH (CHỈ TỪ CỘT M) ---
					var currentImages = new List<byte[]>();
					int imageColumn = 13; // Cột M

					// A. Lấy ảnh "Place over Cells" từ map (chỉ cột M)
					if (imagesMap.TryGetValue((row, imageColumn), out var imagesOverCell))
					{
						currentImages.AddRange(imagesOverCell);
					}
					// B. Lấy ảnh "Place in Cell" từ giá trị ô (chỉ cột M)
					if (worksheet.Cells[row, imageColumn].Value is OfficeOpenXml.ExcelImage imageInCell)
					{
						currentImages.Add(imageInCell.ImageBytes);
					}
					// --- KẾT THÚC LOGIC LẤY ẢNH ---


					// Kịch bản 1: Hàng định nghĩa SẢN PHẨM MỚI (Cột A và C không rỗng)
					// (Giả định hàng này KHÔNG chứa biến thể)
					if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(categoryName))
					{
						currentProduct = new ProductImportItem
						{
							Name = name,
							Description = worksheet.Cells[row, 2].Value?.ToString()?.Trim(),
							CategoryName = categoryName,
							Screen = worksheet.Cells[row, 4].Value?.ToString()?.Trim(),
							Processor = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
							CameraSystem = worksheet.Cells[row, 6].Value?.ToString()?.Trim(),
							Battery = worksheet.Cells[row, 7].Value?.ToString()?.Trim()
						};

						// Gán ảnh (ở cột M) cho Sản phẩm
						if (currentImages.Count > 0)
						{
							currentProduct.MainImages.AddRange(currentImages);
						}

						result.Add(currentProduct);
					}
					// Kịch bản 2: Hàng định nghĩa BIẾN THỂ MỚI (A rỗng, H, I, J không rỗng)
					else if (currentProduct != null && string.IsNullOrEmpty(name) &&
									 !string.IsNullOrEmpty(color) && !string.IsNullOrEmpty(ram) && !string.IsNullOrEmpty(storage))
					{
						var newVariant = new ProductVariantImportItem
						{
							Color = color,
							Ram = ram,
							Storage = storage,
							Price = decimal.Parse(worksheet.Cells[row, 11].Value?.ToString() ?? "0"),
							StockQuantity = int.Parse(worksheet.Cells[row, 12].Value?.ToString() ?? "0")
						};

						// Gán ảnh (ở cột M) cho Biến thể này
						if (currentImages.Count > 0)
						{
							newVariant.Images.AddRange(currentImages);
						}
						currentProduct.Variants.Add(newVariant);
					}
					// Kịch bản 3: Hàng chỉ chứa ẢNH (A, H, I, J đều rỗng, nhưng M có ảnh)
					else if (currentProduct != null && string.IsNullOrEmpty(name) && string.IsNullOrEmpty(color) &&
									 currentImages.Count > 0)
					{
						// Gán ảnh cho BIẾN THỂ CUỐI CÙNG (nếu có)
						if (currentProduct.Variants.Any())
						{
							currentProduct.Variants.Last().Images.AddRange(currentImages);
						}
						// Nếu không có biến thể nào, gán ảnh cho SẢN PHẨM
						else
						{
							currentProduct.MainImages.AddRange(currentImages);
						}
					}
				}
			}

			// Dùng Task.FromResult vì logic đã là đồng bộ
			return await Task.FromResult(result);
		}
	}

	// DTOs (Không đổi)
	public class ProductImportItem
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string CategoryName { get; set; }
		public string Screen { get; set; }
		public string Processor { get; set; }
		public string CameraSystem { get; set; }
		public string Battery { get; set; }
		public List<byte[]> MainImages { get; set; } = new();
		public List<ProductVariantImportItem> Variants { get; set; } = new();
	}

	public class ProductVariantImportItem
	{
		public string Color { get; set; }
		public string Ram { get; set; }
		public string Storage { get; set; }
		public decimal Price { get; set; }
		public int StockQuantity { get; set; }
		public List<byte[]> Images { get; set; } = new();
	}

	public class ImportResult
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; } // Thêm Message để thông báo kết quả
		public int SuccessCount { get; set; }
		public int TotalCount { get; set; }
		public int WarningCount { get; set; }
		public string ErrorMessage { get; set; }
		public string ErrorDetails { get; set; }
		public List<string> Warnings { get; set; } = new();
	}

	// Fake IFormFile (Không đổi)
	public class FakeFormFile : IFormFile
	{
		private readonly Stream _baseStream;
		private readonly string _fileName;

		public FakeFormFile(Stream baseStream, string fileName)
		{
			_baseStream = baseStream;
			_fileName = fileName;
		}

		public string ContentType => GetContentType(_fileName);
		public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{_fileName}\"";
		public IHeaderDictionary Headers => new HeaderDictionary();
		public long Length => _baseStream.Length;
		public string Name => "file";
		public string FileName => _fileName;

		public void CopyTo(Stream target) => _baseStream.CopyTo(target);
		public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
			=> await _baseStream.CopyToAsync(target, cancellationToken);
		public Stream OpenReadStream() => _baseStream;

		private string GetContentType(string fileName)
		{
			var ext = Path.GetExtension(fileName).ToLower();
			return ext switch
			{
				".jpg" or ".jpeg" => "image/jpeg",
				".png" => "image/png",
				".gif" => "image/gif",
				".webp" => "image/webp",
				_ => "application/octet-stream"
			};
		}
	}
}