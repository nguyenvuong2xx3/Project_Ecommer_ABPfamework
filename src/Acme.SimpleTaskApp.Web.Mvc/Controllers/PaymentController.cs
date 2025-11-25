using Microsoft.AspNetCore.Mvc;
using Acme.SimpleTaskApp.Controllers;

namespace Acme.SimpleTaskApp.Web.Controllers
{
    public class PaymentController : SimpleTaskAppControllerBase
    {
        public IActionResult Success(string orderId, string transactionId)
        {
            ViewBag.OrderId = orderId;
            ViewBag.TransactionId = transactionId;
            return View();
        }

        public IActionResult Failed(string orderId, string message)
        {
            ViewBag.OrderId = orderId;
            ViewBag.Message = message;
            return View();
        }
    }
}
