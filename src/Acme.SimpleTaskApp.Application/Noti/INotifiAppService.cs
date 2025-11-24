// BBK.SaaS.Application.Shared, Version=9.1.0.0, Culture=neutral, PublicKeyToken=null
// BBK.SaaS.Notifications.INotificationAppService
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Acme.SimpleTaskApp.Notifications.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Noti
{
public interface INotifiAppService : IApplicationService, ITransientDependency
{
	Task<GetNotificationsOutput> GetUserNotifications(GetUserNotificationsInput input);

	Task<SetNotificationAsReadOutput> SetAllAvailableVersionNotificationAsRead();

	Task SetAllNotificationsAsRead();

	Task<SetNotificationAsReadOutput> SetNotificationAsRead(EntityDto<Guid> input);

	Task<GetNotificationSettingsOutput> GetNotificationSettings();

	Task UpdateNotificationSettings(UpdateNotificationSettingsInput input);

	Task DeleteNotification(EntityDto<Guid> input);

	Task DeleteAllUserNotifications(DeleteAllUserNotificationsInput input);

	//Task CreateMassNotification(CreateMassNotificationInput input);

	//Task CreateNewVersionReleasedNotification();

	Task<bool> ShouldUserUpdateApp();

	List<string> GetAllNotifiers();

	Task<GetPublishedNotificationsOutput> GetNotificationsPublishedByUser(GetPublishedNotificationsInput input);
}
}