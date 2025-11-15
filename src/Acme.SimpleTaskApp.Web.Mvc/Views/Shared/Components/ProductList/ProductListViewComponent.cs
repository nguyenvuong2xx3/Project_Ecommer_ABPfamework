using Abp.Domain.Uow;
using Acme.SimpleTaskApp.Banners;
using Acme.SimpleTaskApp.Banners.Dtos;
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
			private readonly IBannerAppService _bannerAppService;

			public ProductListViewComponent(IHomeCustomerAppService homeCustomerAppService, IBannerAppService bannerAppService)
			{
				_homeCustomerAppService = homeCustomerAppService;
				_bannerAppService = bannerAppService;
			}

			public async Task<IViewComponentResult> InvokeAsync(string viewName = "FilterPrice", int categoryId = 0)
			{
				if (viewName == "FilterPrice")
				{
					var result = await _homeCustomerAppService.GetAllProductHomeCustomers(new SearchHomeCustomerDto { MaxPrice = 5000000 });
					var banners = await _bannerAppService.GetListBanners(new GetAllBannerDto { Position = BannerPosition.HomeTop, IsActive = true });
					var model = new ProductListViewModel
					{
						ProductsInfo = result.Items.ToList(),
						Banner = banners.FirstOrDefault()
					};

					return View(viewName, model);
				}
				if (viewName == "FilterCreatetion")
				{
					var result = await _homeCustomerAppService.GetAllProductHomeCustomers(new SearchHomeCustomerDto { SortingCreation = true });
					var banners = await _bannerAppService.GetListBanners(new GetAllBannerDto { Position = BannerPosition.HomeMiddle, IsActive = true });
					var model = new ProductListViewModel
					{
						ProductsInfo = result.Items.ToList(),
						Banner = banners.FirstOrDefault()
					};
					return View(viewName, model);
				}
				if (viewName == "FilterCategory" && categoryId > 0)
				{
					var result = await _homeCustomerAppService.GetAllProductHomeCustomers(new SearchHomeCustomerDto { SortingCreation = true, CategoryId = categoryId });
					var banners = await _bannerAppService.GetListBanners(new GetAllBannerDto { Position = BannerPosition.HomeMiddle, IsActive = true });
					var model = new ProductListViewModel
					{
						ProductsInfo = result.Items.ToList(),
						Banner = banners.FirstOrDefault()
					};
					return View(viewName, model);
				}
				return View();
			}
		}
	}
}