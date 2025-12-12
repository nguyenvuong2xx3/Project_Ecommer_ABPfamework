using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductComments
{
	/// <summary>
	/// Interface for broadcasting product comment updates via SignalR
	/// </summary>
	public interface IProductCommentBroadcaster
	{
		/// <summary>
		/// Phát comment mới tới tất cả client đang join nhóm của productVariantId.
		/// </summary>
		Task BroadcastNewComment(int productVariantId, object commentData);

		/// <summary>
		///Phát cập nhật comment (cùng group)
		/// </summary>
		Task BroadcastCommentUpdate(int productVariantId, int commentId, object commentData);

		/// <summary>
		/// Phát xóa comment (cùng group)
		/// </summary>
		Task BroadcastCommentDelete(int productVariantId, int commentId);
	}
}
