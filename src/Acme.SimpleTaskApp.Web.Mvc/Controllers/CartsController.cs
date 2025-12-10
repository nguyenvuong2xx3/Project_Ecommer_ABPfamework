using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Web.Models.Orders;
using Acme.SimpleTaskApp.Web.Models.UserProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Acme.SimpleTaskApp.Authorization;

namespace Acme.SimpleTaskApp.Web.Controllers
{
		[Authorize]
		public class CartsController : SimpleTaskAppControllerBase
		{
			private readonly ICartAppService _cartAppService;
			private readonly UserManager<User> _userManager;
			private readonly ILocationAppService _locationAppService;

			public CartsController(ICartAppService cartAppService, UserManager<User> userManager, ILocationAppService locationAppService)
			{
				_cartAppService = cartAppService;
				_userManager = userManager;
				_locationAppService = locationAppService;
			}

			[AbpMvcAuthorize(PermissionNames.Pages_Carts_AddItem)]
			public async Task<ActionResult> AddCart(int productId, int quantity)
			{
				await _cartAppService.CreateCart(productId, quantity);
				return Json(new { success = true });
			}

			[AbpMvcAuthorize(PermissionNames.Pages_Carts_View)]
			public async Task<ActionResult> OrderInfoModal()
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
					return PartialView("_OrderInfoModal", model);
				}
				else
				{
					var model = new UserProfileViewModel
					{
						User = user,
					};
					return PartialView("_OrderInfoModal", model);
				}
			}
		}
}
