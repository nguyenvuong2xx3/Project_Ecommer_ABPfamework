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

		// C?u hình lo?i gi?m giá
		public DiscountType DiscountType { get; set; }
		public string DiscountTypeName { get; set; } // Tên hi?n th?
		public string? VoucherCode { get; set; }
		public int? UsageLimit { get; set; }
		public int UsedCount { get; set; }
		public int? RemainingUsage { get; set; } // S? l??t còn l?i

		// Th?i gian áp d?ng
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Status { get; set; } // "S?p di?n ra", "?ang ho?t ??ng", "?ã k?t thúc"

		// M?c gi?m giá
		public decimal DiscountPercentage { get; set; }
		public decimal? MaximumDiscountAmount { get; set; }
		public decimal? MinimumOrderValue { get; set; }

		// ??i t??ng áp d?ng
		public DiscountApplication ApplyTo { get; set; }
		public string ApplyToName { get; set; } // Tên hi?n th?
		public List<int>? CategoryIds { get; set; }
		public List<int>? ProductIds { get; set; }
		public List<int>? ProductVariantIds { get; set; }

		// Tr?ng thái
		public bool IsActive { get; set; }
		public bool CanUse { get; set; } // Có th? s? d?ng hay không (tính toán d?a trên IsActive, th?i gian, usage limit)

		// Metadata
		public DateTime CreationTime { get; set; }
	}
}
