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

		Task<OrdersDto> GetOrder(int OrderId);

		Task<OrdersDto> GetOrderByUserId();

		Task<List<OrdersDto>> GetOrdersByUserId();
	}
}
