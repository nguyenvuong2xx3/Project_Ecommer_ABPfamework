using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Categories.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Categories
{
	public interface ICategoryAppService : IApplicationService
	{
		Task<PagedResultDto<Category>> GetAllCategories(GetAllCategoryDto input);
		Task<List<CategoryListDto>> GetAllCategoriesProduct(GetAllCategoryDto input);
		Task<Category> CreateCategory(Category input);
		Task<Category> GetByIdCategory(EntityDto<int> input);
		Task DeleteCategory(EntityDto<int> input);
		Task<Category> UpdateCategory(Category input);
		Task<PagedResultDto<CategoryListDto>> SearchCategory(GetAllCategoryDto input);
		Task<List<CategoryTreeDto>> GetAllCategoriesTree(GetAllCategoryDto input);
		Task<List<CategoryListDto>> GetAllCategoriesForSelect();
	}
}
