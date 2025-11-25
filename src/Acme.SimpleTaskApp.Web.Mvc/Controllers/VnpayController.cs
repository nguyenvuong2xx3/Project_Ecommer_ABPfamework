using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Domain.Repositories;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Payment.VNPay;
using Microsoft.AspNetCore.Mvc;

namespace Acme.SimpleTaskApp.Web.Controllers
{
    [Route("api/[controller]")]
    public class VnpayController : AbpController
    {
        private readonly IVnpayAppService _vnpayService;
        private readonly IRepository<Cart, int> _cartRepository;
        private readonly IRepository<CartItem, int> _cartItemRepository;

        public VnpayController(
            IVnpayAppService vnpayService,
            IRepository<Cart, int> cartRepository,
            IRepository<CartItem, int> cartItemRepository)
        {
            _vnpayService = vnpayService;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
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
        /// Callback từ VNPay sau khi thanh toán
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            try
            {
                var response = _vnpayService.ProcessCallback(Request.Query);

                if (response.Success)
                {
                    // ✅ SUCCESS: Xóa cart và redirect đến OrderConfirmation
                    Logger.Info($"VNPay payment success: OrderId={response.OrderId}, TransactionId={response.TransactionId}");
                    
                    // Xóa cart của user sau khi thanh toán thành công
                    if (AbpSession.UserId.HasValue)
                    {
                        var getCart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == AbpSession.UserId);
                        if (getCart != null)
                        {
                            await _cartItemRepository.DeleteAsync(x => x.CartId == getCart.Id);
                            await _cartRepository.DeleteAsync(getCart);
                            Logger.Info($"Cart deleted for user {AbpSession.UserId.Value} after VNPay payment success");
                        }
                    }
                    
                    return Redirect($"/Orders/OrderConfirmation?orderId={response.OrderId}");
                }
                else
                {
                    // ❌ FAILED: Giữ cart và redirect đến Cart với thông báo lỗi
                    Logger.Warn($"VNPay payment failed: OrderId={response.OrderId}, ResponseCode={response.ResponseCode}, Message={response.Message}");
                    
                    TempData["PaymentError"] = "Đơn hàng chưa được thanh toán. Vui lòng thử lại.";
                    TempData["PaymentMessage"] = response.Message;
                    TempData["PaymentResponseCode"] = response.ResponseCode;
                    
                    // Cart vẫn còn, user có thể thử thanh toán lại
                    return Redirect("/HomeCustomer/Cart");
                }
            }
            catch (System.Exception ex)
            {
                // ❌ EXCEPTION: Giữ cart và redirect đến Cart với thông báo lỗi
                Logger.Error("VNPay callback error", ex);
                
                TempData["PaymentError"] = "Có lỗi xảy ra trong quá trình thanh toán.";
                TempData["PaymentMessage"] = ex.Message;
                
                return Redirect("/HomeCustomer/Cart");
            }
        }

        /// <summary>
        /// IPN (Instant Payment Notification) từ VNPay
        /// </summary>
        [HttpGet("ipn")]
        public async Task<IActionResult> Ipn()
        {
            try
            {
                var response = _vnpayService.ProcessCallback(Request.Query);

                if (response.Success)
                {
                    Logger.Info($"VNPay IPN success: OrderId={response.OrderId}, TransactionId={response.TransactionId}");
                    
                    // TODO: Cập nhật trạng thái đơn hàng trong database
                    // Example:
                    // await _orderAppService.UpdatePaymentStatus(response.OrderId, PaymentStatus.Paid, response.TransactionId);

                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }
                else
                {
                    Logger.Warn($"VNPay IPN failed: OrderId={response.OrderId}, Message={response.Message}");
                    return Ok(new { RspCode = "99", Message = response.Message });
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
