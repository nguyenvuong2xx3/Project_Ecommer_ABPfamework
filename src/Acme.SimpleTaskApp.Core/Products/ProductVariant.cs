using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace Acme.SimpleTaskApp.Products
{
	[Table("AppProductVariants")]
	public class ProductVariant : FullAuditedEntity<int>
	{
		public int ProductId { get; set; }
		[NotMapped] public string ProductName { get; set; } // cho BE trả về
		public string Ram { get; set; }
		public string Storage { get; set; }
		public string Color { get; set; }
		public decimal Price { get; set; }
		public int StockQuantity { get; set; }

		// Số lượng đang được giữ chỗ bởi các đơn hàng VNPay chưa thanh toán
		// Khi user tạo đơn VNPay: ReservedQuantity += quantity
		// Khi thanh toán thành công: StockQuantity -= quantity, ReservedQuantity -= quantity
		// Khi thanh toán thất bại hoặc hết hạn: ReservedQuantity -= quantity
		// Stock khả dụng = StockQuantity - ReservedQuantity
		public int ReservedQuantity { get; set; } = 0;

		public string Size { get; set; }

		public string SacNhanh { get; set; }
		public string Connectivity { get; set; }

		public string SKU { get; set; }
		[NotMapped] public List<IFormFile> ImageFiles { get; set; }
		[NotMapped] public List<string> DeletedImageUrls { get; set; }
		[NotMapped] public string ImageUrl { get; set; } // cho BE trả về
		[NotMapped] public List<string> ImageUrls { get; set; } // cho BE trả về
		[NotMapped] public List<ProductImage> ProductImages { get; set; } // cho BE trả về

		[NotMapped] public int SoldQuantity { get; set; } = 0; // Số lượng đã bán, cho BE trả về

		// Tính stock khả dụng (đã trừ đi số lượng đang giữ chỗ)
		[NotMapped] public int AvailableStock => StockQuantity - ReservedQuantity;

		[NotMapped]
		public decimal DiscountPercentage { get; set; }
		[NotMapped]
		public decimal DiscountedPrice { get; set; }
		[NotMapped]
		public bool HasActiveDiscount { get; set; }
		[NotMapped]
		public string SaleName { get; set; }

		[NotMapped]
		public double AverageRating { get; set; } = 0; // Trung bình số sao (0-5)
		[NotMapped]
		public int TotalRatings { get; set; } = 0; // Tổng số lượt đánh giá
	}
}
