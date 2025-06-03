//using Abp.Application.Services;
//using Abp.Application.Services.Dto;
//using Acme.SimpleTaskApp.Orders.Dto;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using static Acme.SimpleTaskApp.Orders.OrderAppService;

//namespace Acme.SimpleTaskApp.Orders
//{
//	public interface IOrderAppService : IApplicationService
//	{
//		Task<PagedResultDto<OrderListDto>> GetAllOrder(GetAllOrdersInput input);
//		Task<int> CreateOrder(CreateOrderDto input);
//		Task<List<OrderOutput>> GetStatusOrder(int? orderStatus = 5);
//		//Task<List<int>> GetStatusOrder(int? orderStatus = 5);
//		Task UpdateOrder(UpdateOrderDto input);
//		Task<OrderListDto> GetOrderById(int orderId);
//	}
//}
