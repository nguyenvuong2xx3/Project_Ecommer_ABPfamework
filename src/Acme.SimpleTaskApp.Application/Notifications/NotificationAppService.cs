using Abp;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Localization;
using Abp.Notifications;
using Acme.SimpleTaskApp.Notifications.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications
{
	public class NotificationAppService
	{
		private readonly INotificationPublisher _notificationPublisher;
		private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
		public NotificationAppService(INotificationPublisher notificationPublisher, INotificationSubscriptionManager notificationSubscriptionManager)
		{
			_notificationPublisher = notificationPublisher;
			_notificationSubscriptionManager = notificationSubscriptionManager;
		}

		// đăng kí thông báo
		public async Task Subscribe_SentFriendshipRequest(int? tenantId, long userId)
		{
			await _notificationSubscriptionManager.SubscribeAsync(new UserIdentifier(tenantId, userId), "SentFriendshipRequest");
		}

		// xuất bản thông báo
		public async Task Publish_SentFriendshipRequest(string senderUserName, string friendshipMessage, UserIdentifier targetUserId)
		{
			await _notificationPublisher.PublishAsync("SentFriendshipRequest", new SentFriendshipRequestNotificationData(senderUserName, friendshipMessage), userIds: new[] { targetUserId });
		}


		//Send a general notification to all subscribed users in current tenant (tenant in the session)
		public async Task Publish_LowDisk(int remainingDiskInMb)
		{
			//Example "LowDiskWarningMessage" content for English -> "Attention! Only {remainingDiskInMb} MBs left on the disk!"
			var data = new LocalizableMessageNotificationData(new LocalizableString("LowDiskWarningMessage", "MyLocalizationSourceName"));
			data["remainingDiskInMb"] = remainingDiskInMb;

			await _notificationPublisher.PublishAsync("System.LowDisk", data, severity: NotificationSeverity.Warn);
		}
	}
}
