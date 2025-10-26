using Acme.SimpleTaskApp.Configuration;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Settings;
using Acme.SimpleTaskApp.Web.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class SettingsController : SimpleTaskAppControllerBase
	{
		private readonly ISettingAppService _settingAppService;
		public SettingsController(ISettingAppService settingAppService)
		{
			_settingAppService = settingAppService;
		}
		public async Task<IActionResult> Index()
		{
			var getAll = await _settingAppService.GetAllSetting();
			var model = new MailSettingViewModel
			{
				GetAllSetting = getAll
			};
			return View(model);
		}
	}
}
