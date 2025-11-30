using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Authorization;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	[AbpMvcAuthorize(PermissionNames.Pages_Dashboard)]
	public class HomeController : SimpleTaskAppControllerBase
	{
		public ActionResult Index()
		{
			return View();
		}
		public ActionResult LowProductVariantModal()
		{
			return PartialView("_LowProductVariantModal");
		}
	}
}
