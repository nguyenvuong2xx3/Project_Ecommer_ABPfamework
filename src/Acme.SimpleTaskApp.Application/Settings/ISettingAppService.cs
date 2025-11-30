using Abp.Application.Services;
using Acme.SimpleTaskApp.Settings.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Settings
{
	public interface ISettingAppService : IApplicationService
	{
		Task<GetAllSettingDto> GetAllSetting();
		Task<MailSettingDto> MailSettings();
		Task<StoreSettingDto> GetStoreSettings();
		Task UpdateStoreSettings(StoreSettingDto input);
	}
}
