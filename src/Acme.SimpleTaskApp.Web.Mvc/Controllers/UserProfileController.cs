using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Web.Models.UserProfiles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class UserProfileController : SimpleTaskAppControllerBase
	{
		private readonly UserManager<User> _userManager;
		private readonly ILocationAppService _locationAppService;
		public UserProfileController(UserManager<User> userManager, ILocationAppService locationAppService)
		{
			_locationAppService = locationAppService;
			_userManager = userManager;
		}
		public async Task<IActionResult> Index()
		{
			var user = await _userManager.FindByIdAsync(AbpSession.UserId.ToString());
			var getDiaChinh = await _locationAppService.GetAllDonViHanhChinh();
			if (user.TinhThanh != null && user.PhuongXa != null)
			{
				var tinhthanh = getDiaChinh.FirstOrDefault(x => x.MatinhTMS == user.TinhThanh);
				var tenTinhThanh = tinhthanh.Tentinhmoi;
				var tenPhuongXa = tinhthanh.Phuongxa.FirstOrDefault(x => x.Maphuongxa == user.PhuongXa).Tenphuongxa;
				var model = new UserProfileViewModel
				{
					User = user,
					TenTinhThanh = tenTinhThanh,
					TenPhuongXa = tenPhuongXa
				};
				return View(model);
			}
			else
			{
				var model = new UserProfileViewModel
				{
					User = user,
				};
				return View(model);
			}
		}
	}
}
