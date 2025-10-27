using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Web.Models.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class CartsController : SimpleTaskAppControllerBase
	{
		private readonly ICartAppService _cartAppService;
		private readonly UserManager<User> _userManager;
		public CartsController(ICartAppService cartAppService, UserManager<User> userManager)
		{
			_cartAppService = cartAppService;
			_userManager = userManager;
		}

		[Authorize]
		public async Task<ActionResult> AddCart(int productId, int quantity)
		{
			await _cartAppService.CreateCart(productId, quantity);
			return Json(new { success = true });
		}
		public async Task<ActionResult> OrderInfoModal()
		{
			var user = await _userManager.FindByIdAsync(AbpSession.UserId.ToString());
			//var tinhThanhs = await _locationService.GetAllTinhThanhAsync();
			var model = new OrderInfoModalViewModel
			{
				//SoDienThoai = user.SoDienThoai,
				//TinhThanh = tinhThanhs,
				User = user
			};
			return PartialView("_OrderInfoModal", model);
		}
	}
}
