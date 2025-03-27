using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class CartsController : Controller
	{
		public async Task<ActionResult> AddCart(int productId)
		{



			return Ok();
		}
	}
}
