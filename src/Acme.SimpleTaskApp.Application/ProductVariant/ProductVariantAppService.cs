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
		private readonly IRepository<Products.ProductVariant> _productVariantRepository;
		private readonly IRepository<Categories.Category> _categoryRepository;
		private readonly IRepository<Products.ProductImage> _productImageRepository;
		private readonly IWebHostEnvironment _env;
		private readonly IUploadFileAppService _uploadFileAppService;

		public ProductVariantAppService(IRepository<Product> productRepository,
								IRepository<Category> categoryRepository,
								IRepository<ProductImage> productImageRepository,
								IRepository<Products.ProductVariant> productVariantRepository,
								IUploadFileAppService uploadFileAppService,
								IWebHostEnvironment env)
		{
			_productVariantRepository = productVariantRepository;
			_uploadFileAppService = uploadFileAppService;
			_categoryRepository = categoryRepository;
			_productImageRepository = productImageRepository;
			_env = env;
		}
		public async Task CreateOrUpdate(Products.ProductVariant input)
		{
			if (input == null) {
				throw new UserFriendlyException("Dữ liệu không được để trống");
			}
			if (input.Id == 0)
			{
				await _productVariantRepository.InsertAsync(input);
			}
			else
			{
				await _productVariantRepository.UpdateAsync(input);
			}
		}

		public async Task Delete(int id)
		{
			var entity = await _productVariantRepository.GetAsync(id);
			if (entity == null)
			{
				throw new UserFriendlyException("Dữ liệu không tồn tại");
			}
			_productVariantRepository.Delete(entity);

		}

		public async Task<PagedResultDto<Products.ProductVariant>> GetAll(GetProductVariantsInput input)
		{
			var query = _productVariantRepository.GetAll();
			if (input.ProductId.HasValue)
			{
				query = query.Where(v => v.ProductId == input.ProductId.Value);
			}
			var totalCount = query.Count();
			var items =  query.OrderByDescending(p => p.CreationTime)
					.PageBy(input)
					.ToList();

			return new PagedResultDto<Products.ProductVariant>(totalCount, items);
		}

		public Task<Products.ProductVariant> GetById(int id)
		{
			return _productVariantRepository.GetAsync(id);
		}
	}
}
