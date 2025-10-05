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

		// --- CÁC THÔNG SỐ KỸ THUẬT CHUNG ---
		// Đây là những thông số không đổi giữa các biến thể

		public string Screen { get; set; }      // Ví dụ: "6.1 inch, Super Retina XDR, ProMotion"
		public string Processor { get; set; }   // Ví dụ: "Apple A17 Pro"
		public string CameraSystem { get; set; } // Ví dụ: "Hệ thống 3 camera: 48MP Chính,..."
		public string Battery { get; set; }     // Ví dụ: "Xem video lên đến 23 giờ"

		public List<ProductImage>? ProductImage { get; set; } // lấy ảnh theo bảng ProductImage
		[NotMapped] public List<IFormFile> Image { get; set; }
		public int CategoryId { get; set; }       // Danh mục sản phẩm
		public int StockQuantity { get; set; }    // Tồn kho hiện tại tổng của các biến thể??
		public DateTime? ExpiryDate { get; set; } // Hạn sử dụng
		
	}
}
