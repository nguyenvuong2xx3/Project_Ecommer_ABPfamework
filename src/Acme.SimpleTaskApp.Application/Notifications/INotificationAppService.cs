using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
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

		Task Publish_SentFriendshipRequest(string senderUserName,string friendshipMessage,UserIdentifier targetUserId);

		Task Publish_LowDisk(int remainingDiskInMb);

		/// Gửi thông báo đơn hàng mới
		Task PublishNewOrderNotification(int orderId, string customerName, decimal totalPrice);

		/// Gửi thông báo thay đổi trạng thái đơn hàng
		Task PublishOrderStatusChangedNotification(int orderId, string newStatus, UserIdentifier userId);

		/// Gửi cảnh báo tồn kho thấp
		Task PublishLowStockNotification(string productName, int remainingStock);

		/// Đánh dấu notification đã đọc
		Task<SetNotificationAsReadOutput> SetNotificationAsRead(EntityDto<Guid> input);

		/// Đánh dấu tất cả đã đọc
		Task SetAllNotificationsAsRead();

		/// Xóa notification
		Task DeleteNotification(EntityDto<Guid> input);
	}
}
