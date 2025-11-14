using Acme.SimpleTaskApp.Banners;
using Acme.SimpleTaskApp.Banners.Dtos;
using Acme.SimpleTaskApp.Web.Views.Shared.Components.ProductList;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.BannerHomeTop
{
	public class BannerHomeTopViewComponent : ViewComponent
	{
		private readonly IBannerAppService _bannerAppService;
		public BannerHomeTopViewComponent(IBannerAppService bannerAppService)
		{
			_bannerAppService = bannerAppService;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var banners = await _bannerAppService.GetListBanners(new GetAllBannerDto { Position = BannerPosition.HomeTop, IsActive = true});
			var model = new BannerHomeTopViewModel
			{
				Banners = banners
			};
			return View(model);
		}
	}
}
