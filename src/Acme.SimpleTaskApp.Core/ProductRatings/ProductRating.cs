using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Acme.SimpleTaskApp.ProductRatings
{
	/// <summary>
	/// Product Rating Entity - NO FOREIGN KEY design
	/// Users can rate products with 1-5 stars and optional review text
	/// </summary>
	public class ProductRating : FullAuditedEntity<int>, IMayHaveTenant
	{
		public int? TenantId { get; set; }

		/// <summary>
		/// User ID who created this rating (NO FK)
		/// </summary>
		[Required]
		public long UserId { get; set; }

		/// <summary>
		/// Product Variant ID being rated (NO FK)
		/// </summary>
		[Required]
		public int ProductVariantId { get; set; }

		/// <summary>
		/// Order ID - to verify user purchased this product (NO FK)
		/// </summary>
		public int? OrderId { get; set; }

		/// <summary>
		/// Rating value (1-5 stars)
		/// </summary>
		[Required]
		[Range(1, 5)]
		public int Rating { get; set; }

		/// <summary>
		/// Review title (optional)
		/// </summary>
		[MaxLength(200)]
		public string Title { get; set; }

		/// <summary>
		/// Review content (optional)
		/// </summary>
		[MaxLength(2000)]
		public string ReviewText { get; set; }

		/// <summary>
		/// Image URLs for review photos (comma-separated)
		/// </summary>
		[MaxLength(1000)]
		public string ImageUrls { get; set; }

		/// <summary>
		/// Is this rating verified (user actually bought the product)
		/// </summary>
		public bool IsVerifiedPurchase { get; set; }

		/// <summary>
		/// Is this rating approved by admin
		/// </summary>
		public bool IsApproved { get; set; }

		/// <summary>
		/// Number of helpful votes (users found this review helpful)
		/// </summary>
		public int HelpfulCount { get; set; }

		/// <summary>
		/// Number of not helpful votes
		/// </summary>
		public int NotHelpfulCount { get; set; }

		/// <summary>
		/// Admin response to this rating
		/// </summary>
		[MaxLength(1000)]
		public string AdminResponse { get; set; }

		/// <summary>
		/// When admin responded
		/// </summary>
		public DateTime? AdminResponseTime { get; set; }

		/// <summary>
		/// Was this rating edited by user
		/// </summary>
		public bool IsEdited { get; set; }

		/// <summary>
		/// When user edited this rating
		/// </summary>
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
