using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.ProductVariants.Dtos;
using Acme.SimpleTaskApp.UploadFile;
using MailKit.Search;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductVariants
{
	public class ProductVariantAppService : ApplicationService, IProductVariantAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<ProductVariant> _productVariantRepository;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<ProductImage> _productImageRepository;
		private readonly IWebHostEnvironment _env;
		private readonly IUploadFileAppService _uploadFileAppService;

		public ProductVariantAppService(IRepository<Product> productRepository,
								IRepository<Category> categoryRepository,
								IRepository<ProductImage> productImageRepository,
								IRepository<ProductVariant> productVariantRepository,
								IUploadFileAppService uploadFileAppService,
								IWebHostEnvironment env)
		{
			_productRepository = productRepository;
			_productVariantRepository = productVariantRepository;
			_uploadFileAppService = uploadFileAppService;
			_categoryRepository = categoryRepository;
			_productImageRepository = productImageRepository;
			_env = env;
		}
		public async Task CreateProductVariant(ProductVariant input)
		{
			if (input == null)
			{
				throw new UserFriendlyException("Dữ liệu không được để trống");
			}

			await _productVariantRepository.InsertAsync(input);
			CurrentUnitOfWork.SaveChanges();
			if (input.ImageFiles != null && input.ImageFiles.Any())
			{
				int sortOrder = 0;
				foreach (var item in input.ImageFiles)
				{
					var imageUrl = await _uploadFileAppService.UploadImageAsync(item, "products/variants");
					var producImage = new ProductImage
					{
						ImageUrl = imageUrl,
						SortOrder = sortOrder++,
						AltText = item.Name,
						ProductVariantId = input.Id,
						ProductId = input.ProductId,
					};
					await _productImageRepository.InsertAsync(producImage);
				}
			}
			else
			{
				var producImage = new ProductImage
				{
					ImageUrl = "/img/products/default.png",
					SortOrder = 0,
					AltText = "default",
					ProductVariantId = input.Id,
					ProductId = input.ProductId,
				};
				await _productImageRepository.InsertAsync(producImage);
			}
		}
		public async Task EditProductVariant(ProductVariant input)
		{
			if (input == null)
			{
				throw new UserFriendlyException("Dữ liệu không được để trống");
			}
			var get = await _productVariantRepository.GetAsync(input.Id);
			get.SKU = input.SKU;
			get.Ram = input.Ram;
			get.Storage = input.Storage;
			get.Color = input.Color;
			get.Price = input.Price;
			get.StockQuantity = input.StockQuantity;
			get.ProductId = input.ProductId;

			if (get == null)
			{
				throw new UserFriendlyException("Không tìm thấy dữ liệu");
			}
			if (input.DeletedImageUrls != null)
			{
				foreach (var item in input.DeletedImageUrls)
				{
					// ảnh cần xóa
					var existingImages = _productImageRepository.FirstOrDefault(x => x.ProductVariantId == input.Id && x.ImageUrl == item);
					if (existingImages != null)
					{
						_productImageRepository.Delete(existingImages.Id);
						// tạo đường dẫn ảnh mặc định
						//var producImage = new ProductImage
						//{
						//	ImageUrl = "/img/products/default.png",
						//	SortOrder = 0,
						//	AltText = "default",
						//	ProductVariantId = input.Id,
						//	ProductId = input.ProductId,
						//};
						//await _productImageRepository.InsertAsync(producImage);
						await _uploadFileAppService.RemoveImage(item);
					}
				}
			}
			if (input.ImageFiles != null && input.ImageFiles.Any())
			{
				int generalSortOrder = 0;

				foreach (var imageInput in input.ImageFiles)
				{
					var imagePath = await _uploadFileAppService.UploadImageAsync(imageInput, "products/general");
					var productImage = new ProductImage
					{
						ProductId = input.ProductId,
						ProductVariantId = input.Id,
						ImageUrl = imagePath,
						SortOrder = generalSortOrder++,
						AltText = "Ảnh sản phẩm",
					};
					_productImageRepository.Insert(productImage);
				}
			}

			await _productVariantRepository.UpdateAsync(get);
		}

		public async Task DeleteProductVariant(int id)
		{
			var item = await _productVariantRepository.GetAsync(id);
			if (item == null)
			{
				throw new UserFriendlyException("Product not found");
			}
			_productVariantRepository.Delete(item);

			// Xóa ảnh liên quan
			_productImageRepository.GetAll().Where(x => x.ProductVariantId == id).ToList().ForEach(img =>
			{
				_uploadFileAppService.RemoveImage(img.ImageUrl);
			});
			_productImageRepository.Delete(x => x.ProductVariantId == id);
			CurrentUnitOfWork.SaveChanges();
		}

		public async Task<PagedResultDto<ProductVariant>> GetAllProductVariant(GetProductVariantsInput input)
		{
			var query = _productVariantRepository.GetAll();

			// Lọc theo ProductId (nếu có)
			if (input.ProductId.HasValue)
			{
				query = query.Where(v => v.ProductId == input.ProductId.Value);
			}

			// Lọc theo tên sản phẩm (tìm trong bảng Product)
			if (!string.IsNullOrWhiteSpace(input.ProductName))
			{
				var productQuery = _productRepository.GetAll()
						.Where(p => p.Name.Contains(input.ProductName))
						.Select(p => p.Id);

				query = query.Where(v => productQuery.Contains(v.ProductId));
			}

			// Lọc theo RAM
			if (!string.IsNullOrWhiteSpace(input.Ram))
			{
				query = query.Where(v => v.Ram.Contains(input.Ram));
			}

			// Lọc theo Storage
			if (!string.IsNullOrWhiteSpace(input.Storage))
			{
				query = query.Where(v => v.Storage.Contains(input.Storage));
			}

			// Lọc theo Color
			if (!string.IsNullOrWhiteSpace(input.Color))
			{
				query = query.Where(v => v.Color.Contains(input.Color));
			}

			// Lọc theo SKU
			if (!string.IsNullOrWhiteSpace(input.SKU))
			{
				query = query.Where(v => v.SKU.Contains(input.SKU));
			}

			// Lọc theo khoảng giá
			if (input.MinPrice.HasValue)
			{
				query = query.Where(v => v.Price >= input.MinPrice.Value);
			}
			if (input.MaxPrice.HasValue)
			{
				query = query.Where(v => v.Price <= input.MaxPrice.Value);
			}

			// Lọc theo số lượng tồn kho
			if (input.StockQuantity.HasValue)
			{
				query = query.Where(v => v.StockQuantity == input.StockQuantity.Value);
			}

			// Lọc theo khoảng thời gian tạo
			if (input.StartTime.HasValue)
			{
				query = query.Where(v => v.CreationTime >= input.StartTime.Value);
			}
			if (input.EndTime.HasValue)
			{
				query = query.Where(v => v.CreationTime <= input.EndTime.Value);
			}

			// Tìm kiếm tổng hợp (SearchTerm) - tìm trong nhiều trường
			if (!string.IsNullOrWhiteSpace(input.SearchTerm))
			{
				var searchTerm = input.SearchTerm.Trim().ToLower();

				// Tìm trong bảng Product cho tên sản phẩm
				var productSearchQuery = _productRepository.GetAll()
						.Where(p => p.Name.ToLower().Contains(searchTerm))
						.Select(p => p.Id);

				query = query.Where(v =>
						v.Ram.ToLower().Contains(searchTerm) ||
						v.Storage.ToLower().Contains(searchTerm) ||
						v.Color.ToLower().Contains(searchTerm) ||
						v.SKU.ToLower().Contains(searchTerm) ||
						productSearchQuery.Contains(v.ProductId)
				);
			}

			var totalCount = await query.CountAsync();

			var items = await query.OrderByDescending(p => p.CreationTime)
							.PageBy(input)
							.ToListAsync();

			// Get product images
			var productVariantIds = items.Select(p => p.Id).ToList();
			var productImages = await _productImageRepository.GetAll()
							.Where(pi => productVariantIds.Contains(pi.ProductVariantId.Value))
							.OrderBy(pi => pi.SortOrder)
							.ToListAsync();

			// Group images by product and take the first one for each product
			var defaultImages = productImages
							.GroupBy(pi => pi.ProductVariantId)
							.ToDictionary(g => g.Key.Value, g => g.FirstOrDefault()?.ImageUrl);

			// Fetch product names
			var productIds = items.Select(i => i.ProductId).Distinct().ToList();
			var products = await _productRepository.GetAll()
							.Where(p => productIds.Contains(p.Id))
							.ToDictionaryAsync(p => p.Id, p => p.Name);

			var resultItems = items.Select(item =>
			{
				var productName = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null;
				return new ProductVariant
				{
					Id = item.Id,
					ProductId = item.ProductId,
					ProductName = productName,
					Ram = item.Ram,
					Storage = item.Storage,
					Color = item.Color,
					Price = item.Price,
					StockQuantity = item.StockQuantity,
					SKU = item.SKU,
					ImageUrl = defaultImages.ContainsKey(item.Id) ? defaultImages[item.Id] : null,
					CreationTime = item.CreationTime // Đảm bảo có CreationTime
				};
			}).ToList();

			return new PagedResultDto<ProductVariant>(totalCount, resultItems);
		}


		public async Task<ProductVariant> GetById(int id)
		{
			if (id <= 0)
				throw new UserFriendlyException("Dữ liệu không được để trống");

			var item = await _productVariantRepository.GetAsync(id);
			var product = await _productRepository.FirstOrDefaultAsync(x => x.Id == item.ProductId);
			item.ProductName = product?.Name;

			item.ImageUrls = await _productImageRepository.GetAll()
				.Where(x => x.ProductVariantId == item.Id)
				.Select(ig => ig.ImageUrl)
				.ToListAsync();

			return item;
		}

		public async Task<List<ProductVariantListDto>> GetAllProductVariantsForSelect()
		{
			var variants = await _productVariantRepository.GetAll()
				.OrderBy(v => v.SKU)
				.ToListAsync();

			var productIds = variants.Select(v => v.ProductId).Distinct().ToList();
			var products = await _productRepository.GetAll()
				.Where(p => productIds.Contains(p.Id))
				.ToDictionaryAsync(p => p.Id, p => p.Name);

			return variants.Select(v => new ProductVariantListDto
			{
				Id = v.Id,
				ProductId = v.ProductId,
				ProductName = products.ContainsKey(v.ProductId) ? products[v.ProductId] : "",
				Ram = v.Ram,
				Storage = v.Storage,
				Color = v.Color,
				Price = v.Price,
				SKU = v.SKU
			}).ToList();
		}
	}
}
