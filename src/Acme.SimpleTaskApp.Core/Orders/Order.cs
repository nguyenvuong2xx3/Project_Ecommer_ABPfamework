using Abp.Domain.Entities.Auditing;
using Acme.SimpleTaskApp.OrderItems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders
{
	public class Order : FullAuditedEntity<int>
	{
		public long? UserId { get; set; }
		public int? PaymentMethod { get; set; }
		public string? FullName { get; set; }
		public int? GioiTinh { get; set; }	
		public int? Status { get; set; } // 0: Pending, 1: Completed, 2: Cancelled
		public string? TinhThanh { get; set; }
		public string? PhuongXa { get; set; }
		public string? DiaChiChiTiet { get; set; }
		public decimal? TotalPrice { get; set; }
		//public List<OrderDetails> OrderDetails { get; set; }
	}
}
