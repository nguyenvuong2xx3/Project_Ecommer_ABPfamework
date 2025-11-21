using Abp.Domain.Repositories;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.HomeCustomers;
using Acme.SimpleTaskApp.HomeCustomers.Dtos;
using Acme.SimpleTaskApp.Identity;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Web.Models.Carts;
using Acme.SimpleTaskApp.Web.Models.HomeCustomers;
using Acme.SimpleTaskApp.Web.Models.Products;
using Acme.SimpleTaskApp.Web.Models.UserProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	namespace Acme.SimpleTaskApp.Web.Controllers
	{
		public class HomeCustomerController : SimpleTaskAppControllerBase
		{
			private readonly IRepository<Category> _categoryRepository;
			private readonly IProductAppService _productAppService;
			private readonly ICategoryAppService _categoryAppService;
			private readonly ICartAppService _cartAppService;
			private readonly IHomeCustomerAppService _homeCustomerAppService;
			private readonly SignInManager _signInManager;
			private readonly UserManager _userManager;
			private readonly ILocationAppService _locationAppService;

			public HomeCustomerController(IProductAppService productAppService,
				IRepository<Category> categoryRepository,
			IHomeCustomerAppService homeCustomerAppService,
																SignInManager signInManager,
																ICategoryAppService categoryAppService,
																ICartAppService cartAppService,
																UserManager userManager,
																ILocationAppService locationAppService
																)
			{
				_locationAppService = locationAppService;
				_userManager = userManager;
				_homeCustomerAppService = homeCustomerAppService;
				_categoryRepository = categoryRepository;
				_cartAppService = cartAppService;
				_signInManager = signInManager;
				_productAppService = productAppService;
				_categoryAppService = categoryAppService;
			}
			public async Task<ActionResult> SignOut()
			{
				await _signInManager.SignOutAsync();
				return RedirectToAction("Index");
			}
			public async Task<ActionResult> Index(SearchHomeCustomerDto input)
			{
				var output = await _homeCustomerAppService.GetAllProductHomeCustomers(input);

				var model = new HomeCustomerViewModel()
				{
					ProductsInfo = output.Items.ToList(),
				};

				return View(model);
			}

			public async Task<IActionResult> DetailProductCustomer(int id)
			{
				var product = await _homeCustomerAppService.GetProductById(id);

				var model = new HomeCustomerViewModel()
				{
					ProductInfo = product
				};

				return View(model);
			}

			// Trong HomeCustomerController.cs
			public async Task<ActionResult> SearchProductCustomer(SearchHomeCustomerDto input)
			{
				var output = await _homeCustomerAppService.GetAllProductHomeCustomers(input);
				var cattegory = await _categoryRepository.GetAllAsync();
				var model = new HomeCustomerViewModel()
				{
					ProductsInfo = output.Items.ToList(),
					Filter = input.Filter,
					Categories = cattegory.ToList()
				};

				return View(model);
			}
			public async Task<PartialViewResult> FilterAdvancedModal()
			{

				return PartialView("_FilterAdvancedModal");
			}

			//// Thêm action cho form search
			//public async Task<ActionResult> LoginMember()
			//{
			//	return View();
			//}

			[Authorize]
			public async Task<ActionResult> Cart()
			{
				// Kiểm tra user đã đăng nhập chưa
				if (!AbpSession.UserId.HasValue)
				{
					// Nếu chưa đăng nhập → trả về view có message
					ViewBag.Message = "Vui lòng đăng nhập để xem giỏ hàng.";
					return View("Cart"); // hoặc View("Cart") nếu bạn muốn hiển thị chung
				}
				var user = _userManager.GetUserById(AbpSession.UserId.Value);
				var cart = await _cartAppService.GetCart();
				var viewModel = new CartViewModel {
					CartItems = cart.CartItems,
					User = user,
					TinhThanh = null,
					PhuongXa = null,
				}; 
				var getDiaChinh = await _locationAppService.GetAllDonViHanhChinh();
				if (user.TinhThanh != null && user.PhuongXa != null)
				{
					var tinhthanh = getDiaChinh.FirstOrDefault(x => x.MatinhTMS == user.TinhThanh);
					var tenTinhThanh = tinhthanh.Tentinhmoi;
					var tenPhuongXa = tinhthanh.Phuongxa.FirstOrDefault(x => x.Maphuongxa == user.PhuongXa).Tenphuongxa;

					// Gọi service lấy thông tin giỏ hàng
					// Ánh xạ sang ViewModel
					viewModel.TinhThanh = tenTinhThanh;
					viewModel.PhuongXa = tenPhuongXa;
				}
				return View(viewModel);
			}
		}
	}
}