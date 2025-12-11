using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductComments
{
    /// <summary>
    /// Interface for broadcasting product comment updates via SignalR
    /// </summary>
    public interface IProductCommentBroadcaster
    {
        /// <summary>
        /// Broadcast new comment to all clients in product group
        /// </summary>
        Task BroadcastNewComment(int productId, object commentData);

        /// <summary>
        /// Broadcast comment update to all clients
        /// </summary>
        Task BroadcastCommentUpdate(int productVariantId, int commentId, object commentData);

        /// <summary>
        /// Broadcast comment deletion to all clients
        /// </summary>
        Task BroadcastCommentDelete(int productId, int commentId);
    }
}
