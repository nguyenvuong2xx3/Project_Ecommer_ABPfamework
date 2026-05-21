using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization;
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
using static Acme.SimpleTaskApp.Orders.OrderNotificationJob;

namespace Acme.SimpleTaskApp.Orders
{
	public class OrdersAppService : ApplicationService, IOrdersAppService
	{
		private readonly IBackgroundJobManager _backgroundJobManager;
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
			IBackgroundJobManager backgroundJobManager,
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
			_backgroundJobManager = backgroundJobManager;
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

		[AbpAuthorize(PermissionNames.Pages_Orders_Create)]
		public async Task<int> CreateOrder(CreateOrderInput input)
		{
			decimal totalPrice = 0;
			decimal finalPrice = 0;
			var orderDetailsList = new List<OrderDetails>();
			var user = await _userRepository.GetAsync(AbpSession.UserId.Value);

			if (string.IsNullOrEmpty(user.TinhThanh) && string.IsNullOrEmpty(user.PhuongXa) &&
				string.IsNullOrEmpty(input.Order.TinhThanh) && string.IsNullOrEmpty(input.Order.PhuongXa))
			{
				throw new UserFriendlyException("Vui lòng cung cấp địa chỉ giao hàng (Tỉnh/Thành và Phường/Xã) trước khi đặt hàng.");
			}

			// Khoas tất cả sản phẩm trong đơn hàng và validate stock
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

					if (!discountResult.IsSuccess)
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

					//tăng số lần đã sử dụng voucher
					if (discountResult.AppliedVoucher != null)
					{
						await _saleAppService.IncrementUsedCount(discountResult.AppliedVoucher.Id);
					}
				}

				// Tao Order voi gia cuoi cung (da tru voucher neu co)
				// Neu la VNPay (PaymentMethod = 2): Status = -1 (Cho thanh toan), KHONG tru stock
				// Neu la COD/Bank (PaymentMethod = 0, 1): Status = 0 (Cho xac nhan), tru stock
				var isVNPayPayment = input.Order.PaymentMethod == 2;
				
				Order order = new Order
				{
					Code = user.Id + DateTime.Now.ToString("yyyyMMdd:HHmm"),
					PaymentMethod = input.Order.PaymentMethod,
					UserId = input.Order.UserId ?? user.Id,
					// Neu VNPay: Status = -1 (cho thanh toan), chua tru stock
					// Neu COD/Bank: Status = 0 (cho xac nhan), da tru stock
					Status = isVNPayPayment ? OrderStatus.ChoThanhToan : OrderStatus.ChoXacNhan,
					TotalPrice = finalPrice,
					FullName = string.IsNullOrWhiteSpace(input.Order.FullName) ? user.Name : input.Order.FullName,
					OrderDetails = orderDetailsList,
					GioiTinh = input.Order.GioiTinh != null && input.Order.GioiTinh >= 0 ? input.Order.GioiTinh : user.GioiTinh,
					TinhThanh = string.IsNullOrWhiteSpace(input.Order.TinhThanh) ? user.TinhThanh : input.Order.TinhThanh,
					PhuongXa = string.IsNullOrWhiteSpace(input.Order.PhuongXa) ? user.PhuongXa : input.Order.PhuongXa,
					DiaChiChiTiet = string.IsNullOrWhiteSpace(input.Order.DiaChiChiTiet) ? user.DiaChiChiTiet : input.Order.DiaChiChiTiet,
					PhoneNumber = string.IsNullOrWhiteSpace(input.Order.PhoneNumber) ? user.PhoneNumber : input.Order.PhoneNumber,
                    EmailAddress = string.IsNullOrWhiteSpace(input.Order.EmailAddress) ? user.EmailAddress : input.Order.EmailAddress,
				};

				order.Serialize();
				var orderId = await _ordersRepository.InsertAndGetIdAsync(order);

