using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Acme.SimpleTaskApp.Configuration.Dto;

namespace Acme.SimpleTaskApp.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : SimpleTaskAppAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
