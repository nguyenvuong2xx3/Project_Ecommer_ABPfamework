//using Abp;
//using Abp.Application.Services;
//using Abp.Domain.Services;
//using Abp.Localization;
//using Abp.Notifications;
//using Acme.SimpleTaskApp.Authorization.Users;
//using Acme.SimpleTaskApp.MultiTenancy;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Acme.SimpleTaskApp.Noti
//{
//	public class AppNotifierAppService : SimpleTaskAppAppServiceBase, IAppNotifierAppService 
//	{
//		private readonly INotificationPublisher _notificationPublisher;

//		public AppNotifierAppService(INotificationPublisher notificationPublisher)
//		{
//			_notificationPublisher = notificationPublisher;
//		}

//		public async Task WelcomeToTheApplicationAsync(User user)
//		{
//			await _notificationPublisher.PublishAsync("App.WelcomeToTheApplication", new MessageNotificationData(L("WelcomeToTheApplicationNotificationMessage")), null, NotificationSeverity.Success, new UserIdentifier[1] { user.ToUserIdentifier() });
//		}

//		public async Task NewUserRegisteredAsync(User user)
//		{
//			LocalizableMessageNotificationData notificationData = new LocalizableMessageNotificationData(new LocalizableString("NewUserRegisteredNotificationMessage", "SaaS"))
//			{
//				["userName"] = user.UserName,
//				["emailAddress"] = user.EmailAddress
//			};
//			await _notificationPublisher.PublishAsync("App.NewUserRegistered", notificationData, null, NotificationSeverity.Info, null, null, new int?[1] { user.TenantId });
//		}

//		public async Task NewTenantRegisteredAsync(Tenant tenant)
//		{
//			LocalizableMessageNotificationData notificationData = new LocalizableMessageNotificationData(new LocalizableString("NewTenantRegisteredNotificationMessage", "SaaS")) { ["tenancyName"] = tenant.TenancyName };
//			await _notificationPublisher.PublishAsync("App.NewTenantRegistered", notificationData);
//		}

//		public async Task GdprDataPrepared(UserIdentifier user, Guid binaryObjectId)
//		{
//			LocalizableMessageNotificationData notificationData = new LocalizableMessageNotificationData(new LocalizableString("GdprDataPreparedNotificationMessage", "SaaS")) { ["binaryObjectId"] = binaryObjectId };
//			await _notificationPublisher.PublishAsync("App.GdprDataPrepared", notificationData, null, NotificationSeverity.Info, new UserIdentifier[1] { user });
//		}

//		public async Task SendMessageAsync(UserIdentifier user, string message, NotificationSeverity severity = NotificationSeverity.Info)
//		{
//			await _notificationPublisher.PublishAsync("App.SimpleMessage", new MessageNotificationData(message), null, severity, new UserIdentifier[1] { user });
//		}

//		//public async Task SendMessageAsync(string notificationName, string message, UserIdentifier[] userIds = null, NotificationSeverity severity = NotificationSeverity.Info)
//		//{
//		//	_ = NotificationPublisher.AllTenants;
//		//	await _notificationPublisher.PublishAsync(notificationName, new MessageNotificationData(message), null, severity, userIds);
//		//}

//		public Task SendMessageAsync(UserIdentifier user, LocalizableString localizableMessage, IDictionary<string, object> localizableMessageData = null, NotificationSeverity severity = NotificationSeverity.Info)
//		{
//			return SendNotificationAsync("App.SimpleMessage", user, localizableMessage, localizableMessageData, severity);
//		}

//		protected async Task SendNotificationAsync(string notificationName, UserIdentifier user, LocalizableString localizableMessage, IDictionary<string, object> localizableMessageData = null, NotificationSeverity severity = NotificationSeverity.Info)
//		{
//			LocalizableMessageNotificationData notificationData = new LocalizableMessageNotificationData(localizableMessage);
//			if (localizableMessageData != null)
//			{
//				foreach (KeyValuePair<string, object> pair in localizableMessageData)
//				{
//					notificationData[pair.Key] = pair.Value;
//				}
//			}
//			await _notificationPublisher.PublishAsync(notificationName, notificationData, null, severity, new UserIdentifier[1] { user });
//		}

//		public Task TenantsMovedToEdition(UserIdentifier user, string sourceEditionName, string targetEditionName)
//		{
//			return SendNotificationAsync("App.TenantsMovedToEdition", user, new LocalizableString("TenantsMovedToEditionNotificationMessage", "SaaS"), new Dictionary<string, object>
//		{
//			{ "sourceEditionName", sourceEditionName },
//			{ "targetEditionName", targetEditionName }
//		});
//		}

//		public Task<TResult> TenantsMovedToEdition<TResult>(UserIdentifier argsUser, int sourceEditionId, int targetEditionId)
//		{
//			throw new NotImplementedException();
//		}

//		public Task SomeUsersCouldntBeImported(UserIdentifier user, string fileToken, string fileType, string fileName)
//		{
//			return SendNotificationAsync("App.DownloadInvalidImportUsers", user, new LocalizableString("ClickToSeeInvalidUsers", "SaaS"), new Dictionary<string, object>
//		{
//			{ "fileToken", fileToken },
//			{ "fileType", fileType },
//			{ "fileName", fileName }
//		});
//		}

//		public async Task SendMassNotificationAsync(string message, UserIdentifier[] userIds = null, NotificationSeverity severity = NotificationSeverity.Info, Type[] targetNotifiers = null)
//		{
//			await _notificationPublisher.PublishAsync("App.MassNotification", new MessageNotificationData(message), null, severity, userIds, null, null, targetNotifiers);
//		}

//		public async Task SendMessageAsync(SendMessageInput input)
//		{
//			// Sử dụng các thuộc tính từ input
//			await _notificationPublisher.PublishAsync(
//					input.NotificationName,
//					new MessageNotificationData(input.Message),
//					null,
//					input.Severity,
//					input.UserIds
//			);
//		}
//	}
//}
