using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Acme.SimpleTaskApp.Web.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
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
			var result = await _categoryFEAppService.GetAllCategoriesTree();

			var model = new CategoryViewCustomerModel
			{
				Categories = result
			};

			return View(viewName, model);
		}
	}
}