				// Xu ly stock theo phuong thuc thanh toan
				if (!isVNPayPayment)
				{
					// COD/Bank Transfer: Tru stock ngay lap tuc
					await _orderQueueService.ProcessOrderAsync(input);
				}
				else
				{
					// VNPay: Reserve stock (giu cho) thay vi tru
					// Stock se duoc tru that su khi thanh toan thanh cong
					// Neu thanh toan that bai, reserved se duoc hoan lai
					foreach (var item in input.OrderDetails)
					{
						var variant = await _productVariantRepository.GetAsync(item.ProductVariantId.Value);
						if (variant != null && item.Quantity.HasValue)
						{
							// Kiem tra stock kha dung (StockQuantity - ReservedQuantity)
							var availableStock = variant.StockQuantity - variant.ReservedQuantity;
							if (availableStock < item.Quantity.Value)
							{
								// Khong du stock kha dung
								throw new UserFriendlyException($"San pham {variant.ProductName ?? variant.Id.ToString()} khong du so luong. Con lai: {availableStock}");
							}
							
							// Tang reserved quantity (giu cho)
							variant.ReservedQuantity += item.Quantity.Value;
							await _productVariantRepository.UpdateAsync(variant);
							Logger.Info($"CreateOrder VNPay: Reserved {item.Quantity.Value} cua variant {variant.Id}. Reserved hien tai: {variant.ReservedQuantity}");
						}
					}
					
					// Release locks sau khi da reserve
					await _orderQueueService.ReleaseProductLocksAsync(input);
				}

				// Xoa cart va cartItem cua user CHI KHI khong phai VNPay
				// VNPay (PaymentMethod = 2) se giu cart cho den khi payment thanh cong
				// Neu payment that bai, user co the thu lai
				if (!isVNPayPayment)
				{
					var getCart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == AbpSession.UserId);
					if (getCart != null)
					{
						await _cartItemRepository.DeleteAsync(x => x.CartId == getCart.Id);
						await _cartRepository.DeleteAsync(getCart);
					}
				}
				// Note: Với VNPay, cart sẽ được xóa trong IPN callback khi thanh toán thành công

				await CurrentUnitOfWork.SaveChangesAsync();

				// Chi gui email va thong bao neu KHONG phai VNPay
				// VNPay se gui sau khi thanh toan thanh cong
				if (!isVNPayPayment)
				{
					// Gui email xac nhan don hang
					await _sendMailAppService.SendMailOrderAsync();
					// Thong bao don hang moi
					await _backgroundJobManager.EnqueueAsync<OrderNotificationJob, OrderNotificationJobArgs>(
							new OrderNotificationJobArgs { Code = order.Code }
					);
				}
				// Voi VNPay: Email va thong bao se duoc gui trong ConfirmVNPayPayment
				
