using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.ProductComments.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductComments
{
	public interface IProductCommentAppService : IApplicationService
	{
		/// <summary>
		/// lấy tất cả cmt của một sản phẩm có phân trang
		/// </summary>
		Task<PagedResultDto<ProductCommentDto>> GetAllComments(GetAllProductCommentsInput input);

		/// <summary>
		/// lấy tất cả cmt của một sản phẩm theo tree - có replies
		/// </summary>
		Task<List<ProductCommentDto>> GetProductVariantCommentsTree(int productVariantId);

		/// <summary>
		/// tạo mới
		/// </summary>
		Task<ProductCommentDto> CreateComment(CreateProductCommentDto input);

		/// <summary>
		/// update
		/// </summary>
		Task<ProductCommentDto> UpdateComment(UpdateProductCommentDto input);

		/// <summary>
		/// Xóa comment
		/// </summary>
		Task DeleteComment(int id);

		/// <summary>
		/// Approve/Reject comment (cho admin)
		/// </summary>
		Task ApproveComment(int id, bool isApproved);
	}
}
