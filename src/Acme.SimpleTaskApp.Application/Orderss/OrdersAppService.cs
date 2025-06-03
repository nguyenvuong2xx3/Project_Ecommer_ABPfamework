using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.OrderItems;
using Acme.SimpleTaskApp.Orders.Dtos;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders
{
	public class OrdersAppService : ApplicationService, IOrdersAppService
	{
		private readonly IRepository<Order, int> _ordersRepository;
		private readonly IRepository<User, long> _userRepository;
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<OrderDetails, int> _orderDetailsRepository;
		public OrdersAppService(IRepository<Order, int> orderRepository, IRepository<OrderDetails, int> orderDetailsRepository, IRepository<Product> productRepository, 
			IRepository<User, long> userRepository)
		{
			_userRepository = userRepository;
			_ordersRepository = orderRepository;
			_productRepository = productRepository;
			_orderDetailsRepository = orderDetailsRepository;
			_userRepository = userRepository;
		}
		public async Task CreateOrder(CreateOrderInput input)
		{
			Order order = new Order
			{
				PaymentMethod = input.PaymentMethod,
				UserId = input.UserId,
				Status = 0
			};
			var orderId = await _ordersRepository.InsertAndGetIdAsync(order);
			foreach(var orderDetailDto in input.OrderDetails)
			{
				var getProduct = _productRepository.GetAll().FirstOrDefault(u => u.Id == orderDetailDto.ProductId);
				OrderDetails orderDetail = new OrderDetails
				{
					ProductId = orderDetailDto.ProductId,
					OrderId = orderId, // Associate the order detail with the created order
					Quantity = orderDetailDto.Quantity,
					NewPrice = getProduct.Price
				};
				await _orderDetailsRepository.InsertAsync(orderDetail);
			}
		}
		public async Task<PagedResultDto<CreateOrderInput>> GetAllOrder(GetAllOrderInput input)
		{
			var query = _ordersRepository.GetAll(); // Correctly call the GetAll method

					query = query
					.WhereIf(!string.IsNullOrEmpty(input.Search), o => o.PaymentMethod.ToString().Contains(input.Search) || o.UserId.ToString().Contains(input.Search) || o.Status.ToString().Contains(input.Search)); // Adjusted to use 'Search' property from GetAllOrderInput

			var InfoUser = await _userRepository.GetAll()
					.Select(u => u.Id)
					.ToListAsync();


			var getallOrders = await query
					.Select(o => new CreateOrderInput
					{
						PaymentMethod = o.PaymentMethod,
						UserId = o.UserId,
						Status = o.Status,
						OrderDetails = _orderDetailsRepository.GetAll().Include(u => u.Product)
									.Where(od => od.OrderId == o.Id)
									.Select(od => new OrderDetailDto
									{
										ImageUrl = od.Product.Image, // Assuming Product has an Image property
										NewPrice = od.Product.Price, // Assuming NewPrice is a property in OrderDetails
										ProductName = od.Product.Name, // Assuming Product has a Name property
										ProductId = od.ProductId,
										Quantity = od.Quantity
									}).ToList()
					})
					.OrderByDescending(o => o.PaymentMethod) // Adjusted ordering to a valid property
					.PageBy(input)
					.ToListAsync();

			return new PagedResultDto<CreateOrderInput>(getallOrders.Count, getallOrders);
		}

		//public async Task<OrderListDto> GetUserOrders(GetAllOrderInput input)
		//{
		//	var currentUser = AbpSession.UserId;
		//	if (currentUser == null)
		//	{
		//		throw new UserFriendlyException("User is not logged in.");
		//	}

		//	var query = _ordersRepository.GetAll()
		//			.Where(o => o.UserId == currentUser);

		//	var totalCount = await query.CountAsync();

		//	var orders = await query
		//			.OrderByDescending(o => o.CreationTime)
		//			.PageBy(input)
		//			.Select(o => new CreateOrderInput
		//			{
		//				PaymentMethod = o.PaymentMethod, // trả về chuỗi
		//																										// hoặc PaymentMethod = (int)o.PaymentMethod, // trả về số nếu muốn
		//				Status = o.Status,
		//						// hoặc Status = (int)o.Status,
		//						OrderDetails = _orderDetailsRepository.GetAll()
		//				.Where(od => od.OrderId == o.Id)
		//				.Select(od => new OrderDetailDto
		//				{
		//					ProductId = od.ProductId,
		//					ProductName = od.Product.Name,
		//					ImageUrl = od.Product.Image,
		//					NewPrice = od.Product.Price,
		//					Quantity = od.Quantity
		//				}).ToList()
		//			})

		//	return new OrderListDto
		//	{
		//		Orders = orders,
		//		TotalCount = totalCount
		//	};
		//}
	}
}
