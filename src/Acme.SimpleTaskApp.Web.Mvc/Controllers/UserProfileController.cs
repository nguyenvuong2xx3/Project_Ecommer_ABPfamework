using Acme.SimpleTaskApp.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	[Route("thongtincanhan")]
	public class UserProfileController : SimpleTaskAppControllerBase
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
