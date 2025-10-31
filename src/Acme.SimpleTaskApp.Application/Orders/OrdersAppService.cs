using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Notifications;
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
		private readonly INotificationAppService _notificationAppService;
		private readonly IOrderQueueService _orderQueueService;
		private readonly IRepository<Cart, int> _cartRepository;
		private readonly IRepository<CartItem, int> _cartItemRepository;


		public OrdersAppService(
			IRepository<Order, int> orderRepository,
			IRepository<OrderDetails, int> orderDetailsRepository,
			IRepository<Product> productRepository,
			IRepository<ProductVariant> productVariantRepository,
			IRepository<User, long> userRepository,
			INotificationAppService notificationAppService,
			IRepository<CartItem, int> cartItemRepository,
		IRepository<Cart, int> cartRepository,
		IOrderQueueService orderQueueService)
		{
			_cartRepository = cartRepository;
			_cartItemRepository = cartItemRepository;
			_ordersRepository = orderRepository;
			_userRepository = userRepository;
			_ordersRepository = orderRepository;
			_productRepository = productRepository;
			_productVariantRepository = productVariantRepository;
			_orderDetailsRepository = orderDetailsRepository;
			_notificationAppService = notificationAppService;
			_orderQueueService = orderQueueService;
		}

		public async Task<int> CreateOrder(CreateOrderInput input)
		{
			decimal totalPrice = 0;
			var orderDetailsList = new List<OrderDetails>();
			var currentUser = _userRepository.Get(AbpSession.UserId.Value);

			// Cố gắng khóa tất cả sản phẩm trong đơn hàng
			if (!await _orderQueueService.TryLockProductsAsync(input))
			{
				throw new UserFriendlyException("Một số sản phẩm trong đơn hàng không còn đủ số lượng. Vui lòng thử lại.");
			}

			try
			{
				// Validate và tính tổng giá
				foreach (var item in input.OrderDetails)
				{
					var productVariant = await _productVariantRepository.GetAll()
							.Where(pv => pv.Id == item.ProductVariantId)
							.FirstOrDefaultAsync();

					if (productVariant == null)
					{
						throw new UserFriendlyException($"Sản phẩm với ID {item.ProductVariantId} không tồn tại.");
					}

					if (productVariant.StockQuantity < item.Quantity)
					{
						throw new UserFriendlyException(
								$"Sản phẩm '{productVariant.Color} {productVariant.Storage}' chỉ còn {productVariant.StockQuantity} sản phẩm.");
					}

					totalPrice += item.Quantity * item.NewPrice;

					orderDetailsList.Add(new OrderDetails
					{
						ProductVariantId = item.ProductVariantId,
						Quantity = item.Quantity,
						NewPrice = item.NewPrice
					});
				}

				// Tạo Order
				var user = await _userRepository.GetAsync(currentUser.Id);

				Order order = new Order
				{
					PaymentMethod = input.Order.PaymentMethod,
					UserId = input.Order.UserId ?? user.Id,
					Status = 0,
					TotalPrice = totalPrice,
					FullName = string.IsNullOrWhiteSpace(input.Order.FullName) ? user.Name : input.Order.FullName,
					GioiTinh = input.Order.GioiTinh != null && input.Order.GioiTinh > 0 ? user.GioiTinh : input.Order.GioiTinh,
					TinhThanh = string.IsNullOrWhiteSpace(input.Order.TinhThanh) ? user.TinhThanh : input.Order.TinhThanh,
					PhuongXa = string.IsNullOrWhiteSpace(input.Order.PhuongXa) ? user.PhuongXa : input.Order.PhuongXa,
					DiaChiChiTiet = string.IsNullOrWhiteSpace(input.Order.DiaChiChiTiet) ? user.DiaChiChiTiet : input.Order.DiaChiChiTiet
				};


				int orderId = await _ordersRepository.InsertAndGetIdAsync(order);

				// Xử lý đơn hàng thông qua queue service
				foreach (var orderDetail in orderDetailsList)
				{
					orderDetail.OrderId = orderId;
					await _orderDetailsRepository.InsertAsync(orderDetail);
				}

				await _orderQueueService.ProcessOrderAsync(input);
				// xóa hết cart và cartItem của user
				var getCart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == AbpSession.UserId);
				await _cartRepository.DeleteAsync(getCart);
				await _cartItemRepository.DeleteAsync(x => x.CartId == getCart.Id);
				await CurrentUnitOfWork.SaveChangesAsync();
				return orderId;
			}
			catch (Exception)
			{
				// Đảm bảo release locks nếu có lỗi xảy ra
				await _orderQueueService.ReleaseProductLocksAsync(input);
				throw;
			}
		}
	}
}
