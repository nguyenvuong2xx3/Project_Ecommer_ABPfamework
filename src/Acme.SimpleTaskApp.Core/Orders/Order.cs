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
	public class Order : FullAuditedEntity<int>
	{
		public string Code { get; set; }
		public long? UserId { get; set; }
		[NotMapped] public User User {get; set;} // response
		public int? PaymentMethod { get; set; }
		public string? FullName { get; set; }
		public int? GioiTinh { get; set; }
		public int? Status { get; set; } // 0: chờ xác nhận - mặc định; 1: đang xử lý; 2: đang giao; 3: thành công, 4: đã hủy của admin; 5: đã hủy của user
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
