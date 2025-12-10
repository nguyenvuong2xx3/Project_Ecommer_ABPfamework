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
using Abp.AspNetCore.Mvc.Authorization;
using Acme.SimpleTaskApp.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	[AbpMvcAuthorize(PermissionNames.Pages_Orders)]
	public class OrdersController : SimpleTaskAppControllerBase
	{
		private readonly ICartAppService _cartAppService;
		private readonly IOrdersAppService _ordersAppService;

		public OrdersController(ICartAppService cartAppService, IOrdersAppService ordersAppService)
		{
			_cartAppService = cartAppService;
			_ordersAppService = ordersAppService;
		}

		[AbpMvcAuthorize(PermissionNames.Pages_Orders_View)]
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

		[AbpMvcAuthorize(PermissionNames.Pages_Orders_View)]
		public async Task<IActionResult> DetailModal(int orderId)
		{
			var order = await _ordersAppService.GetOrder(orderId);

			var model = new OrderViewModel()
			{
				Order = order
			};
			return PartialView("_DetailOrderModal", model);
		}

		[Authorize]
		public async Task<IActionResult> OrderConfirmation(int orderId)
		{
			var order = await _ordersAppService.GetOrder(orderId);
			var model = new OrderViewModel()
			{
				Order = order
			};
			return PartialView("OrderConfirmation", model);
		}
	}
}
