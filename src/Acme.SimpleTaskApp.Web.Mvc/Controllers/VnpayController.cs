using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Domain.Repositories;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Orders;
using Acme.SimpleTaskApp.Payment.VNPay;
using Microsoft.AspNetCore.Mvc;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	[Route("api/[controller]")]
	public class VnpayController : AbpController
	{
		private readonly IVnpayAppService _vnpayService;
		private readonly IOrdersAppService _ordersAppService;

		public VnpayController(
				IVnpayAppService vnpayService,
				IOrdersAppService ordersAppService)
		{
			_vnpayService = vnpayService;
			_ordersAppService = ordersAppService;
		}

		/// <summary>
		/// Tạo URL thanh toán VNPay
		/// </summary>
		[HttpPost("create-payment-url")]
		public IActionResult CreatePaymentUrl([FromBody] VnpayPaymentRequest request)
		{
			if (request == null || request.Amount <= 0)
			{
				return BadRequest(new { success = false, message = "Thông tin thanh toán không hợp lệ" });
			}

			try
			{
				var response = _vnpayService.CreatePaymentUrl(request);

				if (response.Success && !string.IsNullOrEmpty(response.PaymentUrl))
				{
					return Ok(new { success = true, paymentUrl = response.PaymentUrl });
				}
				else
				{
					return BadRequest(new { success = false, message = "Không thể tạo URL thanh toán" });
				}
			}
			catch (System.Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		/// <summary>
		/// Callback tu VNPay sau khi thanh toan
		/// Day la URL ma VNPay redirect user ve sau khi thanh toan
		/// </summary>
		[HttpGet("callback")]
		public async Task<IActionResult> Callback()
		{
			try
			{
				var response = _vnpayService.ProcessCallback(Request.Query);

				// Lay orderId tu response
				if (string.IsNullOrEmpty(response.OrderId) || !int.TryParse(response.OrderId, out var orderId))
				{
					Logger.Error($"VNPay callback: Khong lay duoc OrderId tu response");
					TempData["PaymentError"] = "Khong xac dinh duoc don hang.";
					return Redirect("/HomeCustomer/Cart");
				}

				if (response.Success)
				{
					// THANH TOAN THANH CONG
					Logger.Info($"VNPay callback thanh cong: OrderId={orderId}, TransactionId={response.TransactionId}");

					// Goi service de xac nhan thanh toan
					// - Cap nhat trang thai don hang
					// - Tru stock san pham
					// - Xoa gio hang
					// - Gui email va thong bao
					var confirmResult = await _ordersAppService.ConfirmVNPayPayment(orderId, response.TransactionId);
					
					if (!confirmResult)
					{
						Logger.Error($"VNPay callback: Xac nhan thanh toan that bai cho don hang {orderId}");
						TempData["PaymentError"] = "Co loi xay ra khi xu ly don hang.";
						return Redirect("/HomeCustomer/Cart");
					}

					return Redirect($"/Orders/OrderConfirmation?orderId={orderId}");
				}
				else
				{
					// THANH TOAN THAT BAI
					Logger.Warn($"VNPay callback that bai: OrderId={orderId}, ResponseCode={response.ResponseCode}, Message={response.Message}");

					// Goi service de huy don hang
					// Don hang se chuyen sang trang thai "Thanh toan that bai"
					// KHONG can hoan stock vi chua tru
					await _ordersAppService.CancelVNPayPayment(orderId, response.Message);

					TempData["PaymentError"] = "Thanh toán không thành công";
					TempData["PaymentMessage"] = response.Message;
					TempData["PaymentResponseCode"] = response.ResponseCode;

					// Redirect ve trang gio hang de user co the thu lai
					return Redirect("/HomeCustomer/Cart");
				}
			}
			catch (System.Exception ex)
			{
				Logger.Error("VNPay callback error", ex);

				TempData["PaymentError"] = "Co loi xay ra trong qua trinh thanh toan.";
				TempData["PaymentMessage"] = ex.Message;

				return Redirect("/HomeCustomer/Cart");
			}
		}

		/// <summary>
		/// IPN (Instant Payment Notification) tu VNPay
		/// Day la URL ma VNPay goi truc tiep (server-to-server) de thong bao ket qua
		/// IPN duoc goi doc lap voi callback, dam bao xu ly ngay ca khi user dong trinh duyet
		/// </summary>
		[HttpGet("ipn")]
		public async Task<IActionResult> Ipn()
		{
			try
			{
				var response = _vnpayService.ProcessCallback(Request.Query);

				// Lay orderId tu response
				if (string.IsNullOrEmpty(response.OrderId) || !int.TryParse(response.OrderId, out var orderId))
				{
					Logger.Error($"VNPay IPN: Khong lay duoc OrderId tu response");
					return Ok(new { RspCode = "99", Message = "Invalid OrderId" });
				}

				if (response.Success)
				{
					Logger.Info($"VNPay IPN thanh cong: OrderId={orderId}, TransactionId={response.TransactionId}");

					// Goi service de xac nhan thanh toan
					// IPN co the duoc goi truoc hoac sau callback
					// Service se kiem tra trang thai don hang truoc khi xu ly
					var confirmResult = await _ordersAppService.ConfirmVNPayPayment(orderId, response.TransactionId);

					if (confirmResult)
					{
						return Ok(new { RspCode = "00", Message = "Confirm Success" });
					}
					else
					{
						// Don hang da duoc xu ly truoc do (co the tu callback)
						return Ok(new { RspCode = "02", Message = "Order already processed" });
					}
				}
				else
				{
					Logger.Warn($"VNPay IPN that bai: OrderId={orderId}, Message={response.Message}");

					// Huy don hang
					await _ordersAppService.CancelVNPayPayment(orderId, response.Message);

					return Ok(new { RspCode = "00", Message = "Cancelled" });
				}
			}
			catch (System.Exception ex)
			{
				Logger.Error("VNPay IPN error", ex);
				return Ok(new { RspCode = "99", Message = ex.Message });
			}
		}
	}
}
