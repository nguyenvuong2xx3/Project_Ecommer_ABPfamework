using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Categories.Dto;
using Acme.SimpleTaskApp.Categories.Dtos;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Categories
{
	public interface ICategoryAppService : IApplicationService
	{
		Task<PagedResultDto<CategoryListDto>> GetAllCategories(GetAllCategoryDto input);


		Task<CategoryListDto> CreateCategory(CreateCategoryDto input);

		Task<CategoryListDto> GetByIdCategory(EntityDto<int> input);

		Task DeleteCategory(EntityDto<int> input);

		Task<CategoryListDto> UpdateCategory(UpdateCategoryDto input);

		Task<PagedResultDto<CategoryListDto>> SearchCategory(GetAllCategoryDto input);
	}
}
