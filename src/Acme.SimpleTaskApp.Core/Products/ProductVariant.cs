using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Products
{
	[Table("AppProductVariants")]
	public class ProductVariant : FullAuditedEntity<int>
	{
		public int ProductId { get; set; }
		public string Ram { get; set; }            // 4GB, 8GB
		public string Storage { get; set; }        // 64GB, 128GB
		public string Color { get; set; }          // Đen, Trắng, Xanh
		public decimal Price { get; set; }         // Giá riêng cho biến thể
		public int StockQuantity { get; set; }     // Tồn kho riêng
		public string SKU { get; set; }            // Mã riêng cho biến thể tự sinh ở BE
	}
}
