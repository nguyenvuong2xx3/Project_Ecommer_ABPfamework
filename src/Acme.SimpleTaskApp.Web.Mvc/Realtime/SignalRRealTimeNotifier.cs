using Abp;
using Abp.Dependency;
using Abp.Notifications;
using Abp.RealTime;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Realtime
{
	/// <summary>
	/// Real-time notifier s? d?ng SignalR ?? g?i notifications
	/// </summary>
	public class SignalRRealTimeNotifier : IRealTimeNotifier, ITransientDependency
	{
		private readonly IHubContext<NotificationHub> _hubContext;

		/// <summary>
		/// Ch? s? d?ng notifier này khi ???c yêu c?u c? th?
		/// </summary>
		public bool UseOnlyIfRequestedAsTarget => false;

		public SignalRRealTimeNotifier(IHubContext<NotificationHub> hubContext)
		{
			_hubContext = hubContext;
		}

		/// <summary>
		/// G?i notification ??n user ?ang online
		/// </summary>
		public async Task SendNotificationsAsync(UserNotification[] userNotifications)
		{
			foreach (var userNotification in userNotifications)
			{
				await SendNotificationAsync(userNotification);
			}
		}

		/// <summary>
		/// G?i notification ??n m?t user c? th?
		/// </summary>
		private async Task SendNotificationAsync(UserNotification userNotification)
		{
			// T?o notification object ?? g?i
			var notificationData = new
			{
				id = userNotification.Id,
				notificationName = userNotification.Notification.NotificationName,
				data = userNotification.Notification.Data,
				severity = userNotification.Notification.Severity,
				creationTime = userNotification.Notification.CreationTime,
				state = userNotification.State
			};

			// G?i ??n t?t c? connections c?a user (n?u ?ang online)
			try
			{
				// S? d?ng User() method c?a SignalR ?? g?i theo UserId
				var userIdString = userNotification.UserId.ToString();
				await _hubContext.Clients.User(userIdString)
					.SendAsync("getNotification", notificationData);
			}
			catch (System.Exception ex)
			{
				// Log l?i nh?ng không throw ?? không ?nh h??ng ??n flow
				System.Diagnostics.Debug.WriteLine($"Error sending notification: {ex.Message}");
			}
		}
	}
}
