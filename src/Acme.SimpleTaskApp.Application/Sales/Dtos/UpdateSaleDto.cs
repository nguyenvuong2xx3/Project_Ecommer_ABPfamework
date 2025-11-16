using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Acme.SimpleTaskApp.Sales.Dtos
{
	public class UpdateSaleDto
	{
		[Required]
		public int Id { get; set; }

		// Thông tin chung
		[Required(ErrorMessage = "Tên ch??ng trình không ???c ?? tr?ng")]
		[StringLength(200, MinimumLength = 3, ErrorMessage = "Tên ph?i có ?? dài t? 3 ??n 200 ký t?")]
		public string Name { get; set; }

		[StringLength(1000)]
		public string? Description { get; set; }

		// C?u hình lo?i gi?m giá
		[Required(ErrorMessage = "Lo?i gi?m giá không ???c ?? tr?ng")]
		public DiscountType DiscountType { get; set; }

		[StringLength(50)]
		public string? VoucherCode { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "S? l??ng s? d?ng ph?i l?n h?n 0")]
		public int? UsageLimit { get; set; }

		// Th?i gian áp d?ng
		[Required(ErrorMessage = "Ngày b?t ??u không ???c ?? tr?ng")]
		public DateTime StartDate { get; set; }

		[Required(ErrorMessage = "Ngày k?t thúc không ???c ?? tr?ng")]
		public DateTime EndDate { get; set; }

		// M?c gi?m giá
		[Required(ErrorMessage = "Ph?n tr?m gi?m giá không ???c ?? tr?ng")]
		[Range(0, 100, ErrorMessage = "Ph?n tr?m gi?m giá ph?i t? 0 ??n 100")]
		public decimal DiscountPercentage { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "S? ti?n gi?m t?i ?a ph?i l?n h?n ho?c b?ng 0")]
		public decimal? MaximumDiscountAmount { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Giá tr? ??n hàng t?i thi?u ph?i l?n h?n ho?c b?ng 0")]
		public decimal? MinimumOrderValue { get; set; }

		// ??i t??ng áp d?ng
		[Required(ErrorMessage = "Ph?m vi áp d?ng không ???c ?? tr?ng")]
		public DiscountApplication ApplyTo { get; set; }

		public List<int>? CategoryIds { get; set; }
		public List<int>? ProductIds { get; set; }
		public List<int>? ProductVariantIds { get; set; }

		// Tr?ng thái
		public bool IsActive { get; set; } = true;
	}
}
