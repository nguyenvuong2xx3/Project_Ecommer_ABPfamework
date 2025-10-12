using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.UploadFile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
		private readonly IUploadFileAppService _uploadFileAppService;

		public ProductAppService(IRepository<Product> productRepository,
								IRepository<Category> categoryRepository,
								IRepository<ProductImage> productImageRepository,
								IRepository<ProductVariant> productVariantRepository,
								IUploadFileAppService uploadFileAppService,
								IWebHostEnvironment env)
		{
			_productVariantRepository = productVariantRepository;
			_productRepository = productRepository;
			_uploadFileAppService = uploadFileAppService;
			_categoryRepository = categoryRepository;
			_productImageRepository = productImageRepository;
			_env = env;
		}

		#region tạo mới
		public Product CreateProducts(CreateProductDto input)
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

			_productRepository.Insert(product);
			CurrentUnitOfWork.SaveChanges();

			var currentProductId = product.Id;

			// Lấy kết quả trả về
			return product;
		}
		//public void CreateProductVariants(int productId, List<ProductVariant> variants, string productName)
		//{
		//	int sortOrder = 0;

		//	foreach (var variantInput in variants)
		//	{
		//		var variant = new ProductVariant
		//		{
		//			ProductId = productId,
		//			Ram = variantInput.Ram,
		//			Storage = variantInput.Storage,
		//			Color = variantInput.Color,
		//			Price = variantInput.Price,
		//			StockQuantity = variantInput.StockQuantity,
		//			SKU = "fake"
		//		};

		//		_productVariantRepository.Insert(variant);
		//		CurrentUnitOfWork.SaveChanges();

		//		var variantId = variant.Id;

		//		// Upload ảnh
		//		if (variantInput.ImageFiles != null && variantInput.ImageFiles.Any())
		//		{
		//			var imagePaths = _uploadFileAppService.UploadMultipleImagesAsync(
		//							variantInput.ImageFiles, "products/variants");

		//			foreach (var imagePath in imagePaths)
		//			{
		//				var productImage = new ProductImage
		//				{
		//					ProductId = productId,
		//					ProductVariantId = variantId,
		//					ImageUrl = imagePath,
		//					SortOrder = sortOrder++,
		//					AltText = $"{productName} - {variantInput.Color} {variantInput.Storage}"
		//				};
		//				_productImageRepository.Insert(productImage);
		//			}
		//		}
		//	}
		//}
		public void CreateGeneralProductImages(int productId, List<ProductImage> imageInputs, string productName)
		{
			int generalSortOrder = 0;

			foreach (var imageInput in imageInputs)
			{
				if (imageInput.ImageFiles != null && imageInput.ImageFiles.Any())
				{
					foreach (var imageFile in imageInput.ImageFiles)
					{
						var imagePath = _uploadFileAppService.UploadImageAsync(imageFile, "products/general");
						var productImage = new ProductImage
						{
							ProductId = productId,
							ProductVariantId = null,
							ImageUrl = imagePath,
							SortOrder = generalSortOrder++,
							AltText = imageInput.AltText ?? productName
						};
						_productImageRepository.InsertAsync(productImage);
					}
				}
			}
		}
		#endregion
		public async Task<PagedResultDto<Product>> GetAllProduct(SearchProductDto input)
		{
			var query = _productRepository.GetAll()
							.WhereIf(!string.IsNullOrWhiteSpace(input.Name), p => p.Name.Contains(input.Name))
							.WhereIf(!string.IsNullOrWhiteSpace(input.Description), p => p.Description.Contains(input.Description));

			var totalCount = await query.CountAsync();

			var products = await query
					.OrderByDescending(p => p.CreationTime)
					.PageBy(input)
					.ToListAsync();

			// Get product images
			var productIds = products.Select(p => p.Id).ToList();
			var productImages = await _productImageRepository.GetAll()
					.Where(pi => productIds.Contains(pi.ProductId))
					.OrderBy(pi => pi.SortOrder)
					.ToListAsync();

			// Group images by product and take the first one for each product
			var defaultImages = productImages
					.GroupBy(pi => pi.ProductId)
					.ToDictionary(g => g.Key, g => g.FirstOrDefault()?.ImageUrl);

			// Set the default image for each product
			foreach (var product in products)
			{
				product.ImageUrl = defaultImages.ContainsKey(product.Id) ? defaultImages[product.Id] : null;
			}
			return new PagedResultDto<Product>(totalCount, products);
		}

		public async Task<Product> GetProductById(int id)
		{
			if (id <= 0)
				throw new UserFriendlyException("Dữ liệu không được để trống");
			var item = await _productRepository.GetAsync(id);
			item.ImageUrls = _productImageRepository.GetAll().Where(x => x.ProductId == item.Id).Select(ig => ig.ImageUrl).ToList(); // lấy tất cả đường dẫn ảnh để hiển thị
			return item;
		}
		public async Task<Product> EditProduct(Product input)
		{
			var product = _productRepository.FirstOrDefault(input.Id);
			if (product == null)
			{
				throw new UserFriendlyException("Product not found");
			}
			product.Name = input.Name;
			product.Description = input.Description;
			product.CategoryId = input.CategoryId ?? product.CategoryId;
			product.SKU = input.SKU;
			product.Screen = input.Screen;
			product.Processor = input.Processor;
			product.CameraSystem = input.CameraSystem;
			product.Battery = input.Battery;

			_productRepository.Update(product);
			CurrentUnitOfWork.SaveChanges();
			if (input.DeletedImageUrls != null && input.DeletedImageUrls.Any())
			{
				foreach (var item in input.DeletedImageUrls)
				{
					// ảnh cần xóa
					var existingImages = _productImageRepository.FirstOrDefault(x => x.ProductId == input.Id && x.ImageUrl == item);
					 _productImageRepository.Delete(existingImages.Id);
					if (existingImages != null)
					{
						await _uploadFileAppService.RemoveImage(item);
					}
				}
			}
			if (input.Images != null && input.Images.Any())
			{
				int generalSortOrder = 0;

				foreach (var imageInput in input.Images)
				{
					var imagePath = _uploadFileAppService.UploadImageAsync(imageInput, "products/general");
					var productImage = new ProductImage
					{
						ProductId = input.Id,
						ProductVariantId = null,
						ImageUrl = imagePath,
						SortOrder = generalSortOrder++,
						AltText = input.Name
					};
					_productImageRepository.Insert(productImage);
				}
			}
			return product;
		}

		public async Task<Product> DeleteProduct(int id)
		{
			var item = await _productRepository.GetAsync(id);
			if (item == null)
			{
				throw new UserFriendlyException("Product not found");
			}
			_productRepository.Delete(item);

			// Xóa ảnh liên quan
			_productImageRepository.GetAll().Where(x => x.ProductId == id).ToList().ForEach(img =>
			{
				_uploadFileAppService.RemoveImage(img.ImageUrl);
			});
			_productImageRepository.Delete(x => x.ProductId == id);
			CurrentUnitOfWork.SaveChanges();
			return item;
		}
	}
}