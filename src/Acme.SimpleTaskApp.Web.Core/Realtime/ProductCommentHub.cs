using Abp.Dependency;
using Abp.RealTime;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Realtime
{
    public class ProductCommentHub : Hub, ITransientDependency
    {
        public IAbpSession AbpSession { get; set; }

        public ProductCommentHub()
        {
            AbpSession = NullAbpSession.Instance;
        }

        // Tham gia nhóm bình luận của một biến thể sản phẩm để nhận cập nhật realtime
        public async Task JoinProductGroup(int productVariantId)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Rời nhóm bình luận của biến thể sản phẩm
        public async Task LeaveProductGroup(int productVariantId)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // Gửi comment mới đến tất cả client trong nhóm
        public async Task SendCommentToProduct(int productVariantId, object commentData)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Clients.Group(groupName).SendAsync("ReceiveComment", commentData);
        }

        // Gửi cập nhật comment đến tất cả client
        public async Task SendCommentUpdate(int productVariantId, int commentId, object commentData)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Clients.Group(groupName).SendAsync("CommentUpdated", commentId, commentData);
        }

        // Gửi thông báo xóa comment
        public async Task SendCommentDeleted(int productVariantId, int commentId)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Clients.Group(groupName).SendAsync("CommentDeleted", commentId);
        }

        // Hiển thị chỉ báo người dùng đang gõ
        public async Task UserTyping(int productVariantId, string userName)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", userName);
        }

        // Hiển thị chỉ báo người dùng dừng gõ
        public async Task UserStoppedTyping(int productVariantId, string userName)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Clients.OthersInGroup(groupName).SendAsync("UserStoppedTyping", userName);
        }

        private static string GetProductVariantGroupName(int productVariantId)
        {
            return $"ProductVariant_{productVariantId}_Comments";
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
