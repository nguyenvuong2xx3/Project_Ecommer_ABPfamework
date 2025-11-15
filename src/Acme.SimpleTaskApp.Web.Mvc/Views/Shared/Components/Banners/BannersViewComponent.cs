using Acme.SimpleTaskApp.Banners;
using Acme.SimpleTaskApp.Banners.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.Banners
{
	public class BannersViewComponent : ViewComponent
	{
		private readonly IBannerAppService _bannerAppService;
		public BannersViewComponent(IBannerAppService bannerAppService)
		{
			_bannerAppService = bannerAppService;
		}
		public async Task<IViewComponentResult> InvokeAsync(string viewName = "BannerHomeTop")
		{
			if (viewName == "BannerHomeTop")
			{
				var banners = await _bannerAppService.GetListBanners(new GetAllBannerDto { Position = BannerPosition.HomeTop, IsActive = true });
				var model = new BannersViewModel
				{
					Banners = banners
				};
				return View(viewName, model);

			}
			if (viewName == "BannerHomeMiddle")
			{
				var banners = await _bannerAppService.GetListBanners(new GetAllBannerDto { Position = BannerPosition.HomeMiddle, IsActive = true });
				var model = new BannersViewModel
				{
					Banners = banners
				};
				return View(viewName, model);
			}
			if (viewName == "BannerDetailMiddle")
			{
				var banners = await _bannerAppService.GetListBanners(new GetAllBannerDto { Position = BannerPosition.ProductPage, IsActive = true });
				var model = new BannersViewModel
				{
					Banners = banners
				};
				return View(viewName, model);
			}
			return View();
		}
	}
}
