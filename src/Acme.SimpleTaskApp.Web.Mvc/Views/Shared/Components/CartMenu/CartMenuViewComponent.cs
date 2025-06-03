using Acme.SimpleTaskApp.Carts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.CartMenu
{
	public class CartMenuViewComponent : ViewComponent
	{
		private readonly ICartFrontendAppService _cartFrontendAppService;

		public CartMenuViewComponent(ICartFrontendAppService cartFrontendAppService)
		{
			_cartFrontendAppService = cartFrontendAppService;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var model = new CartMenuViewModel
			{
				totalCartItems = await _cartFrontendAppService.GetCartCountAsync()
			};
			return View(model);
		}
	}
}