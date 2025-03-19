using Abp.Application.Services.Dto;
using Abp.UI;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Web.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class CategoriesController : SimpleTaskAppControllerBase
	{
		private readonly ICategoryAppService _categoryAppService;

		public CategoriesController(ICategoryAppService categoryAppService)
		{
			_categoryAppService = categoryAppService;
		}
		public async Task<ActionResult> Index(GetAllCategoryDto input)
		{
			var output = await _categoryAppService.GetAllCategories(input);
			var model = new CategoryViewModel(output.Items);
			return View(model);
		}


		[HttpPost]
		[Route("Categories/EditModal")]
		public async Task<PartialViewResult> EditCategoryModal(int categoryId)
		{
			try
			{
				// Gọi service để lấy sản phẩm theo Id
				var category = await _categoryAppService.GetByIdCategory(new EntityDto<int>(categoryId));
				var editCategoryDto = new EditCategoryDto
				{
					Id = category.Id,
					Name = category.Name,
					Description = category.Description,
				};
				var viewModel = new EditCategoryModalViewModel { Category = editCategoryDto };
				return PartialView("_EditCategoryModal", viewModel);
			}
			catch (UserFriendlyException ex)
			{
				// Log lỗi và throw exception để client nhận thông báo
				Logger.Error(ex.Message, ex);
				throw;
			}
		}
	}
}
