using Abp.AspNetCore.Mvc.Authorization;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Notifications;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acme.SimpleTaskApp.Authorization;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	[AbpMvcAuthorize(PermissionNames.Pages_Notifications)]
	public class NotificationsController : SimpleTaskAppControllerBase
	{
		private readonly INotificationAppService _notificationAppService;

		public NotificationsController(
				INotificationAppService notificationAppService)
		{
			_notificationAppService = notificationAppService;
		}

		public ActionResult Index()
		{
			return View();
		}

		//public async Task<PartialViewResult> SettingsModal()
		//{
		//	var notificationSettings = await _notificationAppService.GetNotificationSettings();
		//	return PartialView("_SettingsModal", notificationSettings);
		//}

		//public PartialViewResult CreateMassNotificationModal()
		//{
		//	var viewModel = new CreateMassNotificationViewModel
		//	{
		//		TargetNotifiers = _notificationAppService.GetAllNotifiers()
		//	};

		//	return PartialView("_CreateMassNotificationModal", viewModel);
		//}

		public PartialViewResult UserLookupTableModal()
		{
			return PartialView("_UserLookupTableModal");
		}


		public ActionResult MassNotifications()
		{
			return View();
		}
	}
}
