using Abp.UI;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Carts.Dtos;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.OrderItems;
using Acme.SimpleTaskApp.Orders;
using Acme.SimpleTaskApp.Orders.Dtos;
using Acme.SimpleTaskApp.Web.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Mail;
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
		public IActionResult IndexForCustomer()
		{
			return View();
		}


		[HttpPost]
		public async Task<IActionResult> CreateOrder(int PaymentMethod)
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

				var orderId = await _ordersAppService.CreateOrder(new CreateOrderInput
				{
					UserId = currentUserId,
					OrderDetails = orderDetails,
					PaymentMethod = PaymentMethod
				});
				await _cartAppService.DeleteCart(currentUserId);

				var viewModel = await _ordersAppService.GetOrder(orderId);

				var model = new OrderViewModel()
				{
					Status = viewModel.Status,
					UserName = viewModel.UserName,
					EmailAddress = viewModel.EmailAddress,
					OrderDetails = viewModel.OrderDetails,
					TotalPrice = viewModel.TotalPrice
				};

				return PartialView("_OrderSuccess", model);
			}

			return PartialView("_OrderSuccess", null);
		}

		public async Task<IActionResult> DetailOrder(int orderId)
		{
			var viewModel = await _ordersAppService.GetOrder(orderId);

			var model = new OrderViewModel()
			{
				Status = viewModel.Status,
				UserName = viewModel.UserName,
				EmailAddress = viewModel.EmailAddress,
				OrderDetails  = viewModel.OrderDetails
			};
			return PartialView("_DetailOrderModal", model);
		}
		public async Task<IActionResult> DetailOrderForUser()
		{
			var viewModel = await _ordersAppService.GetOrderByUserId();

			var model = new OrderViewModel()
			{
				Status = viewModel.Status,
				UserName = viewModel.UserName,
				EmailAddress = viewModel.EmailAddress,
				OrderDetails = viewModel.OrderDetails
			};
			return PartialView("DetailOrderUserModal", model);
		}
		//public async Task<IActionResult> HistoryDetailOrderForUser()
		//{
		//	var viewModel = await _ordersAppService.GetOrdersByUserId();

		//	var model = new OrderViewModel()
		//	{
		//		Status = viewModel.Status,
		//		UserName = viewModel.UserName,
		//		EmailAddress = viewModel.EmailAddress,
		//		OrderDetails = viewModel.OrderDetails
		//	};
		//	return PartialView("DetailOrderUserModal", model);
		//}
	}
}
