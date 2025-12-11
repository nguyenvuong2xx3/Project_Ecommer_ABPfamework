using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Acme.SimpleTaskApp.ProductComments;
using System;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.ProductComments.Dtos
{
	[AutoMapFrom(typeof(ProductComment))]
	public class ProductCommentDto : EntityDto<int>
	{
		public long UserId { get; set; }
		public string UserName { get; set; }
		public string UserFullName { get; set; } // Gán từ code, không tính toán
		public string UserEmail { get; set; }
		
		public int ProductId { get; set; }
		public string Content { get; set; }
		
		public int? ParentCommentId { get; set; }
		
		public bool IsApproved { get; set; }
		public bool IsEdited { get; set; }
		public DateTime? EditedTime { get; set; }
		public DateTime CreationTime { get; set; }
		
		// Danh sách replies
		public List<ProductCommentDto> Replies { get; set; } = new List<ProductCommentDto>();
	}

	public class CreateProductCommentDto
	{
		public int ProductVariantId { get; set; }
		public string Content { get; set; }
		public int? ParentCommentId { get; set; }
	}

	public class UpdateProductCommentDto : EntityDto<int>
	{
		public string Content { get; set; }
	}

	public class GetAllProductCommentsInput : PagedAndSortedResultRequestDto
	{
		public int? ProductVariantId { get; set; }
		public long? UserId { get; set; }
		public bool? IsApproved { get; set; }
	}
}
