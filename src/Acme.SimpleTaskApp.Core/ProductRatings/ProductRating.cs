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

		// tiêu đề
		[MaxLength(200)]
		public string Title { get; set; }

		// mô tả
		[MaxLength(2000)]
		public string ReviewText { get; set; }

		/// // url ảnh
		[MaxLength(1000)]
		public string ImageUrls { get; set; }

		//kiểm tra người dùng thực sự mua sản phẩm không?
		public bool IsVerifiedPurchase { get; set; }

		public bool IsApproved { get; set; }

		/// <summary>
		/// Số lượt hữu ích (người dùng thấy đánh giá này hữu ích)
		/// </summary>
		public int HelpfulCount { get; set; }

		/// <summary>
		///Số lượt không hữu ích (người dùng không thấy đánh giá này hữu ích)
		/// </summary>
		public int NotHelpfulCount { get; set; }

		[MaxLength(1000)]
		public string AdminResponse { get; set; }
		public DateTime? AdminResponseTime { get; set; }

		public bool IsEdited { get; set; }

		public DateTime? EditedTime { get; set; }

		public ProductRating()
		{
			IsApproved = true; // Auto-approve by default
			HelpfulCount = 0;
			NotHelpfulCount = 0;
			IsEdited = false;
			IsVerifiedPurchase = false;
		}
	}
}
