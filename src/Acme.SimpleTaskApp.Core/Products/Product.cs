using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using Acme.SimpleTaskApp.Categories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Acme.SimpleTaskApp.Products
{
	[Table("AppProducts")]
	public class Product : FullAuditedEntity<int>
	{
		public string Name { get; set; }  // tên sản phẩm
		public string? Description { get; set; } //mô tả

		// Thông số cho điện thoại
		public string Screen { get; set; }      // Ví dụ: "6.1 inch, Super Retina XDR, ProMotion"
		public string Processor { get; set; }   // Ví dụ: "Apple A17 Pro"
		public string CameraSystem { get; set; } // Ví dụ: "Hệ thống 3 camera: 48MP Chính,..."
		public string Battery { get; set; }     // Ví dụ: "Xem video lên đến 23 giờ"

		// Thông số cho phụ kiện
		public string ProductType { get; set; } // "Phone", "Accessory"
		public string Warranty { get; set; }    // Ví dụ: "12 tháng", "24 tháng" ...
		public string Compatibility { get; set; } // Ví dụ: "iPhone 13/14/15", "Samsung Galaxy S21+" ...
		public string Material { get; set; }    // Ví dụ: "Silicon", "Nhựa cứng", "Kim loại"
		public string Connector { get; set; }   // Ví dụ: "USB-C", "Lightning", "3.5mm"

		[NotMapped] public List<IFormFile> Images { get; set; } // cho BE nhận file từ FE
		[NotMapped] public string ImageUrl { get; set; } // cho BE trả về
		[NotMapped] public List<string> ImageUrls { get; set; } // cho BE trả về
		[NotMapped] public List<string> DeletedImageUrls { get; set; } // cho BE trả về
		[NotMapped] public List<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>(); // cho BE trả về
		[NotMapped] public ProductVariant ProductVariant { get; set; } // cho BE trả về cho trang chi tiết
		[NotMapped] public List<ProductImage> ProductImages { get; set; } // cho BE trả về
		public int? CategoryId { get; set; }       // Danh mục sản phẩm
		public int StockQuantity { get; set; }    // Tồn kho hiện tại tổng của các biến thể??

	}
}
