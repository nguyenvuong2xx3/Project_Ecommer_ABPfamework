using Abp;
using Abp.Dependency;
using Abp.Notifications;
using Abp.RealTime;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Realtime
{
	public class SignalRRealTimeNotifier : IRealTimeNotifier, ITransientDependency
	{
		private readonly IHubContext<NotificationHub> _hubContext;

		/// <summary>
		/// Chỉ sử dụng khi notification được gửi với mục tiêu cụ thể
		/// </summary>
		public bool UseOnlyIfRequestedAsTarget => false;

		public SignalRRealTimeNotifier(IHubContext<NotificationHub> hubContext)
		{
			_hubContext = hubContext;
		}

		/// <summary>
		/// Gửi notification đến nhiều user online
		/// </summary>
		public async Task SendNotificationsAsync(UserNotification[] userNotifications)
		{
			foreach (var userNotification in userNotifications)
			{
				await SendNotificationAsync(userNotification);
			}
		}

		/// <summary>
		/// Gửi notification đến một user cụ thể
		/// </summary>
		private async Task SendNotificationAsync(UserNotification userNotification)
		{
			var notificationData = new
			{
				id = userNotification.Id,
				notificationName = userNotification.Notification.NotificationName,
				data = userNotification.Notification.Data,
				severity = userNotification.Notification.Severity,
				creationTime = userNotification.Notification.CreationTime,
				state = userNotification.State
			};

			try
			{
				var userIdString = userNotification.UserId.ToString();
				await _hubContext.Clients.User(userIdString)
					.SendAsync("getNotification", notificationData);
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error sending notification: {ex.Message}");
			}
		}
	}
}
