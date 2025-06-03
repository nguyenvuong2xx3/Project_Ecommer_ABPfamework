using Abp.Runtime.Session;
using Abp.UI;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Carts.Dtos;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Orders;
using Acme.SimpleTaskApp.Orders.Dtos;
using Acme.SimpleTaskApp.Web.Models.Orders;
using Acme.SimpleTaskApp.Web.Models.Products;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class OrdersController : SimpleTaskAppControllerBase
	{
		private readonly ICartAppService _cartAppService;
		private readonly IOrdersAppService _ordersAppService;
		public OrdersController(ICartAppService cartAppService, IOrdersAppService ordersAppService)
		{
			_cartAppService = cartAppService;
			_ordersAppService = ordersAppService;
		}
		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> CreateOrder()
		{
			var currentUserId = AbpSession.UserId ?? throw new UserFriendlyException("Cannot find user");
			var getCart = await _cartAppService.GetCart(new GetCartInput { UserId = currentUserId });
			if (getCart.CartItems.Count > 0)
			{
				var orderDetails = getCart.CartItems.Select(cartItem => new OrderDetailDto
				{
					ProductId = cartItem.ProductId,
					Quantity = cartItem.Quantity
				}).ToList();

				await _ordersAppService.CreateOrder(new CreateOrderInput
				{
					UserId = currentUserId,
					OrderDetails = orderDetails
				});
				await _cartAppService.DeleteCart(currentUserId);
			}
			return PartialView("_OrderSuccess");
		}
		//public async Task<IActionResult> DetailOrder()
		//{
		//	var viewmodel = await _ordersAppService.GetUserOrders(new GetAllOrderInput { });

		//	var model = new OrdersViewModel()
		//	{
		//		Status = viewmodel.Orders.Select(o => o.Status).ToList(),
		//	};
		//	return View();
		//}
	}
}
