using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Products.Dtos;
using MailKit.Search;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Products
{
	//[AbpAuthorize]
	public class ProductAppService : ApplicationService, IProductAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<ProductVariant> _productVariantRepository;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<ProductImage> _productImageRepository;
		private readonly IWebHostEnvironment _env;

		public ProductAppService(IRepository<Product> productRepository,
								IRepository<Category> categoryRepository,
								IRepository<ProductImage> productImageRepository,
								IRepository<ProductVariant> productVariantRepository,
								IWebHostEnvironment env)
		{
			_productVariantRepository = productVariantRepository;
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
			_productImageRepository = productImageRepository;
			_env = env;
		}

		/// <summary>
		/// Hàm helper upload ảnh với mã hóa đường dẫn để tránh trùng
		/// </summary>
		/// <param name="file">File ảnh cần upload</param>
		/// <param name="subFolder">Thư mục con để lưu ảnh (mặc định là "products")</param>
		/// <returns>Đường dẫn tương đối của ảnh đã upload</returns>
		private async Task<string> UploadImageAsync(IFormFile file, string subFolder = "products")
		{
			if (file == null || file.Length == 0)
			{
				return null;
			}

			// Kiểm tra định dạng ảnh hợp lệ
			string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".jfif" };
			string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

			if (!allowedExtensions.Contains(fileExtension))
			{
				throw new Exception($"Định dạng file không hợp lệ. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}");
			}

			// Kiểm tra kích thước file (giới hạn 5MB)
			if (file.Length > 5 * 1024 * 1024)
			{
				throw new Exception("Kích thước file vượt quá 5MB");
			}

			// Tạo đường dẫn thư mục lưu trữ
			string uploadsFolder = Path.Combine(_env.WebRootPath, "img", subFolder);
			Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có

			// Tạo tên file unique với mã hóa:
			// Format: {timestamp}_{guid}_{original-filename-hash}{extension}
			// Ví dụ: 20231225103045_a1b2c3d4_abc123.jpg
			string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			string uniqueGuid = Guid.NewGuid().ToString("N").Substring(0, 8); // Lấy 8 ký tự đầu

			// Tạo hash từ tên file gốc để tránh trùng lặp nhưng vẫn có thể trace back
			string originalFileNameHash = GetFileNameHash(Path.GetFileNameWithoutExtension(file.FileName));

			string uniqueFileName = $"{timestamp}_{uniqueGuid}_{originalFileNameHash}{fileExtension}";
			string filePath = Path.Combine(uploadsFolder, uniqueFileName);

			// Lưu file vào đĩa
			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(fileStream);
			}

			// Trả về đường dẫn tương đối (dùng cho web)
			return $"/img/{subFolder}/{uniqueFileName}";
		}

		/// <summary>
		/// Tạo hash ngắn gọn từ tên file để tránh trùng lặp
		/// </summary>
		private string GetFileNameHash(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				return "noname";
			}

			// Chỉ lấy các ký tự alphanumeric
			string cleaned = new string(fileName.Where(c => char.IsLetterOrDigit(c)).ToArray());

			if (cleaned.Length > 10)
			{
				cleaned = cleaned.Substring(0, 10);
			}

			// Tạo hash code từ chuỗi
			int hashCode = fileName.GetHashCode();
			string hash = Math.Abs(hashCode).ToString("X").Substring(0, Math.Min(6, Math.Abs(hashCode).ToString("X").Length));

			return $"{cleaned}_{hash}".ToLower();
		}

		/// <summary>
		/// Upload nhiều ảnh cùng lúc
		/// </summary>
		/// <param name="files">Danh sách file ảnh</param>
		/// <param name="subFolder">Thư mục con</param>
		/// <returns>Danh sách đường dẫn ảnh đã upload</returns>
		private async Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> files, string subFolder = "products")
		{
			var uploadedPaths = new List<string>();

			if (files == null || !files.Any())
			{
				return uploadedPaths;
			}

			foreach (var file in files)
			{
				try
				{
					var path = await UploadImageAsync(file, subFolder);
					if (!string.IsNullOrEmpty(path))
					{
						uploadedPaths.Add(path);
					}
				}
				catch (Exception ex)
				{
					// Log lỗi nhưng vẫn tiếp tục upload các file khác
					Logger.Error($"Lỗi khi upload file {file.FileName}: {ex.Message}");
				}
			}

			return uploadedPaths;
		}

		public async Task<ProductListDto> CreateProducts(CreateProductDto input)
		{
			// Tạo mới sản phẩm
			var product = new Product
			{
				Name = input.Name,
				Description = input.Description,
				CategoryId = input.CategoryId ?? 0,
				SKU = input.SKU,
				Screen = input.Screen,
				Processor = input.Processor,
				CameraSystem = input.CameraSystem,
				Battery = input.Battery,
				StockQuantity = input.ProductVariants?.Sum(v => v.StockQuantity) ?? 0,
			};

			var currentProductId = await _productRepository.InsertAndGetIdAsync(product);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Lưu biến thể và ảnh của biến thể
			if (input.ProductVariants != null && input.ProductVariants.Any())
			{
				int sortOrder = 0;
				foreach (var variantInput in input.ProductVariants)
				{
					// Tạo ProductVariant entity
					var variant = new ProductVariant
					{
						ProductId = currentProductId,
						Ram = variantInput.Ram,
						Storage = variantInput.Storage,
						Color = variantInput.Color,
						Price = variantInput.Price,
						StockQuantity = variantInput.StockQuantity,
						SKU = variantInput.SKU ?? GenerateVariantSKU(product.SKU, variantInput)
					};

					var variantId = await _productVariantRepository.InsertAndGetIdAsync(variant);
					await CurrentUnitOfWork.SaveChangesAsync();

					// Upload và lưu ảnh của biến thể
					if (variantInput.ImageFiles != null && variantInput.ImageFiles.Any())
					{
						var imagePaths = await UploadMultipleImagesAsync(variantInput.ImageFiles, "products/variants");

						foreach (var imagePath in imagePaths)
						{
							var productImage = new ProductImage
							{
								ProductId = currentProductId,
								ProductVariantId = variantId,
								ImageUrl = imagePath,
								SortOrder = sortOrder++,
								AltText = $"{product.Name} - {variantInput.Color} {variantInput.Storage}"
							};
							await _productImageRepository.InsertAsync(productImage);
						}
					}
				}
			}

			// Lưu ảnh chung của sản phẩm
			if (input.ProductImages != null && input.ProductImages.Any())
			{
				int generalSortOrder = 0;
				foreach (var imageInput in input.ProductImages)
				{
					// Nếu ProductImage có ImageFiles (từ form upload)
					if (imageInput.ImageFiles != null && imageInput.ImageFiles.Any())
					{
						var imagePaths = await UploadMultipleImagesAsync(imageInput.ImageFiles, "products/general");

						foreach (var imagePath in imagePaths)
						{
							var productImage = new ProductImage
							{
								ProductId = currentProductId,
								ProductVariantId = null, // Ảnh chung không liên kết với biến thể cụ thể
								ImageUrl = imagePath,
								SortOrder = generalSortOrder++,
								AltText = imageInput.AltText ?? product.Name
							};
							await _productImageRepository.InsertAsync(productImage);
						}
					}
				}
			}

			await CurrentUnitOfWork.SaveChangesAsync();

			// Lấy ảnh đầu tiên để hiển thị
			var firstImage = await _productImageRepository.FirstOrDefaultAsync(
				x => x.ProductId == currentProductId
			);

			return new ProductListDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				CreationTime = product.CreationTime,
				Image = firstImage?.ImageUrl,
				CategoryId = product.CategoryId
			};
		}

		/// <summary>
		/// Tự động sinh SKU cho biến thể dựa trên SKU sản phẩm chính
		/// </summary>
		private string GenerateVariantSKU(string productSKU, ProductVariant variant)
		{
			var parts = new List<string>();

			if (!string.IsNullOrEmpty(productSKU))
			{
				parts.Add(productSKU);
			}
			else
			{
				parts.Add("PRD");
			}

			if (!string.IsNullOrEmpty(variant.Ram))
			{
				parts.Add(variant.Ram.Replace("GB", "").Replace(" ", ""));
			}

			if (!string.IsNullOrEmpty(variant.Storage))
			{
				parts.Add(variant.Storage.Replace("GB", "").Replace("TB", "T").Replace(" ", ""));
			}

			if (!string.IsNullOrEmpty(variant.Color))
			{
				// Lấy 3 ký tự đầu của màu
				string colorCode = new string(variant.Color.Take(3).ToArray()).ToUpper();
				parts.Add(colorCode);
			}

			return string.Join("-", parts);
		}

		public Task DeleteProducts(EntityDto<int> input)
		{
			throw new NotImplementedException();
		}

		public Task<byte[]> ExportProductsToExcel(GetAllProductsInput input)
		{
			throw new NotImplementedException();
		}

		public Task<PagedResultDto<ProductListDto>> GetAllProducts(GetAllProductsInput input)
		{
			throw new NotImplementedException();
		}

		public Task<ProductListDto> GetByIdProducts(EntityDto<int> input)
		{
			throw new NotImplementedException();
		}

		public Task<List<ImportProductResultDto>> ImportProductsFromExcel(IFormFile file)
		{
			throw new NotImplementedException();
		}

		public Task<PagedResultDto<ProductListDto>> SearchProducts(GetAllProductsInput input)
		{
			throw new NotImplementedException();
		}

		public Task<ProductListDto> UpdateProducts(UpdateProductDto input)
		{
			throw new NotImplementedException();
		}
	}
}
