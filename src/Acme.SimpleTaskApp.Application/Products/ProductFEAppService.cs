using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Acme.SimpleTaskApp.Products.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Acme.SimpleTaskApp.Categories;
using Microsoft.AspNetCore.Hosting;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Acme.SimpleTaskApp.Products
{
	public interface IProductFEAppService : IApplicationService
	{
		Task<PagedResultDto<ProductListDto>> GetAllProducts(GetAllProductsInput input);

	}

	public class ProductFEAppService : ApplicationService, IProductFEAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IWebHostEnvironment _env;

		public ProductFEAppService(IRepository<Product> productRepository,
															IRepository<Category> categoryRepository, IWebHostEnvironment env)
		{
			_productRepository = productRepository;
			_env = env;
		}

		public async Task<PagedResultDto<ProductListDto>> GetAllProducts(GetAllProductsInput input)
		{

			using var uow = UnitOfWorkManager.Begin();
			using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
				try
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
				catch (Exception)
				{
					return null;
				}
				finally
				{
					await uow.CompleteAsync();
				}
		}

	}
}
