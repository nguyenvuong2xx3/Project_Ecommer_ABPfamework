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
		public int? Status { get; set; } // 0: Pending, 1: duyệt, 2, hoàn thành, 3: từ chối 
		public string? PhoneNumber { get; set; }
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
