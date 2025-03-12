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
			try
			{
				// Lấy danh mục cần xóa
				var category = await _categoryRepository.GetAsync(input.Id);
				if (category == null)
				{
					throw new UserFriendlyException("Không tìm thấy danh mục!");
				}

				// Lấy tất cả sản phẩm thuộc danh mục này
				var productsInCategory = await _productRepository.GetAll()
				.Where(p => p.CategoryId == input.Id)
				.ToListAsync();

				// Cập nhật CategoryId của sản phẩm thành null
				foreach (var product in productsInCategory)
				{
					product.CategoryId = null; // Đặt CategoryId thành null
					await _productRepository.UpdateAsync(product); // Cập nhật
				}

				// Xóa danh mục
				await _categoryRepository.DeleteAsync(category);

				// Lưu thay đổi
				await CurrentUnitOfWork.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				throw new UserFriendlyException("Xóa danh mục thất bại: " + ex.Message);
			}
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
			// Lấy sản phẩm hiện có
			var category = await _categoryRepository.GetAsync(input.Id);
			if (category == null)
			{
				throw new UserFriendlyException("Category not found!");
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
