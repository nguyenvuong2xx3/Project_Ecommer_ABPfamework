using Microsoft.AspNetCore.Mvc;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class WebsiteController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
