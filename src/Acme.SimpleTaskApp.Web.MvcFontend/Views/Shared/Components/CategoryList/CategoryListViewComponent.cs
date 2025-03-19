using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.CategoryList
{
	public class CategoryListViewComponent : ViewComponent
	{
		private readonly ICategoryFEAppService _categoryFEAppService;

		public CategoryListViewComponent(ICategoryFEAppService categoryFEAppService)
		{
			_categoryFEAppService = categoryFEAppService;
		}

		public async Task<IViewComponentResult> InvokeAsync(string viewName = "Default")
		{
			var model = new CategoryListViewModel
			{
				Categories = await _categoryFEAppService.GetAllCategories(new GetAllCategoryDto { })
			};

			return View(viewName, model);
		}
	}
}
