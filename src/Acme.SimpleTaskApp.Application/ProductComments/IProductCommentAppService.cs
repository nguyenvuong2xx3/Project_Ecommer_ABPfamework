using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.ProductComments.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductComments
{
	public interface IProductCommentAppService : IApplicationService
	{
		// Lấy tất cả comment của một sản phẩm có phân trang
		Task<PagedResultDto<ProductCommentDto>> GetAllComments(GetAllProductCommentsInput input);

		// Lấy tất cả comment của một biến thể sản phẩm theo cây (có replies)
		Task<List<ProductCommentDto>> GetProductVariantCommentsTree(int productVariantId);

		// Tạo comment mới
		Task<ProductCommentDto> CreateComment(CreateProductCommentDto input);

		// Cập nhật comment
		Task<ProductCommentDto> UpdateComment(UpdateProductCommentDto input);

		// Xóa comment
		Task DeleteComment(int id);

		// Duyệt / từ chối comment (admin)
		Task ApproveComment(int id, bool isApproved);
	}
}
