using Abp.Application.Services.Dto;
using System;

namespace Acme.SimpleTaskApp.Sales.Dtos
{
	public class GetAllSaleDto : PagedResultRequestDto
	{
		// Tìm ki?m chung
		public string? Filter { get; set; } // Tìm theo Name ho?c VoucherCode

		// L?c theo lo?i
		public DiscountType? DiscountType { get; set; }
		public DiscountApplication? ApplyTo { get; set; }

		// L?c theo th?i gian
		public DateTime? StartDateFrom { get; set; }
		public DateTime? StartDateTo { get; set; }
		public DateTime? EndDateFrom { get; set; }
		public DateTime? EndDateTo { get; set; }

		// L?c theo giá tr?
		public decimal? MinDiscountPercentage { get; set; }
		public decimal? MaxDiscountPercentage { get; set; }

		// L?c theo tr?ng thái
		public bool? IsActive { get; set; }

		// L?c theo voucher
		public string? VoucherCode { get; set; }
		public bool? HasUsageLimitReached { get; set; } // ?ã h?t l??t s? d?ng
	}
}
