using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Acme.SimpleTaskApp.Categories.Dto;
using Acme.SimpleTaskApp.Categories.Dtos;
using Acme.SimpleTaskApp.Products;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Categories
{
	public interface ICategoryFEAppService : IApplicationService
	{
		Task<PagedResultDto<CategoryListDto>> GetAllCategories(GetAllCategoryDto input);
	}

	public class CategoryFEAppService : ApplicationService, ICategoryFEAppService
	{

		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<Category> _categoryRepository;

		public CategoryFEAppService(IRepository<Category> categoryRepository, IRepository<Product> productRepository)
		{
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
		}

			

		public async Task<PagedResultDto<CategoryListDto>> GetAllCategories(GetAllCategoryDto input)
		{
			using var uow = UnitOfWorkManager.Begin();
			using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
				try
				{
					var category = _categoryRepository.GetAll();
					var count = await category.CountAsync();

					var categoryDtos = await category.OrderByDescending(x => x.CreationTime)
																					.PageBy(input)
																					.Select(p => new CategoryListDto
																					{
																						Id = p.Id,
																						Name = p.Name,
																						Description = p.Description,
																						CreationTime = p.CreationTime,
																					}).ToListAsync();

					return new PagedResultDto<CategoryListDto>(count, categoryDtos);
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
