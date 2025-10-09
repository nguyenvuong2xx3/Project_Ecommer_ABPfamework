using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.ProductVariant.Dtos;
using Acme.SimpleTaskApp.UploadFile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductVariant
{
	public class ProductVariantAppService : ApplicationService, IProductVariantAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<Products.ProductVariant> _productVariantRepository;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<ProductImage> _productImageRepository;
		private readonly IWebHostEnvironment _env;
		private readonly IUploadFileAppService _uploadFileAppService;

		public ProductVariantAppService(IRepository<Product> productRepository,
								IRepository<Category> categoryRepository,
								IRepository<ProductImage> productImageRepository,
								IRepository<Products.ProductVariant> productVariantRepository,
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
		public async Task CreateProductVariant(Products.ProductVariant input)
		{
			if (input == null)
			{
				throw new UserFriendlyException("Dữ liệu không được để trống");
			}

			await _productVariantRepository.InsertAsync(input);
			CurrentUnitOfWork.SaveChanges();

			int sortOrder = 0;
			foreach (var item in input.ImageFiles)
			{
				var imageUrl = _uploadFileAppService.UploadImageAsync(item, "products/variants");
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
		public async Task EditProductVariant(Products.ProductVariant input)
		{
			if (input == null)
			{
				throw new UserFriendlyException("Dữ liệu không được để trống");
			}
			var get = await _productVariantRepository.GetAsync(input.Id);
			if(get == null)
			{
				throw new UserFriendlyException("Không tìm thấy dữ liệu");
			}
			if (input.DeletedImageUrls.Count > 0) 
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
			if(input.ImageFiles != null && input.ImageFiles.Any())
			{
				int generalSortOrder = 0;

				foreach (var imageInput in input.ImageFiles)
				{
					var imagePath = _uploadFileAppService.UploadImageAsync(imageInput, "products/general");
					var productImage = new ProductImage
					{
						ProductId = input.Id,
						ProductVariantId = input.Id,
						ImageUrl = imagePath,
						SortOrder = generalSortOrder++,
						AltText = "Dữ liệu ảnh lỗi"
					};
					_productImageRepository.Insert(productImage);
				}
			}
			await _productVariantRepository.UpdateAsync(input);
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

		public async Task<PagedResultDto<Products.ProductVariant>> GetAllProductVariant(GetProductVariantsInput input)
		{
			var query = _productVariantRepository.GetAll();
			if (input.ProductId.HasValue)
			{
				query = query.Where(v => v.ProductId == input.ProductId.Value);
			}

			var totalCount = query.Count();
			var items = query.OrderByDescending(p => p.CreationTime)
					.PageBy(input)
					.ToList();

			// Get product images
			var productVariantIds = query.Select(p => p.Id).ToList();
			var productImages = await _productImageRepository.GetAll()
					.Where(pi => productVariantIds.Contains(pi.ProductVariantId.Value))
					.OrderBy(pi => pi.SortOrder)
					.ToListAsync();

			// Group images by product and take the first one for each product
			var defaultImages = productImages
					.GroupBy(pi => pi.ProductVariantId)
					.ToDictionary(g => g.Key, g => g.FirstOrDefault()?.ImageUrl);

			// Fetch product names
			var productIds = items.Select(i => i.ProductId).Distinct().ToList();
			var products = await _productRepository.GetAll()
					.Where(p => productIds.Contains(p.Id))
					.ToDictionaryAsync(p => p.Id, p => p.Name);

			// Set the default image for each product
			foreach (var item in items)
			{
				item.ImageUrl = defaultImages.ContainsKey(item.Id) ? defaultImages[item.Id] : null;
			}
			var resultItems = items.Select(item =>
			{
				var productName = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null;
				return new Products.ProductVariant
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
					ImageUrl = defaultImages.ContainsKey(item.Id) ? defaultImages[item.Id] : null
				};
			}).ToList();
			return new PagedResultDto<Products.ProductVariant>(totalCount, items);
		}
		public async Task<Products.ProductVariant> GetById(int id)
		{
			if (id <= 0)
				throw new UserFriendlyException("Dữ liệu không được để trống");
			var item = await _productVariantRepository.GetAsync(id);
			item.ImageUrls = _productImageRepository.GetAll().Where(x => x.ProductVariantId == item.Id).Select(ig => ig.ImageUrl).ToList(); // lấy tất cả đường dẫn ảnh để hiển thị
			return item;
		}
	}
}
