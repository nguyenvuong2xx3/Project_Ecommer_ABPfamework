using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.UploadFile;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductImport
{
	public interface IProductImportAppService : IApplicationService
	{
		Task<ImportResult> ImportProductsWithImagesAsync(Stream excelStream);
	}

	public class ProductImportAppService : ApplicationService, IProductImportAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<ProductVariant> _productVariantRepository;
		private readonly IRepository<ProductImage> _productImageRepository;
		private readonly IUploadFileAppService _uploadFileAppService;

		public ProductImportAppService(
			IRepository<Product> productRepository,
			IRepository<ProductVariant> productVariantRepository,
			IRepository<ProductImage> productImageRepository,
			IUploadFileAppService uploadFileAppService)
		{
			_productRepository = productRepository;
			_productVariantRepository = productVariantRepository;
			_productImageRepository = productImageRepository;
			_uploadFileAppService = uploadFileAppService;
		}

		[UnitOfWork]
		public async Task<ImportResult> ImportProductsWithImagesAsync(Stream excelStream)
		{
			// Set EPPlus license context
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

			var result = new ImportResult();
			var errorDetails = new List<string>();
			int successCount = 0;
			int rowNum = 0;

			try
			{
				// 1. Extract product data và images từ Excel
				var productDataList = await ExtractProductDataWithImages(excelStream);

				if (productDataList == null || productDataList.Count == 0)
				{
					result.IsSuccess = false;
					result.ErrorMessage = "File Excel không chứa dữ liệu hoặc định dạng không hợp lệ";
					return result;
				}

				// 2. Loop qua từng product
				foreach (var productData in productDataList)
				{
					rowNum++;
					try
					{
						// Validate dữ liệu cơ bản
						if (string.IsNullOrWhiteSpace(productData.Name))
						{
							errorDetails.Add($"Hàng {rowNum + 1}: Tên sản phẩm không được để trống");
							continue;
						}

						// Kiểm tra xem product đã tồn tại chưa (theo Name)
						var existingProduct = await _productRepository.FirstOrDefaultAsync(p => p.Name == productData.Name);

						Product product;

						if (existingProduct != null)
						{
							// Update product hiện có
							existingProduct.Name = productData.Name;
							existingProduct.Description = productData.Description;
							existingProduct.CategoryId = productData.CategoryId;
							
							product = await _productRepository.UpdateAsync(existingProduct);
						}
						else
						{
							// Tạo product mới
							product = new Product
							{
								Name = productData.Name,
								Description = productData.Description,
								CategoryId = productData.CategoryId,
								CreationTime = DateTime.Now
							};

							product = await _productRepository.InsertAsync(product);
						}

						// 3. Import variants
						if (productData.Variants != null && productData.Variants.Count > 0)
						{
							int variantIndex = 0;
							foreach (var variantData in productData.Variants)
							{
								try
								{
									variantIndex++;

									// Kiểm tra variant đã tồn tại - dùng Color, Storage, Ram để định danh
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

									// 4. Import ảnh của variant
									await ImportVariantImagesAsync(
										variant.Id, 
										product.Id, 
										variantData.Images, 
										variantData.ImageFileNames);

									successCount++;
								}
								catch (Exception ex)
								{
									errorDetails.Add(
										$"Hàng {rowNum + 1}, Variant {variantIndex}: {ex.Message}");
								}
							}
						}
						else
						{
							// Nếu không có variant, tạo 1 variant mặc định với ảnh của product
							try
							{
								var defaultVariant = new ProductVariant
								{
									ProductId = product.Id,
									Color = "Mặc định",
									Storage = "Không xác định",
									Ram = "0GB",
									Price = productData.Price,
									StockQuantity = productData.StockQuantity,
									CreationTime = DateTime.Now
								};

								defaultVariant = await _productVariantRepository.InsertAsync(defaultVariant);

								// Import ảnh chính
								await ImportVariantImagesAsync(
									defaultVariant.Id, 
									product.Id, 
									productData.MainImages, 
									productData.MainImageFileNames);

								successCount++;
							}
							catch (Exception ex)
							{
								errorDetails.Add($"Hàng {rowNum + 1}, Variant mặc định: {ex.Message}");
							}
						}
					}
					catch (Exception ex)
					{
						errorDetails.Add($"Hàng {rowNum + 1}: {ex.Message}");
					}
				}

				await CurrentUnitOfWork.SaveChangesAsync();

				result.IsSuccess = true;
				result.SuccessCount = successCount;
				result.TotalCount = productDataList.Count;

				if (errorDetails.Count > 0)
				{
					result.Warnings = errorDetails;
					result.WarningCount = errorDetails.Count;
				}
			}
			catch (Exception ex)
			{
				result.IsSuccess = false;
				result.ErrorMessage = $"Lỗi khi import file Excel: {ex.Message}";
				result.ErrorDetails = ex.StackTrace;
			}

			return result;
		}

		/// <summary>
		/// Import ảnh cho variant
		/// </summary>
		private async Task ImportVariantImagesAsync(
			int variantId, 
			int productId, 
			List<byte[]> images, 
			List<string> imageFileNames)
		{
			if (images == null || images.Count == 0)
			{
				// Nếu không có ảnh, thêm ảnh mặc định
				var defaultImage = new ProductImage
				{
					ProductVariantId = variantId,
					ProductId = productId,
					ImageUrl = "/img/products/default.png",
					SortOrder = 0,
					AltText = "Ảnh mặc định"
				};
				await _productImageRepository.InsertAsync(defaultImage);
				return;
			}

			// Lưu tất cả ảnh
			for (int i = 0; i < images.Count; i++)
			{
				try
				{
					var imageBytes = images[i];
					var imageName = imageFileNames != null && i < imageFileNames.Count 
						? imageFileNames[i] 
						: $"image_{i}.jpg";

					// Lưu ảnh vào server
					var imageUrl = await SaveImageToServerAsync(imageBytes, imageName);

					// Tạo record ProductImage
					var productImage = new ProductImage
					{
						ProductVariantId = variantId,
						ProductId = productId,
						ImageUrl = imageUrl,
						SortOrder = i,
						AltText = imageName
					};

					await _productImageRepository.InsertAsync(productImage);
				}
				catch (Exception ex)
				{
					throw new UserFriendlyException(
						$"Lỗi khi lưu ảnh {imageFileNames?[i] ?? "unknown"}: {ex.Message}");
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
					// Tạo fake IFormFile để compatible với IUploadFileAppService
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
		/// Extract product data từ Excel file kèm ảnh nhúng
		/// </summary>
		private async Task<List<ProductImportItem>> ExtractProductDataWithImages(Stream excelStream)
		{
			var result = new List<ProductImportItem>();

			try
			{
				// Copy stream vì phải dùng 2 lần
				var memoryStream = new MemoryStream();
				await excelStream.CopyToAsync(memoryStream);
				memoryStream.Position = 0;

				// Extract ảnh từ xlsx
				var imagesMap = ExtractImagesFromExcel(memoryStream);

				// Đọc Excel data
				memoryStream.Position = 0;
				using (var package = new ExcelPackage(memoryStream))
				{
					var worksheet = package.Workbook.Worksheets[0];
					var rowCount = worksheet.Dimension?.Rows ?? 0;

					if (rowCount < 2)
					{
						throw new UserFriendlyException("File Excel không có dữ liệu (phải có ít nhất 2 hàng: header + data)");
					}

					// Column mapping:
					// A=Name, B=Description, C=Price, D=StockQuantity, E=CategoryId
					// F=VariantColor, G=VariantRam, H=VariantStorage, I=VariantPrice, J=VariantStockQuantity
					// K=Images (comma-separated: image1.jpg,image2.jpg,...)

					ProductImportItem currentProduct = null;

					for (int row = 2; row <= rowCount; row++)
					{
						try
						{
							var name = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
							var description = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
							var priceStr = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
							var stockStr = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
							var categoryIdStr = worksheet.Cells[row, 5].Value?.ToString()?.Trim();

							var variantColor = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
							var variantRam = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
							var variantStorage = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
							var variantPriceStr = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
							var variantStockStr = worksheet.Cells[row, 10].Value?.ToString()?.Trim();
							var imagesStr = worksheet.Cells[row, 11].Value?.ToString()?.Trim();

							// Nếu hàng này là product mới (có name)
							if (!string.IsNullOrEmpty(name))
							{
								decimal.TryParse(priceStr, out var price);
								int.TryParse(stockStr, out var stock);
								int.TryParse(categoryIdStr, out var categoryId);

								currentProduct = new ProductImportItem
								{
									Name = name,
									Description = description,
									Price = price,
									StockQuantity = stock,
									CategoryId = categoryId > 0 ? categoryId : null,
									Variants = new List<ProductVariantImportItem>()
								};

								// Thêm ảnh chính của product
								if (!string.IsNullOrEmpty(imagesStr))
								{
									var imageNames = imagesStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
										.Select(x => x.Trim())
										.ToList();

									foreach (var imageName in imageNames)
									{
										if (imagesMap.ContainsKey(imageName))
										{
											currentProduct.MainImages.Add(imagesMap[imageName]);
											currentProduct.MainImageFileNames.Add(imageName);
										}
									}
								}

								result.Add(currentProduct);
							}

							// Nếu có thông tin variant
							if (!string.IsNullOrEmpty(variantColor) && currentProduct != null)
							{
								decimal.TryParse(variantPriceStr, out var variantPrice);
								int.TryParse(variantStockStr, out var variantStock);

								var variant = new ProductVariantImportItem
								{
									Color = variantColor ?? "Không xác định",
									Ram = variantRam ?? "0GB",
									Storage = variantStorage ?? "Không xác định",
									Price = variantPrice > 0 ? variantPrice : currentProduct.Price,
									StockQuantity = variantStock > 0 ? variantStock : currentProduct.StockQuantity,
									Images = new List<byte[]>(),
									ImageFileNames = new List<string>()
								};

								// Thêm ảnh của variant
								if (!string.IsNullOrEmpty(imagesStr))
								{
									var imageNames = imagesStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
										.Select(x => x.Trim())
										.ToList();

									foreach (var imageName in imageNames)
									{
										if (imagesMap.ContainsKey(imageName))
										{
											variant.Images.Add(imagesMap[imageName]);
											variant.ImageFileNames.Add(imageName);
										}
									}
								}

								currentProduct.Variants.Add(variant);
							}
						}
						catch (Exception ex)
						{
							// Log row error nhưng tiếp tục xử lý
							System.Diagnostics.Debug.WriteLine($"Lỗi xử lý hàng {row}: {ex.Message}");
						}
					}
				}

				memoryStream.Dispose();
			}
			catch (Exception ex)
			{
				throw new UserFriendlyException($"Lỗi khi đọc file Excel: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Extract tất cả images từ /xl/media/ trong xlsx file
		/// </summary>
		private Dictionary<string, byte[]> ExtractImagesFromExcel(Stream excelStream)
		{
			var imagesMap = new Dictionary<string, byte[]>();

			try
			{
				using (var zip = new ZipArchive(excelStream, ZipArchiveMode.Read, leaveOpen: true))
				{
					var mediaEntries = zip.Entries
						.Where(e => e.FullName.StartsWith("xl/media/") && !e.Name.StartsWith("."))
						.ToList();

					foreach (var entry in mediaEntries)
					{
						using (var stream = entry.Open())
						{
							var buffer = new byte[entry.Length];
							stream.Read(buffer, 0, (int)entry.Length);
							imagesMap[entry.Name] = buffer;
						}
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Lỗi extract images: {ex.Message}");
			}

			return imagesMap;
		}
	}

	// DTOs
	public class ProductImportItem
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public int StockQuantity { get; set; }
		public int? CategoryId { get; set; }

		public List<byte[]> MainImages { get; set; } = new();
		public List<string> MainImageFileNames { get; set; } = new();
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
		public List<string> ImageFileNames { get; set; } = new();
	}

	public class ImportResult
	{
		public bool IsSuccess { get; set; }
		public int SuccessCount { get; set; }
		public int TotalCount { get; set; }
		public int WarningCount { get; set; }
		public string ErrorMessage { get; set; }
		public string ErrorDetails { get; set; }
		public List<string> Warnings { get; set; } = new();
	}

	/// <summary>
	/// Fake IFormFile để convert byte array sang IFormFile
	/// </summary>
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