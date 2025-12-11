using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Localization;
using Abp.Notifications;
using Abp.Runtime.Session;
using Acme.SimpleTaskApp.Notifications.Dtos;
using System;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications
{
	public class NotificationAppService : ApplicationService, INotificationAppService
	{
		private readonly INotificationPublisher _notificationPublisher;
		private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
		private readonly IUserNotificationManager _userNotificationManager;
		
		public NotificationAppService(
			INotificationPublisher notificationPublisher, 
			INotificationSubscriptionManager notificationSubscriptionManager,
			IUserNotificationManager userNotificationManager)
		{
			_notificationPublisher = notificationPublisher;
			_notificationSubscriptionManager = notificationSubscriptionManager;
			_userNotificationManager = userNotificationManager;
		}

		// đăng kí thông báo
		public async Task Subscribe_SentFriendshipRequest(int? tenantId, long userId)
		{
			await _notificationSubscriptionManager.SubscribeAsync(
				new UserIdentifier(tenantId, userId), 
				"SentFriendshipRequest");
		}

		// xuất bản thông báo
		public async Task Publish_SentFriendshipRequest(
			string senderUserName, 
			string friendshipMessage, 
			UserIdentifier targetUserId)
		{
			await _notificationPublisher.PublishAsync(
				"SentFriendshipRequest", 
				new SentFriendshipRequestNotificationData(senderUserName, friendshipMessage), 
				userIds: new[] { targetUserId });
		}

		// Send a general notification to all subscribed users in current tenant
		public async Task Publish_LowDisk(int remainingDiskInMb)
		{
			var data = new LocalizableMessageNotificationData(
				new LocalizableString("LowDiskWarningMessage", "MyLocalizationSourceName"));
			data["remainingDiskInMb"] = remainingDiskInMb;

			await _notificationPublisher.PublishAsync(
				"System.LowDisk", 
				data, 
				severity: NotificationSeverity.Warn);
		}

		/// <summary>
		/// Gửi thông báo đơn hàng mới cho admin
		/// </summary>
		public async Task PublishNewOrderNotification(int orderId, string customerName, decimal totalPrice)
		{
			var data = new NotificationData();
			data["orderId"] = orderId;
			data["customerName"] = customerName;
			data["totalPrice"] = totalPrice;
			data["message"] = $"Đơn hàng mới từ {customerName} với giá trị {totalPrice:N0}đ";

			await _notificationPublisher.PublishAsync(
				"App.NewOrder",
				data,
				severity: NotificationSeverity.Success,
				userIds: null // Gửi cho tất cả admin subscribed
			);
		}

		/// <summary>
		/// Gửi thông báo thay đổi trạng thái đơn hàng cho khách hàng
		/// </summary>
		public async Task PublishOrderStatusChangedNotification(
			int orderId, 
			string newStatus, 
			UserIdentifier userId)
		{
			var data = new NotificationData();
			data["orderId"] = orderId;
			data["status"] = newStatus;
			data["message"] = $"Đơn hàng #{orderId} đã chuyển sang trạng thái: {newStatus}";

			await _notificationPublisher.PublishAsync(
				"App.OrderStatusChanged",
				data,
				severity: NotificationSeverity.Info,
				userIds: new[] { userId }
			);
		}

		/// <summary>
		/// Gửi cảnh báo tồn kho thấp
		/// </summary>
		public async Task PublishLowStockNotification(string productName, int remainingStock)
		{
			var data = new NotificationData();
			data["productName"] = productName;
			data["remainingStock"] = remainingStock;
			data["message"] = $"Sản phẩm '{productName}' chỉ còn {remainingStock} trong kho";

			await _notificationPublisher.PublishAsync(
				"App.LowStock",
				data,
				severity: NotificationSeverity.Warn,
				userIds: null // Gửi cho admin
			);
		}

		[DisableAuditing]
		public async Task<GetNotificationsOutput> GetUserNotifications(GetUserNotificationsInput input)
		{
			var user = base.AbpSession.ToUserIdentifier();
			var total = await _userNotificationManager.GetUserNotificationCountAsync(
				user, 
				input.State, 
				input.StartDate, 
				input.EndDate);
			
			var unreadCount = await _userNotificationManager.GetUserNotificationCountAsync(
				base.AbpSession.ToUserIdentifier(), 
				UserNotificationState.Unread, 
				input.StartDate, 
				input.EndDate);
			
			var notifications = await _userNotificationManager.GetUserNotificationsAsync(
				base.AbpSession.ToUserIdentifier(), 
				input.State, 
				input.SkipCount, 
				input.MaxResultCount, 
				input.StartDate, 
				input.EndDate);
			
			return new GetNotificationsOutput(total, unreadCount, notifications);
		}

		/// <summary>
		/// Đánh dấu notification là đã đọc
		/// </summary>
		public async Task<SetNotificationAsReadOutput> SetNotificationAsRead(EntityDto<Guid> input)
		{
			var user = AbpSession.ToUserIdentifier();
			var userNotification = await _userNotificationManager.GetUserNotificationAsync(
				user.TenantId,
				input.Id);
			
			if (userNotification == null)
			{
				return new SetNotificationAsReadOutput(false);
			}
			
			if (userNotification.UserId != AbpSession.GetUserId())
			{
				return new SetNotificationAsReadOutput(false);
			}
			
			if (userNotification.State == UserNotificationState.Read)
			{
				return new SetNotificationAsReadOutput(false);
			}
			
			await _userNotificationManager.UpdateUserNotificationStateAsync(
				user.TenantId,
				input.Id,
				UserNotificationState.Read);
			
			return new SetNotificationAsReadOutput(true);
		}

		/// <summary>
		/// Đánh dấu tất cả notifications là đã đọc
		/// </summary>
		public async Task SetAllNotificationsAsRead()
		{
			var user = AbpSession.ToUserIdentifier();
			await _userNotificationManager.UpdateAllUserNotificationStatesAsync(
				user,
				UserNotificationState.Read);
		}

		/// <summary>
		/// Xóa notification
		/// </summary>
		public async Task DeleteNotification(EntityDto<Guid> input)
		{
			var user = AbpSession.ToUserIdentifier();
			var notification = await _userNotificationManager.GetUserNotificationAsync(
				user.TenantId,
				input.Id);
			
			if (notification != null && notification.UserId == AbpSession.GetUserId())
			{
				await _userNotificationManager.DeleteUserNotificationAsync(
					user.TenantId,
					input.Id);
			}
		}
	}
}
