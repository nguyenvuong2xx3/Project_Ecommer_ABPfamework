using Abp.Configuration.Startup;
using Abp.Runtime.Session;
using Acme.SimpleTaskApp.Sessions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.User
{
	public class UserViewComponent : ViewComponent
	{
		private readonly ISessionAppService _sessionAppService;
		private readonly IMultiTenancyConfig _multiTenancyConfig;

		public UserViewComponent(ISessionAppService sessionAppService, IMultiTenancyConfig multiTenancyConfig )
		{
			_sessionAppService = sessionAppService;
			_multiTenancyConfig = multiTenancyConfig;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var model = new UserViewModel
			{
				LoginInformations = await _sessionAppService.GetCurrentLoginInformations(),
				IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled,
			};

			return View(model);
		}
	}
}
