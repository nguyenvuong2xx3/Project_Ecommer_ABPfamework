using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Sales
{
	public class Sale : CreationAuditedEntity<int>
	{
		// Thông tin chung
		public string Name { get; set; } // Tên chương trình giảm giá
		public string? Description { get; set; }

		// Cấu hình loại giảm giá
		public DiscountType DiscountType { get; set; } // Loại giảm giá
		public string? VoucherCode { get; set; } // Mã giảm giá (nếu có)
		public int? UsageLimit { get; set; } // Số lượng voucher tối đa
		public int UsedCount { get; set; } = 0; // Số lượng đã sử dụng

		// Thời gian áp dụng
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		// Mức giảm giá
		public decimal DiscountPercentage { get; set; }
		public decimal? MaximumDiscountAmount { get; set; } // Số tiền giảm tối đa
		public decimal? MinimumOrderValue { get; set; } // Giá trị đơn hàng tối thiểu

		// Đối tượng áp dụng
		public DiscountApplication ApplyTo { get; set; } // Phạm vi áp dụng
		public List<int>? CategoryIds { get; set; }
		public List<int>? ProductIds { get; set; }
		public List<int>? ProductVariantIds { get; set; }

		// Trạng thái
		public bool IsActive { get; set; } = true;
	}

	// Enum xác định loại giảm giá
	public enum DiscountType
	{
		Automatic,  // Giảm giá tự động không cần mã
		Voucher     // Giảm giá yêu cầu nhập mã
	}

	// Enum xác định phạm vi áp dụng
	public enum DiscountApplication
	{
		EntireOrder,    // Áp dụng toàn bộ đơn hàng
		Categories,     // Áp dụng theo danh mục
		Products,       // Áp dụng theo sản phẩm
		Variants        // Áp dụng theo biến thể sản phẩm
	}
}
