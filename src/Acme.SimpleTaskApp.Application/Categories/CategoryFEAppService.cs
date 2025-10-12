using Abp.Application.Services;
using Abp.Domain.Repositories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Acme.SimpleTaskApp.Products;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Categories
{
	public interface ICategoryFEAppService : IApplicationService
	{
		Task<List<Category>> GetAllCategories();
		Task<List<CategoryTreeDto>> GetAllCategoriesTree();
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

		// cây danh mục
		public async Task<List<CategoryTreeDto>> GetAllCategoriesTree()
		{
			using var uow = UnitOfWorkManager.Begin();
			using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
				try
				{
					var categories = await _categoryRepository.GetAll()
				.OrderBy(c => c.Order)
				.ToListAsync();

					// build map
					var map = categories.ToDictionary(c => c.Id, c => new CategoryTreeDto
					{
						Id = c.Id,
						Name = c.Name,
						Description = c.Description,
						ParentId = c.ParentId,
						CreationTime = c.CreationTime
					});

					var roots = new List<CategoryTreeDto>();
					foreach (var dto in map.Values)
					{
						if (dto.ParentId.HasValue && map.ContainsKey(dto.ParentId.Value))
						{
							map[dto.ParentId.Value].Children.Add(dto);
						}
						else
						{
							roots.Add(dto);
						}
					}
					return roots;

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

		// dữ liệu phẳng
		public async Task<List<Category>> GetAllCategories()
		{
			using var uow = UnitOfWorkManager.Begin();
			using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
				try
				{
					var query = _categoryRepository.GetAll();
					return query.ToList();
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
