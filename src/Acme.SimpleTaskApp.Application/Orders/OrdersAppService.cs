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
		public async Task<int> CreateOrder(CreateOrderInput input)
		{
			decimal totalPrice = 0;
			foreach (var item in input.OrderDetails)
			{
				var getProduct = _productRepository.GetAll().FirstOrDefault(u => u.Id == item.ProductId);
				totalPrice += item.Quantity * getProduct.Price;
			}
			Order order = new Order
			{
				PaymentMethod = input.PaymentMethod,
				UserId = input.UserId,
				Status = 0,
				TotalPrice = totalPrice
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
			return orderId;
		}

		public async Task<PagedResultDto<OrderListDto>> GetAllOrder(GetAllOrderInput input)
		{
			var query = _ordersRepository.GetAll();

			// Filter theo NameUser (tên user)
			if (!string.IsNullOrWhiteSpace(input.UserName))
			{
				// Lọc những order mà User liên quan có Name hoặc UserName chứa input.NameUser
				query = query.Where(order =>
						_userRepository.GetAll()
								.Any(u => u.Id == order.UserId &&
													(u.Name.Contains(input.UserName) || u.UserName.Contains(input.UserName))));
			}

			// Filter theo PaymentMethod nếu có
			if (input.PaymentMethod.HasValue)
			{
				query = query.Where(order => order.PaymentMethod == input.PaymentMethod.Value);
			}

			// Filter theo Status (OrderStatus) nếu có
			if (input.Status.HasValue)
			{
				query = query.Where(order => order.Status == input.Status.Value);
			}

			var count = await query.CountAsync();

			var result = await query
					.OrderByDescending(o => o.CreationTime)
					.PageBy(input) 
					.ToListAsync();

			var orderListDtos = result.Select(order =>
			{
				var user = _userRepository.GetAll().FirstOrDefault(u => u.Id == order.UserId);
				return new OrderListDto
				{
					Id = order.Id,
					UserName = user?.UserName,
					Name = user?.Name,
					PaymentMethod = order.PaymentMethod,
					Status = order.Status,
					CreationTime = order.CreationTime.ToString("yyyy-MM-dd HH:mm"),
					TotalCount = order.TotalPrice
				};
			}).ToList();

			return new PagedResultDto<OrderListDto>(count, orderListDtos);
		}
		public async Task<PagedResultDto<OrderListDto>> GetAllOrderForUser(GetAllOrderInput input)
		{
			var query = _ordersRepository.GetAll();
			var currentUserId = AbpSession.UserId ?? throw new UserFriendlyException("Cannot find user");
			// Filter theo NameUser (tên user)
			query.Where(order => order.UserId == currentUserId);
			if (!string.IsNullOrWhiteSpace(input.UserName))
			{
				// Lọc những order mà User liên quan có Name hoặc UserName chứa input.NameUser
				query = query.Where(order =>
						_userRepository.GetAll()
								.Any(u => u.Id == order.UserId &&
													(u.Name.Contains(input.UserName) || u.UserName.Contains(input.UserName))));
			}

			// Filter theo PaymentMethod nếu có
			if (input.PaymentMethod.HasValue)
			{
				query = query.Where(order => order.PaymentMethod == input.PaymentMethod.Value);
			}

			// Filter theo Status (OrderStatus) nếu có
			if (input.Status.HasValue)
			{
				query = query.Where(order => order.Status == input.Status.Value);
			}

			var count = await query.CountAsync();

			var result = await query
					.OrderByDescending(o => o.CreationTime)
					.PageBy(input)
					.ToListAsync();

			var orderListDtos = result.Select(order =>
			{
				var user = _userRepository.GetAll().FirstOrDefault(u => u.Id == order.UserId);
				return new OrderListDto
				{
					Id = order.Id,
					UserName = user?.UserName,
					Name = user?.Name,
					PaymentMethod = order.PaymentMethod,
					Status = order.Status,
					CreationTime = order.CreationTime.ToString("yyyy-MM-dd HH:mm"),
					TotalCount = order.TotalPrice
				};
			}).ToList();

			return new PagedResultDto<OrderListDto>(count, orderListDtos);
		}

		public async Task<OrdersDto> GetOrder(int orderId)
		{
			var order = await _ordersRepository.GetAll()
					.Where(o => o.Id == orderId)
					.Include(o => o.OrderDetails)
					.ThenInclude(od => od.Product)
				.FirstOrDefaultAsync();
			var user = _userRepository.GetAll().FirstOrDefault(u => u.Id == order.UserId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}

			var orderDetails = order.OrderDetails.Select(od => new OrderDetailDto
			{
				ProductId = od.ProductId,
				ProductName = od.Product.Name,
				ImageUrl = od.Product.Image,
				NewPrice = od.NewPrice,
				Quantity = od.Quantity,
				TotalUnit = od.NewPrice * od.Quantity
			}).ToList();

			return new OrdersDto
			{
				PaymentMethod = order.PaymentMethod,
				UserName = user.Name,
				EmailAddress = user.EmailAddress,
				Status = order.Status,
				OrderDetails = orderDetails,
				TotalPrice = order.TotalPrice
			};
		}
		public async Task ApproveOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Status = 1;
			await _ordersRepository.UpdateAsync(order);
		}
		public async Task RejectOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Status = 3;
			await _ordersRepository.UpdateAsync(order);
		}

		public async Task CancelOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Status = 4; 
			await _ordersRepository.UpdateAsync(order);
		}
		public async Task ReorderOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Status = 0;
			await _ordersRepository.UpdateAsync(order);
		}
		public async Task CompleteOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Status = 2;
			await _ordersRepository.UpdateAsync(order);
		}

		public async Task<OrdersDto> GetOrderByUserId()
		{
			var currentUserId = AbpSession.UserId ?? throw new UserFriendlyException("Cannot find user");

			var order = await _ordersRepository.GetAll()
						.Where(o => o.UserId == currentUserId && o.Status == 0 || o.Status == 1)
						.Include(o => o.OrderDetails)
						.ThenInclude(od => od.Product)
						.OrderByDescending(o => o.CreationTime)
						.FirstOrDefaultAsync();

			var user = _userRepository.GetAll().FirstOrDefault(u => u.Id == currentUserId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			var orderDetails = order.OrderDetails.Select(od => new OrderDetailDto
			{
				ProductId = od.ProductId,
				ProductName = od.Product.Name,
				ImageUrl = od.Product.Image,
				NewPrice = od.NewPrice,
				Quantity = od.Quantity,
				TotalUnit = od.NewPrice * od.Quantity
			}).ToList();

			return new OrdersDto
			{
				PaymentMethod = order.PaymentMethod,
				UserName = user.Name,
				EmailAddress = user.EmailAddress,
				Status = order.Status,
				OrderDetails = orderDetails,
				TotalPrice = order.TotalPrice
			};
		}
		public async Task<List<OrdersDto>> GetOrdersByUserId()
		{
			var currentUserId = AbpSession.UserId ?? throw new UserFriendlyException("Cannot find user");

			// Lấy tất cả đơn hàng của user, sắp xếp từ mới đến cũ
			var orders = await _ordersRepository.GetAll()
					.Where(o => o.UserId == currentUserId)
					.Include(o => o.OrderDetails)
					.ThenInclude(od => od.Product)
					.OrderByDescending(o => o.CreationTime)
					.ToListAsync();

			if (!orders.Any())
			{
				throw new UserFriendlyException("No orders found.");
			}

			var user = await _userRepository.GetAsync(currentUserId);

			// Map từng đơn hàng sang DTO
			var orderDtos = orders.Select(order => new OrdersDto
			{
				OrderDate = order.CreationTime.ToString("yyyy-MM-dd HH:mm"),
				PaymentMethod = order.PaymentMethod,
				UserName = user.Name,
				EmailAddress = user.EmailAddress,
				Status = order.Status,
				TotalPrice = order.TotalPrice,
				OrderDetails = order.OrderDetails.Select(od => new OrderDetailDto
				{
					ProductId = od.ProductId,
					ProductName = od.Product?.Name,
					ImageUrl = od.Product?.Image,
					NewPrice = od.NewPrice,
					Quantity = od.Quantity,
					TotalUnit = od.NewPrice * od.Quantity
				}).ToList()
			}).ToList();

			return orderDtos;
		}
	}
}
