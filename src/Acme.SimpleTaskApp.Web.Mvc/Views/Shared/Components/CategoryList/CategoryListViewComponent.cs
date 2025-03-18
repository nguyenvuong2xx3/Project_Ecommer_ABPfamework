using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.CategoryList
{
	public class CategoryListViewComponent : ViewComponent
	{
		private readonly ICategoryAppService _categoryAppService;
		private readonly ICategoryFEAppService _categoryFEAppService;

		public CategoryListViewComponent(ICategoryAppService categoryAppService, ICategoryFEAppService categoryFEAppService)
		{
			_categoryAppService = categoryAppService;
			_categoryFEAppService = categoryFEAppService;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var model = new CategoryListViewModel
			{
				Categories = await _categoryFEAppService.GetAllCategories(new GetAllCategoryDto { })
			};

			return View(model);
		}
	}
}
