using Abp.Dependency;
using Abp.Net.Mail;
using Abp.Notifications;
using Acme.SimpleTaskApp.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Email
{
	public class EmailRealTimeNotifier : IRealTimeNotifier, ITransientDependency
	{
		/// <summary>
		/// If true, this real time notifier will be used for sending real time notifications when it is requested. Otherwise it will not be used.
		/// <para>
		/// If false, this realtime notifier will notify any notifications.
		/// </para>
		/// </summary>
		//bool UseOnlyIfRequestedAsTarget => false;

		bool IRealTimeNotifier.UseOnlyIfRequestedAsTarget => throw new NotImplementedException();

		private readonly IEmailSender _emailSender;
		private readonly UserManager _userManager;

		public EmailRealTimeNotifier(
				IEmailSender emailSender,
				UserManager userManager)
		{
			_emailSender = emailSender;
			_userManager = userManager;
		}

		public async Task SendNotificationsAsync(UserNotification[] userNotifications)
		{
			foreach (var userNotification in userNotifications)
			{
				if (userNotification.Notification.Data is MessageNotificationData data)
				{
					var user = await _userManager.GetUserByIdAsync(userNotification.UserId);

					_emailSender.Send(
							to: user.EmailAddress,
							subject: "You have a new notification!",
							body: data.Message,
							isBodyHtml: true
					);
				}
			}
		}
	}
}
