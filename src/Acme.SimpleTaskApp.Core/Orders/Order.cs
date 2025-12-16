using Abp.Domain.Entities.Auditing;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.OrderItems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders
{
	// Định nghĩa các trạng thái đơn hàng
	public static class OrderStatus
	{
		public const int ChoThanhToan = -1;    // Đơn hàng VNPay đang chờ thanh toán (chưa trừ stock)
		public const int ChoXacNhan = 0;       // Chờ xác nhận (đã trừ stock)
		public const int DangXuLy = 1;         // Đang xử lý
		public const int DangGiao = 2;         // Đang giao
		public const int ThanhCong = 3;        // Thành công
		public const int HuyBoiAdmin = 4;      // Đã hủy bởi admin
		public const int HuyBoiUser = 5;       // Đã hủy bởi user
		public const int ThanhToanThatBai = 6; // Thanh toán VNPay thất bại
	}

	public class Order : FullAuditedEntity<int>
	{
		public string Code { get; set; }
		public long? UserId { get; set; }
		[NotMapped] public User User {get; set;} // Cho BE trả về
		public int? PaymentMethod { get; set; }
		public string? FullName { get; set; }
		public int? GioiTinh { get; set; }
		// Trạng thái đơn hàng:
		// -1: Chờ thanh toán (VNPay) - chưa trừ stock
		// 0: Chờ xác nhận - mặc định, đã trừ stock
		// 1: Đang xử lý
		// 2: Đang giao
		// 3: Thành công
		// 4: Đã hủy bởi admin
		// 5: Đã hủy bởi user
		// 6: Thanh toán thất bại (VNPay)
		public int? Status { get; set; }
		public string? PhoneNumber { get; set; } = string.Empty;
		public string? TinhThanh { get; set; }
		public string? PhuongXa { get; set; }
		public string? DiaChiChiTiet { get; set; }
		public decimal? TotalPrice { get; set; }
		[NotMapped] public List<OrderDetails>? OrderDetails { get; set; }
		public string? OrderDetailJson { get; set; }

		public void Serialize()
		{
			OrderDetailJson = OrderDetails != null && OrderDetails.Any()
					? JsonSerializer.Serialize(OrderDetails) : null;
		}
		public void Deserialize()
		{
			if (OrderDetailJson != null && OrderDetailJson.Length > 0)
				OrderDetails = JsonSerializer.Deserialize<List<OrderDetails>>(OrderDetailJson);
		}
	}
}
