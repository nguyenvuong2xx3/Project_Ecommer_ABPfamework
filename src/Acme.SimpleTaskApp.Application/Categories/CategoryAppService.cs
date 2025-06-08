using Abp.Application.Services;
using System;
using System.Threading.Tasks;
using Acme.SimpleTaskApp.Categories.Dto;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Categories.Dtos;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.Products;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Categories
{
	public class CategoryAppService : ApplicationService, ICategoryAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IWebHostEnvironment _env;
		public CategoryAppService(IRepository<Category> categoryRepository, IWebHostEnvironment env, IRepository<Product> productRepository)
		{
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
			_env = env;
		}

		public async Task<CategoryListDto> CreateCategory(CreateCategoryDto input)
		{

			var checkcategory = _categoryRepository.GetAll();
			checkcategory = checkcategory.Where(p => p.Name == input.Name);
			if (await checkcategory.AnyAsync())
			{
				throw new UserFriendlyException("Danh mục đã tồn tại");
			}
			var category = new Category
			{
				Name = input.Name,
				Description = input.Description
			};

			await _categoryRepository.InsertAsync(category);
			await CurrentUnitOfWork.SaveChangesAsync();

			return new CategoryListDto
			{
				Id = category.Id,
				Name = category.Name,
			};
		}

		public async Task DeleteCategory(EntityDto<int> input)
		{
				// Lấy danh mục cần xóa
				var category = await _categoryRepository.GetAsync(input.Id);
				if (category == null)
				{
					throw new UserFriendlyException("Không tìm thấy danh mục!");
				}

				var productsInCategory = await _productRepository.GetAll()
				.Where(p => p.CategoryId == input.Id)
				.ToListAsync();
				if (productsInCategory.Count >= 0)
				{
					throw new UserFriendlyException($"Danh mục {category.Name} đang có sản phẩm không được xóa");
				}
				// Xóa danh mục
				await _categoryRepository.DeleteAsync(category);

				// Lưu thay đổi
				await CurrentUnitOfWork.SaveChangesAsync();
		}

		public async Task<PagedResultDto<CategoryListDto>> GetAllCategories(GetAllCategoryDto input)
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

		public async Task<List<CategoryListDto>> GetAllCategoriesProduct(GetAllCategoryDto input)
		{
			var category = _categoryRepository.GetAll();
			var count = await category.CountAsync();

			var categoryDtos = await category.OrderByDescending(x => x.CreationTime)
																			.Select(p => new CategoryListDto
																			{
																				Id = p.Id,
																				Name = p.Name,
																				Description = p.Description,
																				CreationTime = p.CreationTime,
																			}).ToListAsync();

			return new List<CategoryListDto>(categoryDtos);
		}


		public async Task<CategoryListDto> GetByIdCategory(EntityDto<int> input)
		{
			// Lấy sản phẩm theo Id
			var category = await _categoryRepository.GetAsync(input.Id);
			if (category == null)
			{
				throw new UserFriendlyException("Category not found!");
			}
			else
			{
				// Ánh xạ sang DTO
				return new CategoryListDto
				{
					Id = category.Id,
					Name = category.Name,
					Description = category.Description
				};
			}
		}

		public Task<PagedResultDto<CategoryListDto>> SearchCategory(GetAllCategoryDto input)
		{
			throw new NotImplementedException();
		}

		public async Task<CategoryListDto> UpdateCategory(UpdateCategoryDto input)
		{
			var checkcategory = _categoryRepository.GetAll();
			checkcategory = checkcategory.Where(p => p.Name == input.Name);
			if (await checkcategory.AnyAsync())
			{
				throw new UserFriendlyException("Danh mục đã tồn tại");
			}
			// Lấy sản phẩm hiện có
			var category = await _categoryRepository.GetAsync(input.Id);
			if (category == null)
			{
				throw new UserFriendlyException("Không tìm thấy danh mục");
			}
			// Cập nhật thông tin
			category.Name = input.Name;
			category.Description = input.Description;

			// Lưu thay đổi vào database
			await _categoryRepository.UpdateAsync(category);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Ánh xạ sang DTO để trả về
			return new CategoryListDto
			{
				Id = category.Id,
				Name = category.Name,
				Description = category.Description
			};
		}

		//public async Task<CategoryListDto> GetByIdCategory(EntityDto<int> input)
		//{
		//	var category = await _categoryRepository.GetAsync(input.Id);
		//	return ObjectMapper.Map<CategoryListDto>(category);
		//}

		//public async Task<PagedResultDto<CategoryListDto>> SearchCategory(GetAllCategoryDto input)
		//{
		//	var query = _categoryRepository.GetAll()
		//			.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))
		//			.WhereIf(input.State.HasValue, x => x.State == input.State);

		//	var totalCount = await AsyncQueryableExecuter.CountAsync(query);
		//	var categories = await AsyncQueryableExecuter.ToListAsync(query);

		//	return new PagedResultDto<CategoryListDto>(
		//			totalCount,
		//			ObjectMapper.Map<List<CategoryListDto>>(categories)
		//	);
		//}

		//public async Task<CategoryListDto> UpdateCategory(UpdateCategoryDto input)
		//{
		//	var category = await _categoryRepository.GetAsync(input.Id);
		//	ObjectMapper.Map(input, category);
		//	await _categoryRepository.UpdateAsync(category);
		//	return ObjectMapper.Map<CategoryListDto>(category);
		//}
	}
}
