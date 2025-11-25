//using Abp;
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
//	public interface IAppNotifierAppService
//	{
//		Task WelcomeToTheApplicationAsync(User user);

//		Task NewUserRegisteredAsync(User user);

//		Task NewTenantRegisteredAsync(Tenant tenant);

//		Task GdprDataPrepared(UserIdentifier user, Guid binaryObjectId);

//		Task SendMessageAsync(UserIdentifier user, string message, NotificationSeverity severity = NotificationSeverity.Info);

//		Task SendMessageAsync(SendMessageInput input);

//		Task SendMessageAsync(UserIdentifier user, LocalizableString localizableMessage, IDictionary<string, object> localizableMessageData = null, NotificationSeverity severity = NotificationSeverity.Info);

//		Task TenantsMovedToEdition(UserIdentifier user, string sourceEditionName, string targetEditionName);

//		Task SomeUsersCouldntBeImported(UserIdentifier user, string fileToken, string fileType, string fileName);

//		Task SendMassNotificationAsync(string message, UserIdentifier[] userIds = null, NotificationSeverity severity = NotificationSeverity.Info, Type[] targetNotifiers = null);
//	}

//	public class SendMessageInput
//	{
//		public string NotificationName { get; set; }
//		public string Message { get; set; }
//		public UserIdentifier[] UserIds { get; set; }
//		public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;
//	}
//}
