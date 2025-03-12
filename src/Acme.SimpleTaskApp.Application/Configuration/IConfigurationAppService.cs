using System.Threading.Tasks;
using Acme.SimpleTaskApp.Configuration.Dto;

namespace Acme.SimpleTaskApp.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
