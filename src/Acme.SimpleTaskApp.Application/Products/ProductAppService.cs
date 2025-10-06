using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Products
{
	//[AbpAuthorize]
	public class ProductAppService : ApplicationService, IProductAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<ProductImage> _productImageRepository;
		private readonly IWebHostEnvironment _env;

		public ProductAppService(IRepository<Product> productRepository, IRepository<Category> categoryRepository,
								IRepository<ProductImage> productImageRepository,
								IWebHostEnvironment env)
		{
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
			_productImageRepository = productImageRepository;
			_env = env;
		}

		public Task<ProductListDto> CreateProducts(CreateProductDto input)
		{
			throw new NotImplementedException();
		}

		//public async Task<ProductListDto> CreateProducts(CreateProductDto input)
		//{
		//	// Tạo mới sản phẩm
		//	var product = new Product
		//	{
		//		Name = input.Name,
		//		Description = input.Description,
		//		CategoryId = input.CategoryId,
		//		Processor = input.Processor,
		//		CameraSystem = input.CameraSystem,
		//		Battery = input.Battery,
		//		StockQuantity = input.Variants?.Sum(v => v.StockQuantity) ?? 0,
		//	};

		//	await _productRepository.InsertAsync(product);
		//	await CurrentUnitOfWork.SaveChangesAsync();

		//	// Lưu biến thể
		//	if (input.Variants != null)
		//	{
		//		foreach (var variant in input.Variants)
		//		{
		//			variant.ProductId = product.Id;
		//			// TODO: Lưu biến thể vào DB (cần repository cho ProductVariant)
		//		}
		//	}

		//	// Lưu ảnh
		//	if (input.ProductImages != null)
		//	{
		//		foreach (var img in input.ProductImages)
		//		{
		//			img.ProductId = product.Id;
		//			await _productImageRepository.InsertAsync(img);
		//		}
		//		await CurrentUnitOfWork.SaveChangesAsync();
		//	}

		//	var firstImage = input.ProductImages?.FirstOrDefault();

		//	return new ProductListDto
		//	{
		//		Id = product.Id,
		//		Name = product.Name,
		//		Description = product.Description,
		//		//Price = product.Price,
		//		CreationTime = product.CreationTime,
		//		Image = firstImage?.ImageUrl,
		//		CategoryId = product.CategoryId
		//	};
		//}

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
