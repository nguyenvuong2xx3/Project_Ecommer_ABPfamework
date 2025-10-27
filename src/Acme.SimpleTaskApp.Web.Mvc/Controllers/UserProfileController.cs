using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Web.Models.UserProfiles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	[Route("thongtincanhan")]
	public class UserProfileController : SimpleTaskAppControllerBase
	{
		private readonly UserManager<User> _userManager;
		public UserProfileController(UserManager<User> userManager)
		{
			_userManager = userManager;
		}
		public async Task<IActionResult> Index()
		{
			var user = await _userManager.FindByIdAsync(AbpSession.UserId.ToString());
			var model = new UserProfileViewModel
			{
				User = user
			};
			return View(model);
		}
	}
}
