using Abp.Dependency;
using Acme.SimpleTaskApp.ProductComments;
using Acme.SimpleTaskApp.Products;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Realtime
{
    public class ProductCommentBroadcaster : IProductCommentBroadcaster, ITransientDependency
    {
        private readonly IHubContext<ProductCommentHub> _hubContext;

        public ProductCommentBroadcaster(IHubContext<ProductCommentHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Phát comment mới tới nhóm biến thể sản phẩm
        public async Task BroadcastNewComment(int productVariantId, object commentData)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveComment", new
            {
                comment = commentData,
                action = "new"
            });
        }

        // Phát cập nhật comment tới nhóm
        public async Task BroadcastCommentUpdate(int productVariantId, int commentId, object commentData)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await _hubContext.Clients.Group(groupName).SendAsync("CommentUpdated", new
            {
                commentId = commentId,
                comment = commentData,
                action = "update"
            });
        }

        // Phát thông báo xóa comment
        public async Task BroadcastCommentDelete(int productVariantId, int commentId)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await _hubContext.Clients.Group(groupName).SendAsync("CommentDeleted", new
            {
                commentId = commentId,
                action = "delete"
            });
        }

        private static string GetProductVariantGroupName(int productVariantId)
        {
            return $"ProductVariant_{productVariantId}_Comments";
        }
    }
}
