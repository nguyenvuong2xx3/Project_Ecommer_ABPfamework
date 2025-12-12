using System;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Sales.Dtos
{
	public class SaleDto
	{
		public int Id { get; set; }

		// Thông tin chung
		public string Name { get; set; }
		public string? Description { get; set; }

		// Cấu hình loại giảm giá
		public DiscountType DiscountType { get; set; }
		public string DiscountTypeName { get; set; } // Tên hiển thị
		public string? VoucherCode { get; set; }
		public int? UsageLimit { get; set; }
		public int UsedCount { get; set; }
		public int? RemainingUsage { get; set; } // Số lượt còn lại

		// Thời gian áp dụng
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Status { get; set; } // sắp diễn ra/đang hoạt động/đã kết thúc

		// Mã giảm giás
		public decimal DiscountPercentage { get; set; }
		public decimal? MaximumDiscountAmount { get; set; }
		public decimal? MinimumOrderValue { get; set; }

		// Phạm vi áp dụng
		public DiscountApplication ApplyTo { get; set; }
		public string ApplyToName { get; set; } // Tên hiển thị
		public List<int>? CategoryIds { get; set; }
		public List<int>? ProductIds { get; set; }
		public List<int>? ProductVariantIds { get; set; }

		// Trạng thái
		public bool IsActive { get; set; }
		public bool CanUse { get; set; } // Bật tắt sử dụng

		// Metadata
		public DateTime CreationTime { get; set; }
	}
}
