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

		Task<PagedResultDto<OrderListDto>> GetAllOrder(GetAllOrderInput input);
		
		Task<PagedResultDto<OrderListDto>> GetAllOrderForUser(GetAllOrderInput input);

		Task<OrdersDto> GetOrder(int OrderId);

		Task<OrdersDto> GetOrderByUserId();

		Task<List<OrdersDto>> GetOrdersByUserId();
		
		Task ApproveOrder(int orderId);
		
		Task RejectOrder(int orderId);
		
		Task CancelOrder(int orderId);
		
		Task ReorderOrder(int orderId);
		
		Task CompleteOrder(int orderId);

		/// <summary>
		/// Lấy thông tin vị trí trong queue
		/// </summary>
		OrderQueueTicket GetQueueTicketInfo(string ticketId);

		/// <summary>
		/// Lấy độ dài queue cho một sản phẩm
		/// </summary>
		int GetProductQueueLength(int productVariantId);
	}
}
