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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
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

			try
			{
				using (var stream = excelFile.OpenReadStream())
				{
					// 1. Extract product data và images từ Excel (Đã cập nhật logic)
					var productDataList = await ExtractProductDataWithImages(stream);
					if (productDataList == null || productDataList.Count == 0)
					{
						result.IsSuccess = false;
						result.ErrorMessage = "File Excel không chứa dữ liệu hoặc định dạng không hợp lệ";
						return result;
					}

					// 2. Loop qua từng product
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

							// 3. Import ảnh sản phẩm (Col M) - liên kết với ProductId
							// Xóa ảnh cũ (nếu update)
							if (existingProduct != null)
							{
								await _productImageRepository.DeleteAsync(pi => pi.ProductId == product.Id && pi.ProductVariantId == null);
							}
							// Lưu ảnh mới
							await SaveAndLinkImagesAsync(product.Id, null, productData.MainImages, product.Name);

							// 4. Import variants
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

										// 5. Import ảnh của variant (Col N) - liên kết với VariantId
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
			}
			catch (Exception ex)
			{
				result.IsSuccess = false;
				result.ErrorMessage = $"Lỗi nghiêm trọng khi import file Excel: {ex.Message}";
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
					// Lấy giá trị các cột chính
					var name = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
					var categoryName = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
					var color = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
					var ram = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
					var storage = worksheet.Cells[row, 10].Value?.ToString()?.Trim();

					// Lấy ảnh từ các ô
					imagesMap.TryGetValue((row, 13), out var productImages);
					imagesMap.TryGetValue((row, 14), out var variantImages);

					// Kịch bản 1: Hàng định nghĩa SẢN PHẨM MỚI (Cột A và C không rỗng)
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

						// Lấy ảnh sản phẩm (Col M) ngay trên hàng này (nếu có)
						if (productImages != null)
						{
							currentProduct.MainImages.AddRange(productImages);
						}

						result.Add(currentProduct);

						// Hàng này CŨNG PHẢI định nghĩa BIẾN THỂ ĐẦU TIÊN
						if (string.IsNullOrEmpty(color) || string.IsNullOrEmpty(ram) || string.IsNullOrEmpty(storage))
						{
							// Bỏ qua sản phẩm này nếu biến thể đầu tiên không hợp lệ
							currentProduct = null;
							result.Remove(result.Last());
							// Ghi log lỗi? (Bỏ qua trong ví dụ này, logic chính sẽ bắt)
							continue;
						}

						var firstVariant = new ProductVariantImportItem
						{
							Color = color,
							Ram = ram,
							Storage = storage,
							Price = decimal.Parse(worksheet.Cells[row, 11].Value?.ToString() ?? "0"),
							StockQuantity = int.Parse(worksheet.Cells[row, 12].Value?.ToString() ?? "0")
						};

						// Lấy ảnh biến thể (Col N) ngay trên hàng này (nếu có)
						if (variantImages != null)
						{
							firstVariant.Images.AddRange(variantImages);
						}
						currentProduct.Variants.Add(firstVariant);
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

						// Lấy ảnh biến thể (Col N) (nếu có)
						if (variantImages != null)
						{
							newVariant.Images.AddRange(variantImages);
						}
						currentProduct.Variants.Add(newVariant);
					}
					// Kịch bản 3: Hàng chỉ chứa ẢNH SẢN PHẨM (A rỗng, H rỗng, M có ảnh)
					else if (currentProduct != null && string.IsNullOrEmpty(name) && string.IsNullOrEmpty(color) &&
							 productImages != null && productImages.Count > 0)
					{
						currentProduct.MainImages.AddRange(productImages);
					}
					// Kịch bản 4: Hàng chỉ chứa ẢNH BIẾN THỂ (A rỗng, H rỗng, N có ảnh)
					// (Logic này giả định ảnh ở Col N thuộc về biến thể cuối cùng được thêm vào)
					else if (currentProduct != null && string.IsNullOrEmpty(name) && string.IsNullOrEmpty(color) &&
							 variantImages != null && variantImages.Count > 0 && currentProduct.Variants.Any())
					{
						currentProduct.Variants.Last().Images.AddRange(variantImages);
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