using Abp.Domain.Entities.Auditing;
using Acme.SimpleTaskApp.Orders;
using Acme.SimpleTaskApp.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.OrderItems
{
	public class OrderDetails : FullAuditedEntity<int>
	{
		public int OrderId { get; set; }

		[ForeignKey("OrderId")]
		public Order Order { get; set; }
		
		// Sử dụng ProductVariantId thay vì ProductId để tracking chính xác variant nào được đặt
		public int ProductVariantId { get; set; }

		[ForeignKey("ProductVariantId")]
		public ProductVariant ProductVariant { get; set; }
		
		public int Quantity { get; set; }
		public decimal NewPrice { get; set; } // Price at the time of order
	}
}
