using Abp.Dependency;
using Acme.SimpleTaskApp.ProductComments;
using Acme.SimpleTaskApp.Products;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Realtime
{
    /// <summary>
    /// Implementation of IProductCommentBroadcaster using SignalR
    /// </summary>
    public class ProductCommentBroadcaster : IProductCommentBroadcaster, ITransientDependency
    {
        private readonly IHubContext<ProductCommentHub> _hubContext;

        public ProductCommentBroadcaster(IHubContext<ProductCommentHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastNewComment(int productId, object commentData)
        {
            var groupName = GetProductGroupName(productId);
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveComment", new
            {
                comment = commentData,
                action = "new"
            });
        }

        public async Task BroadcastCommentUpdate(int productVariantId, int commentId, object commentData)
        {
            var groupName = GetProductGroupName(productVariantId);
            await _hubContext.Clients.Group(groupName).SendAsync("CommentUpdated", new
            {
                commentId = commentId,
                comment = commentData,
                action = "update"
            });
        }

        public async Task BroadcastCommentDelete(int productId, int commentId)
        {
            var groupName = GetProductGroupName(productId);
            await _hubContext.Clients.Group(groupName).SendAsync("CommentDeleted", new
            {
                commentId = commentId,
                action = "delete"
            });
        }

        private static string GetProductGroupName(int productVariantId)
        {
            return $"roductVariantId_{productVariantId}_Comments";
        }
    }
}
