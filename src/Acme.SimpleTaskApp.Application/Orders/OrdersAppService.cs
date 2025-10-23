using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.OrderItems;
using Acme.SimpleTaskApp.Orders.Dtos;
using Acme.SimpleTaskApp.Products;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders
{
	public class OrdersAppService : ApplicationService, IOrdersAppService
	{
		private readonly IRepository<Order, int> _ordersRepository;
		private readonly IRepository<User, long> _userRepository;
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<ProductVariant> _productVariantRepository;
		private readonly IRepository<OrderDetails, int> _orderDetailsRepository;
		private readonly OrderQueueService _orderQueueService;

		public OrdersAppService(
			IRepository<Order, int> orderRepository,
			IRepository<OrderDetails, int> orderDetailsRepository,
			IRepository<Product> productRepository,
			IRepository<ProductVariant> productVariantRepository,
			IRepository<User, long> userRepository,
			OrderQueueService orderQueueService)
		{
			_userRepository = userRepository;
			_ordersRepository = orderRepository;
			_productRepository = productRepository;
			_productVariantRepository = productVariantRepository;
			_orderDetailsRepository = orderDetailsRepository;
			_orderQueueService = orderQueueService;
		}

		public async Task<int> CreateOrder(CreateOrderInput input)
		{
			decimal totalPrice = 0;
			var orderDetailsList = new List<OrderDetails>();

			// Bước 1: Validate đầu vào
			foreach (var item in input.OrderDetails)
			{
				var productVariant = await _productVariantRepository.GetAll()
					.Where(pv => pv.Id == item.ProductId)
					.FirstOrDefaultAsync();

				if (productVariant == null)
				{
					throw new UserFriendlyException($"Sản phẩm với ID {item.ProductId} không tồn tại.");
				}

				// Tính tổng giá
				totalPrice += item.Quantity * productVariant.Price;

				// Lưu thông tin để xử lý sau
				orderDetailsList.Add(new OrderDetails
				{
					ProductVariantId = item.ProductId,
					Quantity = item.Quantity,
					NewPrice = productVariant.Price
				});
			}

			// Bước 2: Thêm vào Queue và chờ xử lý
			// Sắp xếp theo ID để đảm bảo thứ tự lock nhất quán
			var sortedProductIds = input.OrderDetails
				.Select(x => x.ProductId)
				.OrderBy(x => x)
				.Distinct()
				.ToList();

			int orderId = 0;
			var exceptions = new List<Exception>();

			// Xử lý từng ProductVariant trong queue
			foreach (var productId in sortedProductIds)
			{
				var orderDetail = orderDetailsList.First(od => od.ProductVariantId == productId);

				var ticket = await _orderQueueService.EnqueueOrderRequest(
					productId,
					orderDetail.Quantity,
					async () =>
					{
						try
						{
							// Kiểm tra lại stock trong queue (critical section)
							var productVariant = await _productVariantRepository.GetAsync(productId);

							if (productVariant.StockQuantity < orderDetail.Quantity)
							{
								throw new UserFriendlyException(
									$"Sản phẩm '{productVariant.Color} {productVariant.Storage}' chỉ còn {productVariant.StockQuantity} sản phẩm. " +
									$"Bạn không thể đặt {orderDetail.Quantity} sản phẩm.");
							}

							// Tạo order nếu chưa có
							if (orderId == 0)
							{
								Order order = new Order
								{
									PaymentMethod = input.PaymentMethod,
									UserId = input.UserId,
									Status = input.Status,
									TotalPrice = totalPrice
								};
								orderId = await _ordersRepository.InsertAndGetIdAsync(order);
							}

							// Tạo OrderDetail
							orderDetail.OrderId = orderId;
							await _orderDetailsRepository.InsertAsync(orderDetail);

							// Trừ stock
							productVariant.StockQuantity -= orderDetail.Quantity;
							await _productVariantRepository.UpdateAsync(productVariant);

							// Cập nhật tổng stock của Product
							var product = await _productRepository.GetAsync(productVariant.ProductId);
							product.StockQuantity = await _productVariantRepository.GetAll()
								.Where(pv => pv.ProductId == product.Id)
								.SumAsync(pv => pv.StockQuantity);
							await _productRepository.UpdateAsync(product);

							await CurrentUnitOfWork.SaveChangesAsync();
						}
						catch (Exception ex)
						{
							exceptions.Add(ex);
							throw;
						}
					});

				// Chờ ticket được xử lý xong
				await WaitForTicketCompletionAsync(ticket);
			}

			// Kiểm tra xem có lỗi nào không
			if (exceptions.Any())
			{
				throw exceptions.First();
			}

			return orderId;
		}

		/// <summary>
		/// Chờ ticket được xử lý xong
		/// </summary>
		private async Task WaitForTicketCompletionAsync(OrderQueueTicket ticket)
		{
			// Polling để kiểm tra trạng thái (có thể cải thiện bằng SignalR hoặc WebSocket)
			var maxWaitTime = TimeSpan.FromSeconds(30);
			var startTime = DateTime.UtcNow;

			while (DateTime.UtcNow - startTime < maxWaitTime)
			{
				var currentTicket = _orderQueueService.GetTicketInfo(ticket.TicketId);

				if (currentTicket == null)
				{
					throw new UserFriendlyException("Ticket không tồn tại.");
				}

				if (currentTicket.Status == OrderQueueStatus.Completed)
				{
					return; // Thành công
				}

				if (currentTicket.Status == OrderQueueStatus.Failed)
				{
					throw new UserFriendlyException(currentTicket.ErrorMessage ?? "Đặt hàng thất bại");
				}

				// Chờ 100ms trước khi kiểm tra lại
				await Task.Delay(100);
			}

			throw new UserFriendlyException("Timeout: Quá thời gian chờ xử lý đơn hàng");
		}

		public async Task<PagedResultDto<OrderListDto>> GetAllOrder(GetAllOrderInput input)
		{
			var query = _ordersRepository.GetAll();

			// Filter theo NameUser (tên user)
			if (!string.IsNullOrWhiteSpace(input.UserName))
			{
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

			query = query.Where(order => order.UserId == currentUserId);

			if (!string.IsNullOrWhiteSpace(input.UserName))
			{
				query = query.Where(order =>
						_userRepository.GetAll()
								.Any(u => u.Id == order.UserId &&
													(u.Name.Contains(input.UserName) || u.UserName.Contains(input.UserName))));
			}

			if (input.PaymentMethod.HasValue)
			{
				query = query.Where(order => order.PaymentMethod == input.PaymentMethod.Value);
			}

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
					.ThenInclude(od => od.ProductVariant)
					.ThenInclude(pv => pv != null ? _productRepository.GetAll().Where(p => p.Id == pv.ProductId).FirstOrDefault() : null)
				.FirstOrDefaultAsync();

			var user = _userRepository.GetAll().FirstOrDefault(u => u.Id == order.UserId);

			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}

			var orderDetails = new List<OrderDetailDto>();
			foreach (var od in order.OrderDetails)
			{
				var productVariant = await _productVariantRepository.GetAll()
					.Where(pv => pv.Id == od.ProductVariantId)
					.FirstOrDefaultAsync();

				var product = productVariant != null
					? await _productRepository.GetAsync(productVariant.ProductId)
					: null;

				var imageUrl = productVariant != null
					? await _productVariantRepository.GetAll()
						.Where(pv => pv.Id == productVariant.Id)
						.Select(pv => pv.ImageUrl)
						.FirstOrDefaultAsync()
					: null;

				orderDetails.Add(new OrderDetailDto
				{
					ProductId = od.ProductVariantId,
					ProductName = product != null && productVariant != null
						? $"{product.Name} - {productVariant.Color} {productVariant.Storage}"
						: "Unknown",
					ImageUrl = imageUrl,
					NewPrice = od.NewPrice,
					Quantity = od.Quantity,
					TotalUnit = od.NewPrice * od.Quantity
				});
			}

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

			// Khi từ chối đơn, cần hoàn trả stock
			await RestoreStockForOrder(orderId);

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

			// Khi hủy đơn, cần hoàn trả stock
			await RestoreStockForOrder(orderId);

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

			// Kiểm tra stock trước khi đặt lại
			var orderDetails = await _orderDetailsRepository.GetAll()
				.Where(od => od.OrderId == orderId)
				.ToListAsync();

			foreach (var detail in orderDetails)
			{
				var productVariant = await _productVariantRepository.GetAsync(detail.ProductVariantId);
				if (productVariant.StockQuantity < detail.Quantity)
				{
					throw new UserFriendlyException(
						$"Không đủ hàng để đặt lại đơn. Sản phẩm chỉ còn {productVariant.StockQuantity} trong khi bạn cần {detail.Quantity}.");
				}
			}

			// Trừ stock lại
			foreach (var detail in orderDetails)
			{
				var productVariant = await _productVariantRepository.GetAsync(detail.ProductVariantId);
				productVariant.StockQuantity -= detail.Quantity;
				await _productVariantRepository.UpdateAsync(productVariant);
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
						.Where(o => o.UserId == currentUserId && (o.Status == 0 || o.Status == 1))
						.Include(o => o.OrderDetails)
						.ThenInclude(od => od.ProductVariant)
						.OrderByDescending(o => o.CreationTime)
						.FirstOrDefaultAsync();

			var user = _userRepository.GetAll().FirstOrDefault(u => u.Id == currentUserId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}

			var orderDetails = new List<OrderDetailDto>();
			foreach (var od in order.OrderDetails)
			{
				var productVariant = await _productVariantRepository.GetAll()
					.Where(pv => pv.Id == od.ProductVariantId)
					.FirstOrDefaultAsync();

				var product = productVariant != null
					? await _productRepository.GetAsync(productVariant.ProductId)
					: null;

				var imageUrl = productVariant?.ImageUrl;

				orderDetails.Add(new OrderDetailDto
				{
					ProductId = od.ProductVariantId,
					ProductName = product != null && productVariant != null
						? $"{product.Name} - {productVariant.Color} {productVariant.Storage}"
						: "Unknown",
					ImageUrl = imageUrl,
					NewPrice = od.NewPrice,
					Quantity = od.Quantity,
					TotalUnit = od.NewPrice * od.Quantity
				});
			}

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

			var orders = await _ordersRepository.GetAll()
					.Where(o => o.UserId == currentUserId)
					.Include(o => o.OrderDetails)
					.ThenInclude(od => od.ProductVariant)
					.OrderByDescending(o => o.CreationTime)
					.ToListAsync();

			if (!orders.Any())
			{
				throw new UserFriendlyException("No orders found.");
			}

			var user = await _userRepository.GetAsync(currentUserId);

			var orderDtos = new List<OrdersDto>();

			foreach (var order in orders)
			{
				var orderDetails = new List<OrderDetailDto>();

				foreach (var od in order.OrderDetails)
				{
					var productVariant = await _productVariantRepository.GetAll()
						.Where(pv => pv.Id == od.ProductVariantId)
						.FirstOrDefaultAsync();

					var product = productVariant != null
						? await _productRepository.GetAsync(productVariant.ProductId)
						: null;

					var imageUrl = productVariant?.ImageUrl;

					orderDetails.Add(new OrderDetailDto
					{
						ProductId = od.ProductVariantId,
						ProductName = product != null && productVariant != null
							? $"{product.Name} - {productVariant.Color} {productVariant.Storage}"
							: "Unknown",
						ImageUrl = imageUrl,
						NewPrice = od.NewPrice,
						Quantity = od.Quantity,
						TotalUnit = od.NewPrice * od.Quantity
					});
				}

				orderDtos.Add(new OrdersDto
				{
					OrderDate = order.CreationTime.ToString("yyyy-MM-dd HH:mm"),
					PaymentMethod = order.PaymentMethod,
					UserName = user.Name,
					EmailAddress = user.EmailAddress,
					Status = order.Status,
					TotalPrice = order.TotalPrice,
					OrderDetails = orderDetails
				});
			}

			return orderDtos;
		}

		// Helper method để hoàn trả stock khi hủy/từ chối đơn
		private async Task RestoreStockForOrder(int orderId)
		{
			var orderDetails = await _orderDetailsRepository.GetAll()
				.Where(od => od.OrderId == orderId)
				.ToListAsync();

			foreach (var detail in orderDetails)
			{
				var productVariant = await _productVariantRepository.GetAsync(detail.ProductVariantId);
				productVariant.StockQuantity += detail.Quantity;
				await _productVariantRepository.UpdateAsync(productVariant);

				// Cập nhật tổng stock của Product
				var product = await _productRepository.GetAsync(productVariant.ProductId);
				product.StockQuantity = await _productVariantRepository.GetAll()
					.Where(pv => pv.ProductId == product.Id)
					.SumAsync(pv => pv.StockQuantity);
				await _productRepository.UpdateAsync(product);
			}
		}

		/// <summary>
		/// Lấy thông tin ticket trong queue
		/// </summary>
		public OrderQueueTicket GetQueueTicketInfo(string ticketId)
		{
			return _orderQueueService.GetTicketInfo(ticketId);
		}

		/// <summary>
		/// Lấy độ dài queue cho một ProductVariant
		/// </summary>
		public int GetProductQueueLength(int productVariantId)
		{
			return _orderQueueService.GetQueueLength(productVariantId);
		}
	}
}
