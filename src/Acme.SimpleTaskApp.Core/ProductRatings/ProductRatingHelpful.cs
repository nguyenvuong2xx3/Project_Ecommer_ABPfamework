using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;

namespace Acme.SimpleTaskApp.ProductRatings
{
	// Lưu lại người dùng đã bầu hữu ích/không hữu ích cho từng đánh giá
	// Ngăn người dùng bầu nhiều lần cho cùng một đánh giá
	public class ProductRatingHelpful : CreationAuditedEntity<int>, IMayHaveTenant
	{
		public int? TenantId { get; set; }

		// ID người dùng đã bầu (không có FK)
		[Required]
		public long UserId { get; set; }

		// ID đánh giá được bầu (không có FK)
		[Required]
		public int ProductRatingId { get; set; }

		// true = hữu ích, false = không hữu ích
		[Required]
		public bool IsHelpful { get; set; }
	}
}
