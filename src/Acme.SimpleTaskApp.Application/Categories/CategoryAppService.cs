using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Categories.Dtos;
using Acme.SimpleTaskApp.Products;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Categories
{
	public class CategoryAppService : ApplicationService, ICategoryAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<Category> _categoryRepository;
		public CategoryAppService(IRepository<Category> categoryRepository, IRepository<Product> productRepository)
		{
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
		}

		public async Task<Category> CreateCategory(Category input)
		{
			var check = await _categoryRepository.FirstOrDefaultAsync(x => x.Name == input.Name);
			if(check != null)
			{
				throw new UserFriendlyException("Tên danh mục đã tồn tại.");
			}
			var category = await _categoryRepository.InsertAsync(input);
			return category;
		}

		public async Task DeleteCategory(EntityDto<int> input)
		{
			await _categoryRepository.DeleteAsync(input.Id);
		}

		public async Task<PagedResultDto<Category>> GetAllCategories(GetAllCategoryDto input)
		{
			var query = _categoryRepository.GetAll()
				.WhereIf(!string.IsNullOrWhiteSpace(input.Name), c => c.Name.Contains(input.Name))
				.WhereIf(!string.IsNullOrWhiteSpace(input.Description), c => c.Description.Contains(input.Description));

			var totalCount = await query.CountAsync();

			var categories = await query
				.OrderBy(c => c.Order)
				.PageBy(input)
				.ToListAsync();

			return new PagedResultDto<Category>(totalCount, categories);
		}

		public async Task<List<CategoryListDto>> GetAllCategoriesProduct(GetAllCategoryDto input)
		{
			var categories = await _categoryRepository.GetAll()
				.WhereIf(!string.IsNullOrWhiteSpace(input.Name), c => c.Name.Contains(input.Name))
				.WhereIf(!string.IsNullOrWhiteSpace(input.Description), c => c.Description.Contains(input.Description))
				.OrderBy(c => c.Order)
				.ToListAsync();

			return categories.Select(c => new CategoryListDto
			{
				Id = c.Id,
				Name = c.Name,
				Description = c.Description,
				CreationTime = c.CreationTime
			}).ToList();
		}

		public async Task<Category> GetByIdCategory(EntityDto<int> input)
		{
			var category = await _categoryRepository.GetAsync(input.Id);
			if (category.ParentId.HasValue)
			{
				category.Parent = await _categoryRepository.GetAsync(category.ParentId.Value);
			}
			if (category == null)
			{
				throw new UserFriendlyException("Could not find the category, maybe it's deleted.");
			}
			return category;
		}

		public async Task<PagedResultDto<CategoryListDto>> SearchCategory(GetAllCategoryDto input)
		{
			var query = _categoryRepository.GetAll()
				.WhereIf(!string.IsNullOrWhiteSpace(input.Name), c => c.Name.Contains(input.Name))
				.WhereIf(!string.IsNullOrWhiteSpace(input.Description), c => c.Description.Contains(input.Description));

			var totalCount = await query.CountAsync();

			var categories = await query
				.OrderBy(c => c.Order)
				.PageBy(input)
				.ToListAsync();

			var categoryDtos = categories.Select(c => new CategoryListDto
			{
				Id = c.Id,
				Name = c.Name,
				Description = c.Description,
				CreationTime = c.CreationTime
			}).ToList();

			return new PagedResultDto<CategoryListDto>(totalCount, categoryDtos);
		}

		public async Task<Category> UpdateCategory(Category input)
		{
			var category = await _categoryRepository.GetAsync(input.Id);
			if (category == null)
			{
				throw new UserFriendlyException("Could not find the category, maybe it's deleted.");
			}
			if (category.Name == input.Name)
			{
				throw new UserFriendlyException("Tên danh mục đã tồn tại.");
			}
			category.Name = input.Name;
			category.Description = input.Description;
			category.ParentId = input.ParentId;
			category.Order = input.Order;

			await _categoryRepository.UpdateAsync(category);
			return category;
		}

		public async Task<List<CategoryTreeDto>> GetAllCategoriesTree(GetAllCategoryDto input)
		{
			var categories = await _categoryRepository.GetAll()
				.WhereIf(!string.IsNullOrWhiteSpace(input.Name), c => c.Name.Contains(input.Name))
				.WhereIf(!string.IsNullOrWhiteSpace(input.Description), c => c.Description.Contains(input.Description))
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

		public async Task<List<CategoryListDto>> GetAllCategoriesForSelect()
		{
			var categories = await _categoryRepository.GetAll()
				.OrderBy(c => c.Order)
				.ToListAsync();

			return categories.Select(c => new CategoryListDto
			{
				Id = c.Id,
				Name = c.Name,
				Description = c.Description,
				CreationTime = c.CreationTime
			}).ToList();
		}
	}
}
