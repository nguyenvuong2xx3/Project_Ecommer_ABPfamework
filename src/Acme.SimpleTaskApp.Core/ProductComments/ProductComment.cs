using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace Acme.SimpleTaskApp.ProductComments
{
	public class ProductComment : FullAuditedEntity<int>
	{
		// Thông tin người comment (chỉ lưu ID, không có navigation property)
		public long UserId { get; set; }

		// Sản phẩm được comment (chỉ lưu ID, không có navigation property)
		public int ProductVariantId { get; set; }

		// Nội dung comment
		[Required]
		[StringLength(1000)]
		public string Content { get; set; }

		// Comment cha (cho reply/nested comments) - chỉ lưu ID
		public int? ParentCommentId { get; set; }

		// Trạng thái
		public bool IsApproved { get; set; } = true; // Mặc định approved
		public bool IsEdited { get; set; } = false;
		public DateTime? EditedTime { get; set; }
	}
}
