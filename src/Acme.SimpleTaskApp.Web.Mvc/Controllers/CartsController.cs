using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class CartsController : SimpleTaskAppControllerBase
	{
		private readonly ICartAppService _cartAppService;
		public CartsController(ICartAppService cartAppService)
		{
			_cartAppService = cartAppService;
		}

		//[Authorize]
		public async Task<ActionResult> AddCart(int productId, int quantity)
		{
			await _cartAppService.CreateCart(productId, quantity);
			return Json(new { success = true });
		}
	}
}
