using Abp.Dependency;
using Abp.RealTime;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Realtime
{
    /// <summary>
    /// SignalR Hub for real-time product comments
    /// </summary>
    public class ProductCommentHub : Hub, ITransientDependency
    {
        public IAbpSession AbpSession { get; set; }

        public ProductCommentHub()
        {
            AbpSession = NullAbpSession.Instance;
        }

        /// <summary>
        /// Join a product variant's comment group to receive real-time updates
        /// </summary>
        public async Task JoinProductGroup(int productVariantId)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Leave a product variant's comment group
        /// </summary>
        public async Task LeaveProductGroup(int productVariantId)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Send new comment to all clients in the product variant group
        /// </summary>
        public async Task SendCommentToProduct(int productVariantId, object commentData)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Clients.Group(groupName).SendAsync("ReceiveComment", commentData);
        }

        /// <summary>
        /// Send comment update to all clients
        /// </summary>
        public async Task SendCommentUpdate(int productVariantId, int commentId, object commentData)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Clients.Group(groupName).SendAsync("CommentUpdated", commentId, commentData);
        }

        /// <summary>
        /// Send comment deletion notification
        /// </summary>
        public async Task SendCommentDeleted(int productVariantId, int commentId)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Clients.Group(groupName).SendAsync("CommentDeleted", commentId);
        }

        /// <summary>
        /// User is typing indicator
        /// </summary>
        public async Task UserTyping(int productVariantId, string userName)
        {
            var groupName = GetProductVariantGroupName(productVariantId);
            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", userName);
        }

        /// <summary>
        /// User stopped typing
        /// </summary>
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
