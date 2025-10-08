using Abp.Domain.Entities.Auditing;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Acme.SimpleTaskApp.Products
{
	public class ProductVariant : FullAuditedEntity<int>
	{
		public int ProductId { get; set; } // Liên kết với sản phẩm
		public string Ram { get; set; }            // 4GB, 8GB
		public string Storage { get; set; }        // 64GB, 128GB
		public string Color { get; set; }          // Đen, Trắng, Xanh
		public decimal Price { get; set; }         // Giá riêng cho biến thể
		public int StockQuantity { get; set; }     // Tồn kho riêng
		public string SKU { get; set; }            // Mã riêng cho biến thể tự sinh ở BE
		[NotMapped] public List<IFormFile> ImageFiles { get; set; }

	}
}