				return orderId;
			}
			catch (Exception)
			{
				// Đảm bảo release locks nếu có lỗi xảy ra
				await _orderQueueService.ReleaseProductLocksAsync(input);
				throw;
			}
		}

		[AbpAuthorize(PermissionNames.Pages_Orders_View)]
		[HttpPost]
		public async Task<List<Order>> GetOrderByCurrentUser(GetAllOrderInput input)
		{
			var currentUserId = AbpSession.UserId;

			var query = _ordersRepository.GetAll()
					.Where(x => x.UserId == currentUserId)
					.WhereIf(input.StatusUser != null && input.StatusUser.Any(), x => input.StatusUser.Contains(x.Status.Value))
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

		[AbpAuthorize(PermissionNames.Pages_Orders_View)]
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

		[AbpAuthorize(PermissionNames.Pages_Orders_View)]
		public async Task<Order> GetOrder(int orderId)
		{
			var order = await _ordersRepository.FirstOrDefaultAsync(o => o.Id == orderId);
			// người đặt
			var user = await _userRepository.GetAsync(order.UserId.Value);
			if (order == null)
			{
				throw new UserFriendlyException("Không tìm thấy đơn hàng.");
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
		[AbpAuthorize(PermissionNames.Pages_Orders_Confirm)]
		public async Task XuLyOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Không tìm thấy đơn hàng.");
			}
			order.Status = 1;
			await _ordersRepository.UpdateAsync(order);

			// Sử dụng _backgroundJobManager instance thay vì static class
			await _backgroundJobManager.EnqueueAsync<OrderStatusNotificationJob, OrderStatusNotificationJob.OrderStatusNotificationJobArgs>(
					new OrderStatusNotificationJob.OrderStatusNotificationJobArgs
					{
						TenantId = 1,
						UserId = order.UserId.Value,
						OrderId = order.Id,
						OrderCode = order.Code,
						NotificationName = "Đơn hàng đã được duyệt",
						Title = "Đơn hàng đã được duyệt",
						Message = $"Đơn hàng #{order.Code} của bạn đã được xử lý và đang chuẩn bị.",
						NotificationType = "OrderProcessed",
						Status = order.Status.Value,
						Severity = NotificationSeverity.Info
					});
		}

		// hủy đơn - admin
		[AbpAuthorize(PermissionNames.Pages_Orders_Cancel)]
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

			await _backgroundJobManager.EnqueueAsync<OrderStatusNotificationJob, OrderStatusNotificationJob.OrderStatusNotificationJobArgs>(
			 new OrderStatusNotificationJob.OrderStatusNotificationJobArgs
			 {
				 TenantId = 1,
				 UserId = order.UserId.Value,
				 OrderId = order.Id,
				 OrderCode = order.Code,
				 NotificationName = "Đơn hàng đã bị hủy",
				 Title = "Đơn hàng đã bị hủy",
				 Message = $"Đơn hàng #{order.Code} của bạn đã bị hủy bởi quản trị viên.",
				 NotificationType = "OrderCancelledByAdmin",
				 Status = order.Status.Value,
				 Severity = NotificationSeverity.Warn
			 });
		}
		// Hủy đơn - user
		// Cho phép hủy đơn khi:
		// - Status = 0 (Chờ xác nhận) → Hoàn lại StockQuantity
		// - Status = -1 (Chờ thanh toán VNPay) → Hoàn lại ReservedQuantity
		[AbpAuthorize(PermissionNames.Pages_Orders_Cancel)]
		public async Task HuyUserOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Không tìm thấy đơn hàng.");
			}

			// Chỉ cho phép hủy khi status = 0 (Chờ xác nhận) hoặc -1 (Chờ thanh toán VNPay)
			if (order.Status != OrderStatus.ChoXacNhan && order.Status != OrderStatus.ChoThanhToan)
			{
				throw new UserFriendlyException("Không thể hủy đơn hàng ở trạng thái này.");
			}

			order.Deserialize();
			
			var wasWaitingForPayment = order.Status == OrderStatus.ChoThanhToan;
			order.Status = OrderStatus.HuyBoiUser; // Status = 5
			await _ordersRepository.UpdateAsync(order);

			// Hoàn lại stock cho từng sản phẩm
			if (order.OrderDetails != null && order.OrderDetails.Any())
			{
				foreach (var detail in order.OrderDetails)
				{
					var variant = await _productVariantRepository.GetAsync(detail.ProductVariantId.Value);
					if (variant != null && detail.Quantity.HasValue)
					{
						if (wasWaitingForPayment)
						{
							// Đơn VNPay chờ thanh toán: Hoàn lại ReservedQuantity (chưa trừ StockQuantity)
							variant.ReservedQuantity -= detail.Quantity.Value;
							if (variant.ReservedQuantity < 0) variant.ReservedQuantity = 0;
							Logger.Info($"HuyUserOrder: Đơn VNPay #{orderId} - Hoàn reserved {detail.Quantity.Value} cho variant {variant.Id}");
						}
						else
						{
							// Đơn COD/Bank chờ xác nhận: Hoàn lại StockQuantity (đã trừ khi tạo đơn)
							variant.StockQuantity += detail.Quantity.Value;
							Logger.Info($"HuyUserOrder: Đơn #{orderId} - Hoàn stock {detail.Quantity.Value} cho variant {variant.Id}");
						}
						await _productVariantRepository.UpdateAsync(variant);
					}
				}
			}

			await CurrentUnitOfWork.SaveChangesAsync();
			Logger.Info($"HuyUserOrder: Đã hủy đơn hàng #{orderId} bởi user.");
		}

		/// đang giao - admin
		[AbpAuthorize(PermissionNames.Pages_Orders_Ship)]
		public async Task DangGiaoOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Status = 2;
			await _ordersRepository.UpdateAsync(order);

			await _backgroundJobManager.EnqueueAsync<OrderStatusNotificationJob, OrderStatusNotificationJob.OrderStatusNotificationJobArgs>(
				new OrderStatusNotificationJob.OrderStatusNotificationJobArgs
				{
					TenantId = 1,
					UserId = order.UserId.Value,
					OrderId = order.Id,
					OrderCode = order.Code,
					NotificationName = "Đơn hàng đang giao",
					Title = "Đơn hàng đang giao",
					Message = $"Đơn hàng #{order.Code} của bạn đang được vận chuyển.",
					NotificationType = "OrderShipping",
					Status = order.Status.Value,
					Severity = NotificationSeverity.Info
				});
		}

		/// thành công - admin
		[AbpAuthorize(PermissionNames.Pages_Orders_Complete)]
		public async Task ThanhCongOrder(int orderId)
		{
			var order = await _ordersRepository.GetAsync(orderId);
			if (order == null)
			{
				throw new UserFriendlyException("Order not found.");
			}
			order.Status = 3;
			await _ordersRepository.UpdateAsync(order);
			// Gửi thông báo cho user
			await _backgroundJobManager.EnqueueAsync<OrderStatusNotificationJob, OrderStatusNotificationJob.OrderStatusNotificationJobArgs>(
					new OrderStatusNotificationJob.OrderStatusNotificationJobArgs
					{
						TenantId = 1,
						UserId = order.UserId.Value,
						OrderId = order.Id,
						OrderCode = order.Code,
						NotificationName = "Đơn hàng hoàn thành",
						Title = "Đơn hàng hoàn thành",
						Message = $"Đơn hàng #{order.Code} của bạn đã được giao thành công. Cảm ơn bạn!",
						NotificationType = "OrderCompleted",
						Status = order.Status.Value,
						Severity = NotificationSeverity.Success
					});
		}

		// hoan hang - user
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

		// VNPAY PAYMENT METHODS
		
		/// <summary>
		/// Xac nhan thanh toan VNPay thanh cong
		/// - Cap nhat trang thai don hang tu -1 (Cho thanh toan) sang 0 (Cho xac nhan)
		/// - Tru stock san pham
		/// - Xoa gio hang
		/// - Gui email va thong bao
		/// </summary>
		public async Task<bool> ConfirmVNPayPayment(int orderId, string transactionId)
		{
			var order = await _ordersRepository.FirstOrDefaultAsync(o => o.Id == orderId);
			if (order == null)
			{
				Logger.Warn($"ConfirmVNPayPayment: Khong tim thay don hang {orderId}");
				return false;
			}

			// Kiem tra trang thai don hang phai la "Cho thanh toan" (-1)
			if (order.Status != OrderStatus.ChoThanhToan)
			{
				Logger.Warn($"ConfirmVNPayPayment: Don hang {orderId} khong o trang thai cho thanh toan. Status hien tai: {order.Status}");
				return false;
			}

			order.Deserialize();

			// Tru stock cho tung san pham trong don hang
			if (order.OrderDetails != null && order.OrderDetails.Any())
			{
				foreach (var detail in order.OrderDetails)
				{
					var variant = await _productVariantRepository.FirstOrDefaultAsync(v => v.Id == detail.ProductVariantId.Value);
					if (variant != null && detail.Quantity.HasValue)
					{
						// Kiem tra con du stock khong
						if (variant.StockQuantity < detail.Quantity.Value)
						{
							Logger.Error($"ConfirmVNPayPayment: San pham {variant.Id} khong du stock. Can: {detail.Quantity.Value}, Con: {variant.StockQuantity}");
							// Van tiep tuc xu ly, nhung log lai
						}
						
						// Tru stock
						variant.StockQuantity -= detail.Quantity.Value;
						await _productVariantRepository.UpdateAsync(variant);
						Logger.Info($"ConfirmVNPayPayment: Da tru {detail.Quantity.Value} stock cua variant {variant.Id}");
					}
				}
			}

			// Cap nhat trang thai don hang sang "Cho xac nhan"
			order.Status = OrderStatus.ChoXacNhan;
			await _ordersRepository.UpdateAsync(order);

			// Xoa gio hang cua user
			if (order.UserId.HasValue)
			{
				var cart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == order.UserId.Value);
				if (cart != null)
				{
					await _cartItemRepository.DeleteAsync(x => x.CartId == cart.Id);
					await _cartRepository.DeleteAsync(cart);
					Logger.Info($"ConfirmVNPayPayment: Da xoa gio hang cua user {order.UserId.Value}");
				}
			}

			await CurrentUnitOfWork.SaveChangesAsync();

			// Gui email xac nhan don hang
			try
			{
				await _sendMailAppService.SendMailOrderAsync();
			}
			catch (Exception ex)
			{
				Logger.Error($"ConfirmVNPayPayment: Loi gui email cho don hang {orderId}", ex);
			}

			// Gui thong bao don hang moi
			try
			{
				await _backgroundJobManager.EnqueueAsync<OrderNotificationJob, OrderNotificationJobArgs>(
					new OrderNotificationJobArgs { Code = order.Code }
				);
			}
			catch (Exception ex)
			{
				Logger.Error($"ConfirmVNPayPayment: Loi gui thong bao cho don hang {orderId}", ex);
			}

			Logger.Info($"ConfirmVNPayPayment: Xac nhan thanh toan thanh cong cho don hang {orderId}, TransactionId: {transactionId}");
			return true;
		}

		/// <summary>
		/// Huy don hang khi thanh toan VNPay that bai
		/// - Cap nhat trang thai don hang sang 6 (Thanh toan that bai)
		/// - Hoan lai ReservedQuantity
		/// </summary>
		public async Task<bool> CancelVNPayPayment(int orderId, string reason)
		{
			var order = await _ordersRepository.FirstOrDefaultAsync(o => o.Id == orderId);
			if (order == null)
			{
				Logger.Warn($"CancelVNPayPayment: Khong tim thay don hang {orderId}");
				return false;
			}

			// Kiem tra trang thai don hang phai la "Cho thanh toan" (-1)
			if (order.Status != OrderStatus.ChoThanhToan)
			{
				Logger.Warn($"CancelVNPayPayment: Don hang {orderId} khong o trang thai cho thanh toan. Status hien tai: {order.Status}");
				return false;
			}

			order.Deserialize();

			// Hoan lai ReservedQuantity cho tung san pham
			// Stock that su (StockQuantity) khong bi anh huong vi chua tru
			if (order.OrderDetails != null && order.OrderDetails.Any())
			{
				foreach (var detail in order.OrderDetails)
				{
					var variant = await _productVariantRepository.FirstOrDefaultAsync(v => v.Id == detail.ProductVariantId.Value);
					if (variant != null && detail.Quantity.HasValue)
					{
						// Giai phong reserved
						variant.ReservedQuantity -= detail.Quantity.Value;
						if (variant.ReservedQuantity < 0) variant.ReservedQuantity = 0;
						
						await _productVariantRepository.UpdateAsync(variant);
						Logger.Info($"CancelVNPayPayment: Variant {variant.Id} - Hoan reserved {detail.Quantity.Value}, Reserved con: {variant.ReservedQuantity}");
					}
				}
			}

			// Cap nhat trang thai don hang sang "Thanh toan that bai"
			order.Status = OrderStatus.ThanhToanThatBai;
			await _ordersRepository.UpdateAsync(order);
			await CurrentUnitOfWork.SaveChangesAsync();

			Logger.Info($"CancelVNPayPayment: Da huy don hang {orderId} do thanh toan that bai. Ly do: {reason}");
			return true;
		}
	}
}
