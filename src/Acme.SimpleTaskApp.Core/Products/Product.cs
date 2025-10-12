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
		public string SKU { get; set; }           // Mã sản phẩm
		public string Name { get; set; }  // tên sản phẩm
		public string? Description { get; set; } //mô tả
		public string Screen { get; set; }      // Ví dụ: "6.1 inch, Super Retina XDR, ProMotion"
		public string Processor { get; set; }   // Ví dụ: "Apple A17 Pro"
		public string CameraSystem { get; set; } // Ví dụ: "Hệ thống 3 camera: 48MP Chính,..."
		public string Battery { get; set; }     // Ví dụ: "Xem video lên đến 23 giờ"
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
