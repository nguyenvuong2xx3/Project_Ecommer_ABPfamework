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

		//[HttpPost]
		//public async Task<IActionResult> CreateOrder(int PaymentMethod)
		//{
		//	try
		//	{
		//		var currentUserId = AbpSession.UserId ?? throw new UserFriendlyException("Vui lòng đăng nhập để đặt hàng");
				
		//		// Lấy giỏ hàng hiện tại
		//		var getCart = await _cartAppService.GetCart();
				
		//		if (getCart.CartItems == null || getCart.CartItems.Count == 0)
		//		{
		//			throw new UserFriendlyException("Giỏ hàng của bạn đang trống");
		//		}

		//		// Chuyển đổi CartItems thành OrderDetails (sử dụng ProductVariantId)
		//		var orderDetails = getCart.CartItems.Select(cartItem => new OrderDetailDto
		//		{
		//			ProductId = cartItem.IdProductVariant, // Đây là ProductVariantId
		//			Quantity = cartItem.Quantity
		//		}).ToList();

		//		// Tạo đơn hàng với Status = 0 (Chờ xác nhận)
		//		var orderId = await _ordersAppService.CreateOrder(new CreateOrderInput
		//		{
		//			UserId = currentUserId,
		//			OrderDetails = orderDetails,
		//			PaymentMethod = PaymentMethod,
		//			Status = 0 // 0: Chờ xác nhận
		//		});

		//		// Xóa giỏ hàng sau khi đặt hàng thành công
		//		await _cartAppService.DeleteCart(currentUserId);

		//		// Lấy thông tin đơn hàng vừa tạo
		//		var viewModel = await _ordersAppService.GetOrder(orderId);

		//		var model = new OrderViewModel()
		//		{
		//			Status = viewModel.Status,
		//			UserName = viewModel.UserName,
		//			EmailAddress = viewModel.EmailAddress,
		//			OrderDetails = viewModel.OrderDetails,
		//			TotalPrice = viewModel.TotalPrice,
		//			PaymentMethod = viewModel.PaymentMethod
		//		};

		//		return PartialView("_OrderSuccess", model);
		//	}
		//	catch (UserFriendlyException ex)
		//	{
		//		// Trả về thông báo lỗi cho người dùng
		//		return Json(new { success = false, message = ex.Message });
		//	}
		//	catch (System.Exception ex)
		//	{
		//		// Log lỗi và trả về thông báo chung
		//		Logger.Error("Error creating order", ex);
		//		return Json(new { success = false, message = "Đã xảy ra lỗi khi đặt hàng. Vui lòng thử lại sau." });
		//	}
		//}

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
	}
}
