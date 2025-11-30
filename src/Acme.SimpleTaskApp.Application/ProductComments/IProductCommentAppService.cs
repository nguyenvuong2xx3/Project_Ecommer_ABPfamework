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
		/// L?y t?t c? comments c?a m?t s?n ph?m (có phân trang)
		/// </summary>
		Task<PagedResultDto<ProductCommentDto>> GetAllComments(GetAllProductCommentsInput input);

		/// <summary>
		/// L?y t?t c? comments c?a m?t s?n ph?m (d?ng tree - có replies)
		/// </summary>
		Task<List<ProductCommentDto>> GetProductCommentsTree(int productId);

		/// <summary>
		/// T?o comment m?i
		/// </summary>
		Task<ProductCommentDto> CreateComment(CreateProductCommentDto input);

		/// <summary>
		/// C?p nh?t comment
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
