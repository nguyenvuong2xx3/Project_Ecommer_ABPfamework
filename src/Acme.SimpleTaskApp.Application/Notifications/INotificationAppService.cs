using Abp;
using Abp.Application.Services;
using Abp.Notifications;
using Acme.SimpleTaskApp.Notifications.Dtos;
using System;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications
{
	public interface INotificationAppService : IApplicationService
	{
		Task<GetNotificationsOutput> GetUserNotifications(GetUserNotificationsInput input);
		
		Task Subscribe_SentFriendshipRequest(int? tenantId, long userId);
		
		Task Publish_SentFriendshipRequest(
			string senderUserName, 
			string friendshipMessage, 
			UserIdentifier targetUserId);
		
		Task Publish_LowDisk(int remainingDiskInMb);

		/// <summary>
		/// Gửi thông báo đơn hàng mới
		/// </summary>
		Task PublishNewOrderNotification(int orderId, string customerName, decimal totalPrice);

		/// <summary>
		/// Gửi thông báo thay đổi trạng thái đơn hàng
		/// </summary>
		Task PublishOrderStatusChangedNotification(int orderId, string newStatus, UserIdentifier userId);

		/// <summary>
		/// Gửi cảnh báo tồn kho thấp
		/// </summary>
		Task PublishLowStockNotification(string productName, int remainingStock);

		/// <summary>
		/// Đánh dấu notification đã đọc
		/// </summary>
		Task SetNotificationAsRead(Guid notificationId);

		/// <summary>
		/// Đánh dấu tất cả đã đọc
		/// </summary>
		Task SetAllNotificationsAsRead();

		/// <summary>
		/// Xóa notification
		/// </summary>
		Task DeleteNotification(Guid notificationId);
	}
}
