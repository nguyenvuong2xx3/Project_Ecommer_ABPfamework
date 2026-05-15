using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization;
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
	[AbpAuthorize]
	public class ProductAppService : ApplicationService, IProductAppService
	{
		private readonly IRepository<CartItem, int> _cartItemRepository;
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<ProductVariant> _productVariantRepository;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<ProductImage> _productImageRepository;
		private readonly IWebHostEnvironment _env;
		private readonly IUploadFileAppService _uploadFileAppService;

		public ProductAppService(IRepository<Product> productRepository,
								IRepository<CartItem, int> cartItemRepository,
								IRepository<Category> categoryRepository,
								IRepository<ProductImage> productImageRepository,
								IRepository<ProductVariant> productVariantRepository,
								IUploadFileAppService uploadFileAppService,
								IWebHostEnvironment env)
		{
			_cartItemRepository = cartItemRepository;
			_productVariantRepository = productVariantRepository;
			_productRepository = productRepository;
			_uploadFileAppService = uploadFileAppService;
			_categoryRepository = categoryRepository;
			_productImageRepository = productImageRepository;
			_env = env;
		}

		#region tạo mới
		[AbpAuthorize(PermissionNames.Pages_Products_Create)]
		public Product CreateProducts(CreateProductDto input)
		{

			// tên sản phẩm không được trùng
			var existingProduct = _productRepository.FirstOrDefault(p => p.Name == input.Name);
			if (existingProduct != null)
			{
				throw new UserFriendlyException("Tên sản phẩm đã tồn tại. Vui lòng nhập tên khác");
			}

			// Tạo mới sản phẩm
			var product = new Product
			{
				Name = input.Name,
				Description = input.Description,
				CategoryId = input.CategoryId ?? 0,
				
				// Thông số cho điện thoại
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
		public async Task CreateGeneralProductImages(int productId, List<ProductImage> imageInputs, string productName)
		{
			if (imageInputs == null || imageInputs.Count() < 0)
				return;
			int generalSortOrder = 0;

			foreach (var imageInput in imageInputs)
			{
				if (imageInput.ImageFiles != null && imageInput.ImageFiles.Any())
				{
					foreach (var imageFile in imageInput.ImageFiles)
					{
						var imagePath = await _uploadFileAppService.UploadImageAsync(imageFile, "products/general");
						var productImage = new ProductImage
						{
							ProductId = productId,
							ProductVariantId = null,
							ImageUrl = imagePath,
							SortOrder = generalSortOrder++,
							AltText = imageInput.AltText ?? productName
						};
						await _productImageRepository.InsertAsync(productImage);
					}
				}
			}
		}
		#endregion

		[AbpAuthorize(PermissionNames.Pages_Products_View)]
		public async Task<PagedResultDto<Product>> GetAllProduct(SearchProductDto input)
		{
			var query = _productRepository.GetAll()
			.WhereIf(!string.IsNullOrWhiteSpace(input.SearchTerm), p =>
			p.Name.Contains(input.SearchTerm))
			.WhereIf(!string.IsNullOrWhiteSpace(input.Name), p => p.Name.Contains(input.Name))
			.WhereIf(!string.IsNullOrWhiteSpace(input.Description), p => p.Description.Contains(input.Description))
			.WhereIf(!string.IsNullOrWhiteSpace(input.Screen), p => p.Screen.Contains(input.Screen))
			.WhereIf(!string.IsNullOrWhiteSpace(input.Processor), p => p.Processor.Contains(input.Processor))
			.WhereIf(!string.IsNullOrWhiteSpace(input.CameraSystem), p => p.CameraSystem.Contains(input.CameraSystem))
			.WhereIf(!string.IsNullOrWhiteSpace(input.Battery), p => p.Battery.Contains(input.Battery))
			.WhereIf(input.CategoryId.HasValue, p => p.CategoryId == input.CategoryId)
			.WhereIf(input.StockQuantityFrom.HasValue, p => p.StockQuantity >= input.StockQuantityFrom.Value)
			.WhereIf(input.StockQuantityTo.HasValue, p => p.StockQuantity <= input.StockQuantityTo.Value)
			.WhereIf(input.StartTime.HasValue, p => p.CreationTime >= input.StartTime.Value)
			.WhereIf(input.EndTime.HasValue, p => p.CreationTime <= input.EndTime.Value);

			var totalCount = await query.CountAsync();

			var products = await query
			.OrderByDescending(p => p.CreationTime)
			.PageBy(input)
			.ToListAsync();

			// Get product images
			var productIds = products.Select(p => p.Id).ToList();
			var productImages = await _productImageRepository.GetAll()
					.Where(pi => productIds.Contains(pi.ProductId) && pi.ProductVariantId == null)
					.OrderBy(pi => pi.SortOrder)
					.ToListAsync();

			// Group images by product and take the first one for each product
			var defaultImages = productImages
			.GroupBy(pi => pi.ProductId)
			.ToDictionary(g => g.Key, g => g.FirstOrDefault()?.ImageUrl);

			// Set the default image and category name for each product
			foreach (var product in products)
			{
				product.ImageUrl = defaultImages.ContainsKey(product.Id) ? defaultImages[product.Id] : null;
			}

			return new PagedResultDto<Product>(totalCount, products);
		}

		[AbpAuthorize(PermissionNames.Pages_Products_View)]
		public async Task<Product> GetProductById(int id)
		{
			if (id <= 0)
				throw new UserFriendlyException("Dữ liệu không được để trống");
			var item = await _productRepository.GetAsync(id);
			item.ImageUrls = _productImageRepository.GetAll().Where(x => x.ProductId == item.Id && x.ProductVariantId == null).Select(ig => ig.ImageUrl).ToList(); // lấy tất cả đường dẫn ảnh để hiển thị
			return item;
		}

		[AbpAuthorize(PermissionNames.Pages_Products_Update)]
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
			// Thông số cho điện thoại
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
					var imagePath = await _uploadFileAppService.UploadImageAsync(imageInput, "products/general");
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

		[AbpAuthorize(PermissionNames.Pages_Products_Delete)]
		public async Task<Product> DeleteProduct(int id)
		{
			var item = await _productRepository.GetAsync(id);
			// validate nếu cart có sản phẩm được thêm vào giỏ hàng thì không được xóa
			var cartItem = _cartItemRepository.GetAll();
			var query = from ci in cartItem
									join pv in _productVariantRepository.GetAll() on ci.ProductVariantId equals pv.Id
									join p in _productRepository.GetAll() on pv.ProductId equals p.Id
									where pv.ProductId == id
									select p;

			if (query.Any())
			{
				throw new UserFriendlyException("Không thể xóa sản phẩm này vì đã được người dùng thêm vào giỏ hàng");
			}


			if (item == null)
			{
				throw new UserFriendlyException("Product not found");
			}
			// nếu gắn các biến thể thì không cho xóa
			var variants = _productVariantRepository.GetAll().Where(x => x.ProductId == id).ToList();
			if (variants.Count > 0)
			{
				throw new UserFriendlyException("Không thể xóa sản phẩm này vì có các biến thể liên quan. Vui lòng xóa các biến thể trước.");
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

		public async Task<List<ProductListDto>> GetAllProductsForSelect()
		{
			var products = await _productRepository.GetAll()
			.OrderBy(p => p.Name)
			.ToListAsync();

			return products.Select(p => new ProductListDto
			{
				Id = p.Id,
				Name = p.Name,
				Description = p.Description
			}).ToList();
		}
	}
}