using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Acme.SimpleTaskApp.Sales.Dtos
{
	public class CreateSaleDto
	{
		// Thông tin chung
		[Required(ErrorMessage = "Tên chương trình không được để trống")]
		[StringLength(200, MinimumLength = 3, ErrorMessage = "Tên phải có độ dài từ 3 đến 200 ký tự")]
		public string Name { get; set; }

		[StringLength(1000)]
		public string? Description { get; set; }

		// Cấu hình loại giảm giá
		[Required(ErrorMessage = "Loại giảm giá không được để trống")]
		public DiscountType DiscountType { get; set; }

		[StringLength(50)]
		public string? VoucherCode { get; set; } // Chỉ bắt buộc nếu DiscountType = Voucher

		[Range(1, int.MaxValue, ErrorMessage = "Số lượng sử dụng phải lớn hơn 0")]
		public int? UsageLimit { get; set; }

		// Thời gian áp dụng
		[Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
		public DateTime StartDate { get; set; }

		[Required(ErrorMessage = "Ngày kết thúc không được để trống")]
		public DateTime EndDate { get; set; }

		// Mức giảm giá
		[Required(ErrorMessage = "Phần trăm giảm giá không được để trống")]
		[Range(0, 100, ErrorMessage = "Phần trăm giảm giá phải từ 0 đến 100")]
		public decimal DiscountPercentage { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm tối đa phải lớn hơn hoặc bằng 0")]
		public decimal? MaximumDiscountAmount { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu phải lớn hơn hoặc bằng 0")]
		public decimal? MinimumOrderValue { get; set; }

		// Đối tượng áp dụng
		[Required(ErrorMessage = "Phạm vi áp dụng không được để trống")]
		public DiscountApplication ApplyTo { get; set; }

		public List<int>? CategoryIds { get; set; }
		public List<int>? ProductIds { get; set; }
		public List<int>? ProductVariantIds { get; set; }

		// Trạng thái
		public bool IsActive { get; set; } = true;
	}
}
