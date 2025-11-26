using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.AppTrezoRecentNotifications
{
	public class AppTrezoRecentNotificationsViewComponent : SimpleTaskAppViewComponent
	{
		public Task<IViewComponentResult> InvokeAsync(string cssClass, string iconClass = "flaticon-alert-2 unread-notification fs-2")
		{
			if (!HttpContext.User.Identity.IsAuthenticated)
			{
				return Task.FromResult<IViewComponentResult>(Content("")); // Không render gì cả
			}

			var model = new RecentNotificationsViewModel
			{
				CssClass = cssClass,
				IconClass = iconClass
			};

			return Task.FromResult<IViewComponentResult>(View(model));
		}
	}
}
