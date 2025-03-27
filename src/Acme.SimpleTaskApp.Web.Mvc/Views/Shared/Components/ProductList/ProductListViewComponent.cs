
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.ProductList
{

	public class ProductListViewComponent : ViewComponent
	{
		private readonly IProductFEAppService _productFEAppService;

		public ProductListViewComponent(IProductFEAppService productFEAppService)
		{
			_productFEAppService = productFEAppService;
		}

		public async Task<IViewComponentResult> InvokeAsync(string viewName = "Default")
		{
			var model = new ProductListViewModel
			{
				RelatedProducts = await _productFEAppService.GetAllProducts(new GetAllProductsInput { })
			};

			return View(viewName, model);
		}
	}
}
