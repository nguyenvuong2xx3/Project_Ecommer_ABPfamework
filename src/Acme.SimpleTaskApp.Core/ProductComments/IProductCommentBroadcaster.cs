using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductComments
{
	public interface IProductCommentBroadcaster
	{
		// Phát comment mới tới tất cả client trong nhóm biến thể sản phẩm
		Task BroadcastNewComment(int productVariantId, object commentData);

		// Phát cập nhật comment tới tất cả client
		Task BroadcastCommentUpdate(int productVariantId, int commentId, object commentData);

		// Phát xóa comment tới tất cả client
		Task BroadcastCommentDelete(int productVariantId, int commentId);
	}
}
