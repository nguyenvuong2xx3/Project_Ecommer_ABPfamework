using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Products
{
	public class ProductImage : Entity<int>
	{
		public string ImageUrl { get; set; }    // Đường dẫn ảnh
		public int SortOrder { get; set; }      // Thứ tự hiển thị

		public string AltText { get; set; }        // Dữ liệu bổ sung: văn bản thay thế
		public int? ProductVariantId { get; set; } // ảnh riêng cho từng biến thể

	}
}
