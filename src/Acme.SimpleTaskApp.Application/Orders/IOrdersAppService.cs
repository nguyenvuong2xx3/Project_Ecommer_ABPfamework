using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Orders.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders
{
	public interface IOrdersAppService : IApplicationService
	{
		Task<int> CreateOrder(CreateOrderInput input);
		Task<Order> GetOrder(int orderId);
		
		// Cac method cho VNPay payment
		// Xac nhan thanh toan thanh cong - tru stock, cap nhat trang thai
		Task<bool> ConfirmVNPayPayment(int orderId, string transactionId);
		// Huy don hang khi thanh toan that bai
		Task<bool> CancelVNPayPayment(int orderId, string reason);
	}
}
