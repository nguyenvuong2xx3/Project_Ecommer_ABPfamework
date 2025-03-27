using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using System;
using static Acme.SimpleTaskApp.Products.Product;
using System.Collections.Generic;
//using Abp.Authorization;
//using Acme.SimpleTaskApp.Authorization;
namespace Acme.SimpleTaskApp.Products
{
	//[AbpAuthorize]
	public class ProductAppService : ApplicationService, IProductAppService
	{
		private readonly IRepository<Product> _productRepository;
		//private readonly IRepository<Category> _categoryRepository;
		private readonly IWebHostEnvironment _env;

		public ProductAppService(IRepository<Product> productRepository,
															 IWebHostEnvironment env) // IRepository<Category> categoryRepository,
		{
			_productRepository = productRepository;
			//_categoryRepository = categoryRepository;
			_env = env;
		}

		//[AbpAuthorize(PermissionNames.Pages_products)]
		public async Task<PagedResultDto<ProductListDto>> GetAllProducts(GetAllProductsInput input)
		{
			var products = _productRepository.GetAll();

			var count = await products.CountAsync();

			var productDtos = await products.OrderByDescending(x => x.CreationTime)
																			.PageBy(input)
																			.Select(p => new ProductListDto
																			{
																				Id = p.Id,
																				Name = p.Name,
																				Description = p.Description,
																				Price = p.Price,
																				State = p.State,
																				CreationTime = p.CreationTime,
																				Image = p.Image
																			}).ToListAsync();

			return new PagedResultDto<ProductListDto>(count, productDtos);
		}


		//[AbpAuthorize(PermissionNames.Pages_products_create)]
		public async Task<ProductListDto> CreateProducts(CreateProductDto input)
		{
			var product = new Product
			{
				Name = input.Name,
				Description = input.Description,
				Price = input.Price,
				State = input.State,
				CategoryId = input.CategoryId,
				Image = input.Image // Lưu đường dẫn ảnh
			};

			await _productRepository.InsertAsync(product);
			await CurrentUnitOfWork.SaveChangesAsync();

			return new ProductListDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				State = product.State,
				CreationTime = product.CreationTime,
				Image = product.Image
			};
		}


		//[AbpAuthorize(PermissionNames.Pages_products_delete)]
		public async Task DeleteProducts(EntityDto<int> input)
		{
			// Xóa sản phẩm
			await _productRepository.DeleteAsync(input.Id);
		}

		public async Task<ProductListDto> GetByIdProducts(EntityDto<int> input)
		{
			// Lấy sản phẩm theo Id
			var product = await _productRepository.GetAsync(input.Id);
			if (product == null)
			{
				throw new UserFriendlyException("Product not found!");
			}
			else
			{
				// Ánh xạ sang DTO
				return new ProductListDto
				{
					Id = product.Id,
					Name = product.Name,
					Description = product.Description,
					Price = product.Price,
					State = product.State,
					CreationTime = product.CreationTime,
					Image = product.Image,
					CategoryId = product.CategoryId
				};
			}
		}

		//[AbpAuthorize(PermissionNames.Pages_products_update)]
		public async Task<ProductListDto> UpdateProducts(UpdateProductDto input)
		{
			// Lấy sản phẩm hiện có
			var product = await _productRepository.GetAsync(input.Id);
			// đang bị lỗi nếu product = null thì sẽ không trả ra exception và dư 1 ít code đoạn đầu ở controller
			if (product == null)
			{
				throw new Exception("Sản phẩm không tồn tại!");
			}

			// Cập nhật thông tin (trừ Image)
			product.Name = input.Name;
			product.Description = input.Description;
			product.Price = input.Price;
			product.State = input.State;
			product.CategoryId = input.CategoryId;

			// Chỉ cập nhật Image nếu có ảnh mới
			if (!string.IsNullOrEmpty(input.Image))
			{
				product.Image = input.Image;
			}

			// Lưu thay đổi vào database
			await _productRepository.UpdateAsync(product);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Ánh xạ sang DTO để trả về
			return new ProductListDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				State = product.State,
				CreationTime = product.CreationTime,
				Image = product.Image, // Giữ nguyên ảnh cũ nếu không cập nhật
				CategoryId = product.CategoryId
			};
		}


		//[AbpAuthorize(PermissionNames.Pages_products_search)]
		public async Task<PagedResultDto<ProductListDto>> SearchProducts(GetAllProductsInput input)
		{
			var productQuery = _productRepository.GetAll();
			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				string keywordLower = input.Keyword.ToLower();
				productQuery = productQuery.Where(p => p.Name.ToLower().Contains(keywordLower));
			}

			if (!string.IsNullOrWhiteSpace(input.Category))
			{
				int categoryId = Convert.ToInt32(input.Category);
				productQuery = productQuery.Where(x => x.CategoryId == categoryId);
			}
			if (!string.IsNullOrWhiteSpace(input.StateInput) && Enum.TryParse<ProductState>(input.StateInput, out var state))
			{
				productQuery = productQuery.Where(x => x.State == state);
			}
			var count = await productQuery.CountAsync();

			var productDtos = await productQuery.OrderByDescending(p => p.CreationTime)
																			.PageBy(input)
																			.Select(p => new ProductListDto
																			{
																				Id = p.Id,
																				Name = p.Name,
																				Description = p.Description,
																				Price = p.Price,
																				State = p.State,
																				CreationTime = p.CreationTime,
																				Image = p.Image
																			}).ToListAsync();

			return new PagedResultDto<ProductListDto>(count, productDtos);

			
		}
	}
}
