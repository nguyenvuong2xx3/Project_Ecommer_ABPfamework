using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Acme.SimpleTaskApp.Products;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders
{
	/// <summary>
	/// Background Job để dọn dẹp các đơn hàng VNPay quá hạn thanh toán.
	/// Khi đơn hàng VNPay ở trạng thái "Chờ thanh toán" (Status = -1) quá 15 phút,
	/// job này sẽ:
	/// - Cập nhật trạng thái sang "Thanh toán thất bại" (Status = 6)
	/// - Hoàn lại ReservedQuantity cho các sản phẩm
	/// </summary>
	public class ExpiredVNPayOrdersCleanupJob : BackgroundJob<ExpiredVNPayOrdersCleanupJobArgs>, ITransientDependency
	{
		private readonly IRepository<Order, int> _orderRepository;
		private readonly IRepository<ProductVariant, int> _productVariantRepository;

		// Thời gian hết hạn thanh toán VNPay (15 phút)
		private const int VNPAY_PAYMENT_TIMEOUT_MINUTES = 15;

		public ExpiredVNPayOrdersCleanupJob(
			IRepository<Order, int> orderRepository,
			IRepository<ProductVariant, int> productVariantRepository)
		{
			_orderRepository = orderRepository;
			_productVariantRepository = productVariantRepository;
		}

		[UnitOfWork]
		public override void Execute(ExpiredVNPayOrdersCleanupJobArgs args)
		{
			ExecuteAsync().GetAwaiter().GetResult();
		}

		private async Task ExecuteAsync()
		{
			Logger.Info("ExpiredVNPayOrdersCleanupJob: Bắt đầu kiểm tra đơn hàng VNPay quá hạn...");

			// Tính thời điểm hết hạn (15 phút trước)
			var expirationTime = DateTime.Now.AddMinutes(-VNPAY_PAYMENT_TIMEOUT_MINUTES);

			// Tìm các đơn hàng VNPay đang chờ thanh toán và đã quá 15 phút
			var expiredOrders = await _orderRepository.GetAll()
				.Where(o => o.Status == OrderStatus.ChoThanhToan  // Status = -1: Chờ thanh toán
							&& o.PaymentMethod == 2               // PaymentMethod = 2: VNPay
							&& o.CreationTime <= expirationTime)  // Đã tạo hơn 15 phút
				.ToListAsync();

			if (!expiredOrders.Any())
			{
				Logger.Info("ExpiredVNPayOrdersCleanupJob: Không có đơn hàng VNPay quá hạn.");
				return;
			}

			Logger.Info($"ExpiredVNPayOrdersCleanupJob: Tìm thấy {expiredOrders.Count} đơn hàng VNPay quá hạn.");

			foreach (var order in expiredOrders)
			{
				try
				{
					await ProcessExpiredOrder(order);
				}
				catch (Exception ex)
				{
					Logger.Error($"ExpiredVNPayOrdersCleanupJob: Lỗi xử lý đơn hàng {order.Id}", ex);
				}
			}

			Logger.Info("ExpiredVNPayOrdersCleanupJob: Hoàn thành xử lý đơn hàng VNPay quá hạn.");
		}

		private async Task ProcessExpiredOrder(Order order)
		{
			Logger.Info($"ExpiredVNPayOrdersCleanupJob: Xử lý đơn hàng quá hạn #{order.Id} (Code: {order.Code})");

			// Deserialize để lấy OrderDetails
			order.Deserialize();

			// Hoàn lại ReservedQuantity cho từng sản phẩm
			if (order.OrderDetails != null && order.OrderDetails.Any())
			{
				foreach (var detail in order.OrderDetails)
				{
					var variant = await _productVariantRepository.FirstOrDefaultAsync(v => v.Id == detail.ProductVariantId.Value);
					if (variant != null && detail.Quantity.HasValue)
					{
						// Giải phóng reserved
						variant.ReservedQuantity -= detail.Quantity.Value;
						if (variant.ReservedQuantity < 0) variant.ReservedQuantity = 0;

						await _productVariantRepository.UpdateAsync(variant);
						Logger.Info($"ExpiredVNPayOrdersCleanupJob: Đơn hàng #{order.Id} - Hoàn reserved {detail.Quantity.Value} cho variant {variant.Id}, Reserved còn: {variant.ReservedQuantity}");
					}
				}
			}

			// Cập nhật trạng thái đơn hàng sang "Thanh toán thất bại"
			order.Status = OrderStatus.ThanhToanThatBai;
			await _orderRepository.UpdateAsync(order);

			Logger.Info($"ExpiredVNPayOrdersCleanupJob: Đã hủy đơn hàng #{order.Id} do quá hạn thanh toán VNPay.");
		}
	}

	/// <summary>
	/// Arguments cho ExpiredVNPayOrdersCleanupJob
	/// </summary>
	[Serializable]
	public class ExpiredVNPayOrdersCleanupJobArgs
	{
		// Không cần tham số, job sẽ tự tìm các đơn hàng quá hạn
	}
}
