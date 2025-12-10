using Abp.AspNetCore.Mvc.Authorization;
using Acme.SimpleTaskApp.Authorization;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Settings;
using Acme.SimpleTaskApp.Settings.Dtos;
using Acme.SimpleTaskApp.Web.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	[AbpMvcAuthorize(PermissionNames.Pages_Settings)]
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

		[AbpMvcAuthorize(PermissionNames.Pages_Settings_Edit)]
		[HttpPost]
		public async Task<IActionResult> UpdateStoreSettings([FromForm] StoreSettingDto input)
		{
			if (!ModelState.IsValid)
			{
				return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
			}

			try
			{
				await _settingAppService.UpdateStoreSettings(input);
				return Json(new { success = true, message = "Cập nhật thông tin cửa hàng thành công!" });
			}
			catch (System.Exception ex)
			{
				return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetStoreSettings()
		{
			var settings = await _settingAppService.GetStoreSettings();
			return Json(settings);
		}
	}
}
