using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Acme.SimpleTaskApp.ProductRatings
{
	public class ProductRating : FullAuditedEntity<int>, IMayHaveTenant
	{
		public int? TenantId { get; set; }

		[Required]
		public long UserId { get; set; }

		[Required]
		public int ProductVariantId { get; set; }
		public int? OrderId { get; set; }

		[Required]
		[Range(1, 5)]
		public int Rating { get; set; }

		// Tiêu đề đánh giá
		[MaxLength(200)]
		public string Title { get; set; }

		// Nội dung mô tả/đánh giá
		[MaxLength(2000)]
		public string ReviewText { get; set; }

		// Danh sách URL ảnh đánh giá (dạng chuỗi, phân tách bởi dấu phẩy)
		[MaxLength(1000)]
		public string ImageUrls { get; set; }

		// Người đánh giá có phải là người đã mua sản phẩm không
		public bool IsVerifiedPurchase { get; set; }

		public bool IsApproved { get; set; }

		// Số lượt người dùng thấy đánh giá hữu ích
		public int HelpfulCount { get; set; }

		// Số lượt người dùng thấy đánh giá không hữu ích
		public int NotHelpfulCount { get; set; }

		[MaxLength(1000)]
		public string AdminResponse { get; set; }
		public DateTime? AdminResponseTime { get; set; }

		public bool IsEdited { get; set; }

		public DateTime? EditedTime { get; set; }

		public ProductRating()
		{
			IsApproved = true; // Tự động duyệt mặc định
			HelpfulCount = 0;
			NotHelpfulCount = 0;
			IsEdited = false;
			IsVerifiedPurchase = false;
		}
	}
}
