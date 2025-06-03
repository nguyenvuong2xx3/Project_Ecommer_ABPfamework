//using Abp.Application.Services;
//using Abp.Application.Services.Dto;
//using Abp.Domain.Repositories;
//using Abp.Linq.Extensions;
//using Acme.SimpleTaskApp.Orders.Dto;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Acme.SimpleTaskApp.Orders
//{
//	public class OrderAppService : ApplicationService, IOrderAppService
//	{
//		private readonly IRepository<Order> _orderAppService;

//		public OrderAppService(IRepository<Order> orderAppService)
//		{
//			_orderAppService = orderAppService;
//		}

//		public async Task<PagedResultDto<OrderListDto>> GetAllOrder(GetAllOrdersInput input)
//		{

//			var orders = _orderAppService.GetAll();

//			//if (!string.IsNullOrEmpty(input.NameUser))
//			//{
//			//	orders = orders.Where(o => o.NameUser.Contains(input.NameUser));
//			//}

//			if (input.OrderStatus != null)
//			{
//				orders = orders.Where(o => o.Status == input.OrderStatus);
//			}

//			if (input.PaymentMethod != null)
//			{
//				orders = orders.Where(o => o.PaymentMethod == input.PaymentMethod);
//			}

//			var counts = await orders.CountAsync();

//			var orderDtos = orders.PageBy(input).Select(o => new OrderListDto
//			{
//				Id = o.Id,
//				UserId = o.UserId,
//				NameUser = o.NameUser,
//				TotalAmount = o.totalAmount,
//				PaymentMethod = o.PaymentMethod,
//				CreationTime = o.CreationTime,
//				OrderStatus = o.OrderStatus,
//			}).ToList();

//			return new PagedResultDto<OrderListDto>(counts, orderDtos);
//		}
//		public async Task<int> CreateOrder(CreateOrderDto input)
//		{
//			try
//			{
//				var order = new Order
//				{
//					UserId = input.UserId,
//					NameUser = input.NameUser,
//					totalAmount = input.TotalAmount,
//					DiscountAmount = input.DiscountAmount,
//					OrderStatus = input.OrderStatus,
//					PaymentMethod = input.PaymentMethod
//				};
//				var result = await _orderAppService.InsertAndGetIdAsync(order);
//				return result;
//			}
//			catch (Exception ex)
//			{
//				throw;
//			}
//		}

//		public async Task<List<OrderOutput>> GetStatusOrder(int? orderStatus = 5)
//		{
//			if (AbpSession.UserId != null)
//			{
//				var userId = AbpSession.UserId;

//				//List<Order> orderList;
//				List<Order> orderList;

//				if (orderStatus == 5)
//				{
//					orderList = await _orderAppService.GetAllListAsync(x => x.UserId == userId);
//				}
//				else
//				{
//					orderList = await _orderAppService.GetAllListAsync(x => x.OrderStatus == orderStatus && x.UserId == userId);
//				}

//				//var orderIds = orderList.Select(o => o.Id ).ToList();
//				//return orderIds;
//				var orderOutputs = orderList.Select(o => new OrderOutput
//				{
//					OrderId = o.Id,
//					OrderStatus = o.OrderStatus,
//				}).ToList();

//				return orderOutputs;
//			}
//			else
//			{
//				return null;
//			}
//		}
//		public class OrderOutput
//		{
//			public int OrderId { get; set; }
//			public int OrderStatus { get; set; }
//		}
//		public async Task<OrderListDto> GetOrderById(int orderId)
//		{
//			var order = await _orderAppService.FirstOrDefaultAsync(orderId);

//			return new OrderListDto
//			{
//				Id = order.Id,
//				UserId = order.UserId,
//				NameUser = order.NameUser,
//				OrderStatus = order.OrderStatus,
//				TotalAmount = order.totalAmount,
//				PaymentMethod = order.PaymentMethod,
//				DiscountAmount = order.DiscountAmount,
//				CreationTime = order.CreationTime,
//			};
//		}

//		public async Task UpdateOrder(UpdateOrderDto input)
//		{
//			var order = await _orderAppService.GetAsync(input.OrderId);

//			// cập nhật dữ liệu 
//			order.OrderStatus = input.OrderStatus;
//			order.PaymentMethod = input.PaymentMethod;

//			// lưu vào db
//			await _orderAppService.UpdateAsync(order);
//		}
//	}
//}
