using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Email;
using Acme.SimpleTaskApp.Notifications;
using Acme.SimpleTaskApp.OrderItems;
using Acme.SimpleTaskApp.Orders.Dtos;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Sales;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
		private readonly IRepository<Sale, int> _saleRepository;
		private readonly IRepository<Order, int> _ordersRepository;
		private readonly IRepository<User, long> _userRepository;
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<ProductVariant> _productVariantRepository;
		private readonly IRepository<ProductImage> _productImageRepository;
		private readonly INotificationAppService _notificationAppService;
		private readonly IOrderQueueService _orderQueueService;
		private readonly IRepository<Cart, int> _cartRepository;
		private readonly IRepository<CartItem, int> _cartItemRepository;
		private readonly ISendMailAppService _sendMailAppService;
		private readonly ISaleAppService _saleAppService;

		public OrdersAppService(
			IRepository<Sale, int> saleRepository,
			IRepository<ProductImage> productImageRepository,
			IRepository<Order, int> orderRepository,
			IRepository<Product> productRepository,
			IRepository<ProductVariant> productVariantRepository,
			IRepository<User, long> userRepository,
			INotificationAppService notificationAppService,
			IRepository<CartItem, int> cartItemRepository,
		IRepository<Cart, int> cartRepository,
		ISendMailAppService sendMailAppService,
		IOrderQueueService orderQueueService,
		ISaleAppService saleAppService)
		{
			_saleRepository = saleRepository;
			_sendMailAppService = sendMailAppService;
			_productImageRepository = productImageRepository;
			_cartRepository = cartRepository;
			_cartItemRepository = cartItemRepository;
			_ordersRepository = orderRepository;
			_userRepository = userRepository;
			_ordersRepository = orderRepository;
			_productRepository = productRepository;
			_productVariantRepository = productVariantRepository;
			_notificationAppService = notificationAppService;
			_orderQueueService = orderQueueService;
			_saleAppService = saleAppService;
		}

		public async Task<int> CreateOrder(CreateOrderInput input)
		{
			decimal totalPrice = 0;
			decimal finalPrice = 0;
			var orderDetailsList = new List<OrderDetails>();
			var currentUser = _userRepository.Get(AbpSession.UserId.Value);
			var user = await _userRepository.GetAsync(currentUser.Id);

			if (string.IsNullOrEmpty(user.TinhThanh) && string.IsNullOrEmpty(user.PhuongXa) &&
				string.IsNullOrEmpty(input.Order.TinhThanh) && string.IsNullOrEmpty(input.Order.PhuongXa))
			{
				throw new UserFriendlyException("Vui lòng cung cấp địa chỉ giao hàng (Tỉnh/Thành và Phường/Xã) trước khi đặt hàng.");
			}

			// Cố gắng khóa tất cả sản phẩm trong đơn hàng và validate stock
			if (!await _orderQueueService.TryLockProductsAsync(input))
			{
				throw new UserFriendlyException("Một số sản phẩm trong đơn hàng không còn đủ số lượng. Vui lòng thử lại.");
			}

			try
			{
				// Tính tổng giá và chuẩn bị cart items để validate voucher
				var cartItemsForDiscount = new List<Sales.CartItemDiscountDto>();

				foreach (var item in input.OrderDetails)
				{
					totalPrice += item.Quantity.Value * item.NewPrice.Value;

					// Lấy thông tin product và category để validate voucher
					var variant = await _productVariantRepository.GetAsync(item.ProductVariantId.Value);
					var product = await _productRepository.GetAsync(variant.ProductId);

					cartItemsForDiscount.Add(new Sales.CartItemDiscountDto
					{
						ProductVariantId = item.ProductVariantId.Value,
						ProductId = variant.ProductId,
						CategoryId = product.CategoryId,
						Price = item.NewPrice.Value,
						Quantity = item.Quantity.Value
					});

					orderDetailsList.Add(new OrderDetails
					{
						ProductVariantId = item.ProductVariantId,
						Quantity = item.Quantity,
						NewPrice = item.NewPrice
					});
				}

				finalPrice = totalPrice;

				// Validate và apply voucher nếu có
				if (!string.IsNullOrWhiteSpace(input.VoucherCode))
				{
					var request = new RequestDisCountVoucherCart
					{
						CartItems = cartItemsForDiscount,
						VoucherCode = input.VoucherCode.Trim().ToUpper()
					};

					var discountResult = await _saleAppService.CalculateCartDiscount(request);

					if (!discountResult.Success)
					{
						throw new UserFriendlyException($"Mã voucher không hợp lệ: {discountResult.Message}");
					}
					// cập nhật lại số lượng voucher
					//var sale = await _saleRepository.FirstOrDefaultAsync(x => x.VoucherCode == input.VoucherCode);
					//if (sale != null)
					//{
					//	//sale.UsageLimit = (sale.UsageLimit ?? 0) - 1;
					//	sale.UsedCount = sale.UsedCount + 1;
					//}
					//await _saleRepository.UpdateAsync(sale);

					// Cập nhật final price sau khi áp dụng voucher
					finalPrice = discountResult.FinalAmount;

					// Increment voucher used count
					if (discountResult.AppliedVoucher != null)
					{
						await _saleAppService.IncrementUsedCount(discountResult.AppliedVoucher.Id);
					}
				}

				// Tạo Order với giá cuối cùng (đã trừ voucher nếu có)
				Order order = new Order
				{
					Code = currentUser.Id + DateTime.Now.ToString("yyyyMMdd:HHmm"),
					PaymentMethod = input.Order.PaymentMethod,
					UserId = input.Order.UserId ?? user.Id,
					Status = 0,
					TotalPrice = finalPrice, // Sử dụng finalPrice thay vì totalPrice
					FullName = string.IsNullOrWhiteSpace(input.Order.FullName) ? user.Name : input.Order.FullName,
					OrderDetails = orderDetailsList,
					GioiTinh = input.Order.GioiTinh != null && input.Order.GioiTinh >= 0 ? input.Order.GioiTinh : user.GioiTinh,
					TinhThanh = string.IsNullOrWhiteSpace(input.Order.TinhThanh) ? user.TinhThanh : input.Order.TinhThanh,
					PhuongXa = string.IsNullOrWhiteSpace(input.Order.PhuongXa) ? user.PhuongXa : input.Order.PhuongXa,
					DiaChiChiTiet = string.IsNullOrWhiteSpace(input.Order.DiaChiChiTiet) ? user.DiaChiChiTiet : input.Order.DiaChiChiTiet,
					PhoneNumber = string.IsNullOrWhiteSpace(input.Order.PhoneNumber) ? user.PhoneNumber : input.Order.PhoneNumber,
				};

				order.Serialize();
				var orderId = await _ordersRepository.InsertAndGetIdAsync(order);

				// Xử lý trừ stock và release locks
				await _orderQueueService.ProcessOrderAsync(input);

				// Xóa cart và cartItem của user
				var getCart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == AbpSession.UserId);
				if (getCart != null)
				{
					await _cartItemRepository.DeleteAsync(x => x.CartId == getCart.Id);
					await _cartRepository.DeleteAsync(getCart);
				}

				await CurrentUnitOfWork.SaveChangesAsync();

				// Gửi email xác nhận đơn hàng
				_sendMailAppService.SendMailOrderAsync();

				return orderId;
			}
			catch (Exception)
			{
				// Đảm bảo release locks nếu có lỗi xảy ra
				await _orderQueueService.ReleaseProductLocksAsync(input);
				throw;
			}
		}

		[HttpPost]
		public async Task<List<Order>> GetOrderByCurrentUser(GetAllOrderInput input)
		{
			var currentUserId = AbpSession.UserId;

			var query = _ordersRepository.GetAll()
					.Where(x => x.UserId == currentUserId)
					.WhereIf(input.StatusUser != null && input.StatusUser.Any(), x =>  input.StatusUser.Contains(x.Status.Value))
					.WhereIf(input.PaymentMethod.HasValue, x => x.PaymentMethod == input.PaymentMethod)
					.WhereIf(input.StartTime.HasValue && input.EndTime.HasValue, x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime);

			query = query.OrderBy(input.Sorting).PageBy(input);

			var orders = await query.ToListAsync();

			foreach (var order in orders)
			{
				order.Deserialize();
			}

			// Lấy ID tất cả ProductVariant từ TẤT CẢ các đơn hàng
			var allVariantIds = orders
					.Where(o => o.OrderDetails != null) // Đảm bảo OrderDetails không null
					.SelectMany(o => o.OrderDetails)
					.Select(d => d.ProductVariantId)
					.Distinct()
					.ToList();

			if (!allVariantIds.Any())
			{
				return orders; // Không có chi tiết nào, trả về luôn
			}

			var variantsMap = await _productVariantRepository.GetAll()
					.Where(pv => allVariantIds.Contains(pv.Id))
					.ToDictionaryAsync(pv => pv.Id);

			var allProductIds = variantsMap.Values
					.Select(pv => pv.ProductId)
					.Distinct()
					.ToList();

			var productsMap = await _productRepository.GetAll()
					.Where(p => allProductIds.Contains(p.Id))
					.ToDictionaryAsync(p => p.Id);

			// Gán ProductName vào các variant (trong bộ nhớ)
			foreach (var variant in variantsMap.Values)
			{
				if (productsMap.TryGetValue(variant.ProductId, out var product))
				{
					variant.ProductName = product.Name;
				}
			}

			foreach (var order in orders)
			{
				if (order.OrderDetails != null)
				{
					foreach (var detail in order.OrderDetails)
					{
						// Lấy ProductVariant đã có sẵn từ Dictionary
						if (variantsMap.TryGetValue(detail.ProductVariantId.Value, out var variant))
						{
							detail.ProductVariant = variant;
						}
					}
				}
			}
			return orders;
		}

		[HttpPost]
		public async Task<PagedResultDto<Order>> GetAllOrder(GetAllOrderInput input)
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

			if (input.PaymentMethod.HasValue)
			{
				query = query.Where(order => order.PaymentMethod == input.PaymentMethod.Value);
			}

			if (input.Status.HasValue)
			{
				query = query.Where(order => order.Status == input.Status.Value);
			}

			var count = await query.CountAsync();

			var orders = await query
					.OrderByDescending(o => o.CreationTime)
					.PageBy(input)
					.ToListAsync();
			foreach (var order in orders)
			{
				order.Deserialize();
			}
			// Lấy UserName cho từng order
			var userIds = orders.Where(o => o.UserId.HasValue).Select(o => o.UserId.Value).Distinct().ToList();
			var users = await _userRepository.GetAll()
		.Where(u => userIds.Contains(u.Id))
		.ToDictionaryAsync(u => u.Id, u => u);

			foreach (var order in orders)
			{
				if (order.UserId.HasValue && users.TryGetValue(order.UserId.Value, out var user))
				{
					order.User = new User
					{
						Id = user.Id,
						UserName = user.UserName,
						Name = user.Name
					};
				}
			}

			return new PagedResultDto<Order>(count, orders);
		}

		public async Task<Order> GetOrder(int orderId)
		{
			var order = await _ordersRepository.FirstOrDefaultAsync(o => o.Id == orderId);
			// người đặt
			var user = await _userRepository.GetAsync(order.UserId.Value);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Deserialize();
			order.User = user;
			if (order.OrderDetails != null && order.OrderDetails.Any())
			{
				// Get all variant IDs in one go
				var variantIds = order.OrderDetails.Select(x => x.ProductVariantId).Distinct().ToList();

				// Load all variants in one query
				var variants = await _productVariantRepository.GetAll()
						.Where(x => variantIds.Contains(x.Id))
						.ToDictionaryAsync(x => x.Id);

				// Get all product IDs from variants
				var productIds = variants.Values.Select(v => v.ProductId).Distinct().ToList();

				// Load all products in one query
				var products = await _productRepository.GetAll()
						.Where(p => productIds.Contains(p.Id))
						.ToDictionaryAsync(p => p.Id);

				// Load all images in one query
				var images = await _productImageRepository.GetAll()
						.Where(x => variantIds.Contains(x.ProductVariantId.Value))
						.GroupBy(x => x.ProductVariantId.Value)
						.ToDictionaryAsync(g => g.Key, g => g.FirstOrDefault()?.ImageUrl);

				// Assign variants and images to order details
				foreach (var item in order.OrderDetails)
				{
					if (variants.TryGetValue(item.ProductVariantId.Value, out var variant))
					{
						item.ProductVariant = variant;

						if (images.TryGetValue(item.ProductVariantId.Value, out var imageUrl))
						{
							item.ProductVariant.ImageUrl = imageUrl;
						}
						if (products.TryGetValue(variant.ProductId, out var product))
						{
							item.ProductVariant.ProductName = $"{product.Name} - {variant.Ram} - {variant.Storage} - {variant.Color}";
						}
					}
				}
			}
			return order;
		}


		// đang xử lý - admin
		public async Task XuLyOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Status = 1;
			await _ordersRepository.UpdateAsync(order);
		}

		// hủy đơn - admin
		public async Task HuyAdminOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Deserialize();
			order.Status = 4;
			await _ordersRepository.UpdateAsync(order);

			// hoàn lại số lượng voucher đã sử dụng

			// hoàn lại số lượng sản phẩm
			if (order.OrderDetails != null && order.OrderDetails.Any())
			{
				foreach (var detail in order.OrderDetails)
				{
					var variant = await _productVariantRepository.GetAsync(detail.ProductVariantId.Value);
					if (variant != null && detail.Quantity.HasValue)
					{
						variant.StockQuantity += detail.Quantity.Value;
						await _productVariantRepository.UpdateAsync(variant);
					}
				}
			}
		}
		// hủy đơn - user, 
		public async Task HuyUserOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Deserialize();
			order.Status = 5;
			await _ordersRepository.UpdateAsync(order);

			if (order.OrderDetails != null && order.OrderDetails.Any())
			{
				foreach (var detail in order.OrderDetails)
				{
					var variant = await _productVariantRepository.GetAsync(detail.ProductVariantId.Value);
					if (variant != null && detail.Quantity.HasValue)
					{
						variant.StockQuantity += detail.Quantity.Value;
						await _productVariantRepository.UpdateAsync(variant);
					}
				}
			}
		}

		/// đang giao - admin
		public async Task DangGiaoOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Status = 2;
			await _ordersRepository.UpdateAsync(order);
		}
		/// thành công - admin
		public async Task ThanhCongOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Status = 3;
			await _ordersRepository.UpdateAsync(order);
		}

		// hoàn hàng - user
		public async Task HoanHangOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Status = 2;
			await _ordersRepository.UpdateAsync(order);
		}
	}
}
