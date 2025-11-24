// BBK.SaaS.Application, Version=9.1.0.0, Culture=neutral, PublicKeyToken=null
// BBK.SaaS.Notifications.NotificationAppService
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Organizations;
using Abp.Runtime.Session;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Notifications;
using Acme.SimpleTaskApp.Notifications.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Acme.SimpleTaskApp.Noti
{
[AbpAuthorize(new string[] { })]
public class NotifiAppService : ApplicationService, INotifiAppService, IApplicationService, ITransientDependency
{
	private readonly INotificationDefinitionManager _notificationDefinitionManager;

	private readonly IUserNotificationManager _userNotificationManager;

	private readonly INotificationSubscriptionManager _notificationSubscriptionManager;

	private readonly IRepository<User, long> _userRepository;

	private readonly INotificationConfiguration _notificationConfiguration;

	private readonly INotificationStore _notificationStore;

	private readonly IBackgroundJobManager _backgroundJobManager;

	private readonly IRepository<UserNotificationInfo, Guid> _userNotificationRepository;

	public NotifiAppService(INotificationDefinitionManager notificationDefinitionManager, IUserNotificationManager userNotificationManager, INotificationSubscriptionManager notificationSubscriptionManager, IRepository<User, long> userRepository, INotificationConfiguration notificationConfiguration, INotificationStore notificationStore, IBackgroundJobManager backgroundJobManager, IRepository<UserNotificationInfo, Guid> userNotificationRepository)
	{
		_notificationDefinitionManager = notificationDefinitionManager;
		_userNotificationManager = userNotificationManager;
		_notificationSubscriptionManager = notificationSubscriptionManager;
		_userRepository = userRepository;
		_notificationConfiguration = notificationConfiguration;
		_notificationStore = notificationStore;
		_backgroundJobManager = backgroundJobManager;
		_userNotificationRepository = userNotificationRepository;
	}

	[DisableAuditing]
	public async Task<GetNotificationsOutput> GetUserNotifications(GetUserNotificationsInput input)
	{
		return new GetNotificationsOutput(await _userNotificationManager.GetUserNotificationCountAsync(base.AbpSession.ToUserIdentifier(), input.State, input.StartDate, input.EndDate), await _userNotificationManager.GetUserNotificationCountAsync(base.AbpSession.ToUserIdentifier(), UserNotificationState.Unread, input.StartDate, input.EndDate), await _userNotificationManager.GetUserNotificationsAsync(base.AbpSession.ToUserIdentifier(), input.State, input.SkipCount, input.MaxResultCount, input.StartDate, input.EndDate));
	}

	public async Task<bool> ShouldUserUpdateApp()
	{
		return (await _userNotificationManager.GetUserNotificationsAsync(base.AbpSession.ToUserIdentifier(), UserNotificationState.Unread, 0, int.MaxValue, null, null)).Any((UserNotification x) => x.Notification.NotificationName == "App.NewVersionAvailable");
	}

	public async Task<SetNotificationAsReadOutput> SetAllAvailableVersionNotificationAsRead()
	{
		List<UserNotification> filteredNotifications = (await _userNotificationManager.GetUserNotificationsAsync(base.AbpSession.ToUserIdentifier(), UserNotificationState.Unread, 0, int.MaxValue, null, null)).Where((UserNotification x) => x.Notification.NotificationName == "App.NewVersionAvailable").ToList();
		if (!filteredNotifications.Any())
		{
			return new SetNotificationAsReadOutput(success: false);
		}
		foreach (UserNotification notification in filteredNotifications)
		{
			if (notification.State != UserNotificationState.Read)
			{
				await _userNotificationManager.UpdateUserNotificationStateAsync(notification.TenantId, notification.Id, UserNotificationState.Read);
			}
		}
		return new SetNotificationAsReadOutput(success: true);
	}

	public async Task SetAllNotificationsAsRead()
	{
		await _userNotificationManager.UpdateAllUserNotificationStatesAsync(base.AbpSession.ToUserIdentifier(), UserNotificationState.Read);
	}

	public async Task<SetNotificationAsReadOutput> SetNotificationAsRead(EntityDto<Guid> input)
	{
		UserNotification userNotification = await _userNotificationManager.GetUserNotificationAsync(base.AbpSession.TenantId, input.Id);
		if (userNotification == null)
		{
			return new SetNotificationAsReadOutput(success: false);
		}
		if (userNotification.UserId != base.AbpSession.GetUserId())
		{
			throw new Exception($"Given user notification id ({input.Id}) is not belong to the current user ({base.AbpSession.GetUserId()})");
		}
		if (userNotification.State == UserNotificationState.Read)
		{
			return new SetNotificationAsReadOutput(success: false);
		}
		await _userNotificationManager.UpdateUserNotificationStateAsync(base.AbpSession.TenantId, input.Id, UserNotificationState.Read);
		return new SetNotificationAsReadOutput(success: true);
	}

	public async Task<GetNotificationSettingsOutput> GetNotificationSettings()
	{
		GetNotificationSettingsOutput output = new GetNotificationSettingsOutput();
		GetNotificationSettingsOutput getNotificationSettingsOutput = output;
		getNotificationSettingsOutput.ReceiveNotifications = await base.SettingManager.GetSettingValueAsync<bool>("Abp.Notifications.ReceiveNotifications");
		IEnumerable<NotificationDefinition> notificationDefinitions = (await _notificationDefinitionManager.GetAllAvailableAsync(base.AbpSession.ToUserIdentifier())).Where((NotificationDefinition nd) => nd.EntityType == null);
		output.Notifications = base.ObjectMapper.Map<List<NotificationSubscriptionWithDisplayNameDto>>(notificationDefinitions);
		List<string> subscribedNotifications = (await _notificationSubscriptionManager.GetSubscribedNotificationsAsync(base.AbpSession.ToUserIdentifier())).Select((NotificationSubscription ns) => ns.NotificationName).ToList();
		output.Notifications.ForEach(delegate (NotificationSubscriptionWithDisplayNameDto n)
		{
			n.IsSubscribed = subscribedNotifications.Contains(n.Name);
		});
		return output;
	}

	public async Task UpdateNotificationSettings(UpdateNotificationSettingsInput input)
	{
		await base.SettingManager.ChangeSettingForUserAsync(base.AbpSession.ToUserIdentifier(), "Abp.Notifications.ReceiveNotifications", input.ReceiveNotifications.ToString());
		foreach (NotificationSubscriptionDto notification in input.Notifications)
		{
			if (notification.IsSubscribed)
			{
				await _notificationSubscriptionManager.SubscribeAsync(base.AbpSession.ToUserIdentifier(), notification.Name);
			}
			else
			{
				await _notificationSubscriptionManager.UnsubscribeAsync(base.AbpSession.ToUserIdentifier(), notification.Name);
			}
		}
	}

	public async Task DeleteNotification(EntityDto<Guid> input)
	{
		UserNotification notification = await _userNotificationManager.GetUserNotificationAsync(base.AbpSession.TenantId, input.Id);
		if (notification != null)
		{
			if (notification.UserId != base.AbpSession.GetUserId())
			{
				throw new UserFriendlyException(L("ThisNotificationDoesntBelongToYou"));
			}
			await _userNotificationManager.DeleteUserNotificationAsync(base.AbpSession.TenantId, input.Id);
		}
	}

	public async Task DeleteAllUserNotifications(DeleteAllUserNotificationsInput input)
	{
		await _userNotificationManager.DeleteAllUserNotificationsAsync(base.AbpSession.ToUserIdentifier(), input.State, input.StartDate, input.EndDate);
	}

	[AbpAuthorize(new string[] { "Pages.Administration.MassNotification" })]
	public async Task<PagedResultDto<MassNotificationUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
	{
		IQueryable<User> query = _userRepository.GetAll().WhereIf(!string.IsNullOrWhiteSpace(input.Filter), (User e) => (e.Name != null && e.Name.Contains(input.Filter)) || (e.Surname != null && e.Surname.Contains(input.Filter)) || (e.EmailAddress != null && e.EmailAddress.Contains(input.Filter)));
		int totalCount = await query.CountAsync();
		List<User> userList = await query.PageBy(input).ToListAsync();
		List<MassNotificationUserLookupTableDto> lookupTableDtoList = new List<MassNotificationUserLookupTableDto>();
		foreach (User user in userList)
		{
			lookupTableDtoList.Add(new MassNotificationUserLookupTableDto
			{
				Id = user.Id,
				DisplayName = user.Name + " " + user.Surname + " (" + user.EmailAddress + ")"
			});
		}
		return new PagedResultDto<MassNotificationUserLookupTableDto>(totalCount, lookupTableDtoList);
	}

	

	//[AbpAuthorize(new string[] { "Pages.Administration.MassNotification.Create" })]
	//public async Task CreateMassNotification(CreateMassNotificationInput input)
	//{
	//	if (input.TargetNotifiers.IsNullOrEmpty())
	//	{
	//		throw new UserFriendlyException(L("MassNotificationTargetNotifiersFieldIsRequiredMessage"));
	//	}
	//	List<UserIdentifier> userIds = new List<UserIdentifier>();
	//	if (!input.UserIds.IsNullOrEmpty())
	//	{
	//		userIds.AddRange(input.UserIds.Select((long i) => new UserIdentifier(base.AbpSession.TenantId, i)));
	//	}
	//	if (!input.OrganizationUnitIds.IsNullOrEmpty())
	//	{
	//		List<UserIdentifier> list = userIds;
	//		list.AddRange(await _userOrganizationUnitRepository.GetAllUsersInOrganizationUnitHierarchical(input.OrganizationUnitIds));
	//	}
	//	if (userIds.Count == 0)
	//	{
	//		if (input.OrganizationUnitIds.IsNullOrEmpty())
	//		{
	//			throw new UserFriendlyException(L("MassNotificationNoUsersFoundInOrganizationUnitMessage"));
	//		}
	//		throw new UserFriendlyException(L("MassNotificationUserOrOrganizationUnitFieldIsRequiredMessage"));
	//	}
	//	List<Type> targetNotifiers = new List<Type>();
	//	foreach (Type notifier in _notificationConfiguration.Notifiers)
	//	{
	//		if (input.TargetNotifiers.Contains<string>(notifier.FullName))
	//		{
	//			targetNotifiers.Add(notifier);
	//		}
	//	}
	//	await _appNotifier.SendMassNotificationAsync(input.Message, userIds.DistinctBy((UserIdentifier u) => u.UserId).ToArray(), input.Severity, targetNotifiers.ToArray());
	//}

	//[AbpAuthorize(new string[] { "Pages_Administration_NewVersion_Create" })]
	//public async Task CreateNewVersionReleasedNotification()
	//{
	//	SendNotificationToAllUsersArgs args = new SendNotificationToAllUsersArgs
	//	{
	//		NotificationName = "App.NewVersionAvailable",
	//		Message = L("NewVersionAvailableNotificationMessage")
	//	};
	//	await _backgroundJobManager.EnqueueAsync<SendNotificationToAllUsersBackgroundJob, SendNotificationToAllUsersArgs>(args, BackgroundJobPriority.Normal, null);
	//}

	public List<string> GetAllNotifiers()
	{
		return _notificationConfiguration.Notifiers.Select((Type n) => n.FullName).ToList();
	}

	[AbpAuthorize(new string[] { "Pages.Administration.MassNotification" })]
	public async Task<GetPublishedNotificationsOutput> GetNotificationsPublishedByUser(GetPublishedNotificationsInput input)
	{
		return new GetPublishedNotificationsOutput(await _notificationStore.GetNotificationsPublishedByUserAsync(base.AbpSession.ToUserIdentifier(), "App.MassNotification", input.StartDate, input.EndDate));
	}
}
}