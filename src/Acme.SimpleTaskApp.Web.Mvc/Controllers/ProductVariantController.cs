using Microsoft.AspNetCore.Mvc;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class ProductVariantController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

	}
}
