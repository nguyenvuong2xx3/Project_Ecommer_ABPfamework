using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Acme.SimpleTaskApp.Products.Dtos
{
	public class CreateProductDto : EntityDto<int>
	{
		public string SKU { get; set; }           // Mã sản phẩm
		public string Name { get; set; }  // tên sản phẩm
		public string? Description { get; set; } //mô tả
		
		// Thông số cho điện thoại
		public string Screen { get; set; }      // Ví dụ: "6.1 inch, Super Retina XDR, ProMotion"
		public string Processor { get; set; }   // Ví dụ: "Apple A17 Pro"
		public string CameraSystem { get; set; } // Ví dụ: "Hệ thống 3 camera: 48MP Chính,..."
		public string Battery { get; set; }     // Ví dụ: "Xem video lên đến 23 giờ"
		
		// Thông số cho phụ kiện
		public string ProductType { get; set; } // "Phone", "Accessory"
		public string Warranty { get; set; }    // Ví dụ: "12 tháng", "24 tháng"
		public string Compatibility { get; set; } // Ví dụ: "iPhone 13/14/15", "Samsung Galaxy S21+"
		public string Material { get; set; }    // Ví dụ: "Silicon", "Nhựa cứng", "Kim loại"
		public string Connector { get; set; }   // Ví dụ: "USB-C", "Lightning", "3.5mm"
		
		public int? CategoryId { get; set; }       // Danh mục sản phẩm
		public int StockQuantity { get; set; }    // Tồn kho hiện tại tổng của các biến thể??
		//public Product Product { get; set; }

		// Thêm danh sách biến thể và ảnh
		public List<ProductVariant> ProductVariants { get; set; }
		public List<ProductImage> ProductImages { get; set; }
	}
}
