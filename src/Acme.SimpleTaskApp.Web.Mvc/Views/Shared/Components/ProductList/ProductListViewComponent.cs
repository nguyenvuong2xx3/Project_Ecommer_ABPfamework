using Abp.Domain.Uow;
using Acme.SimpleTaskApp.HomeCustomers;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.ProductVariants;
using Acme.SimpleTaskApp.ProductVariants.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.ProductList
{
	namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.ProductList
	{

		public class ProductListViewComponent : ViewComponent
		{
			private readonly IHomeCustomerAppService _homeCustomerAppService;

			public ProductListViewComponent(IHomeCustomerAppService homeCustomerAppService)
			{
				_homeCustomerAppService = homeCustomerAppService;
			}

			public async Task<IViewComponentResult> InvokeAsync(string viewName = "FilterPrice")
			{
				if(viewName == "FilterPrice")
				{
					var result = await _homeCustomerAppService.GetAllProductHomeCustomers(new SearchHomeCustomerDto { MaxPrice = 5000000 });
					var model = new ProductListViewModel
					{
						ProductsInfo = result.Items.ToList()
					};

					return View(viewName, model);
				}
				if(viewName == "FilterCreatetion")
				{
					var result = await _homeCustomerAppService.GetAllProductHomeCustomers(new SearchHomeCustomerDto { SortingCreation = true });
					var model = new ProductListViewModel
					{
						ProductsInfo = result.Items.ToList()
					};
					return View(viewName, model);
				}
				return View();
			}
		}
	}
}