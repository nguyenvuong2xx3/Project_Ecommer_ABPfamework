using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.HomeCustomers;
using Acme.SimpleTaskApp.HomeCustomers.Dtos;
using Acme.SimpleTaskApp.Identity;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Web.Models.HomeCustomers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	namespace Acme.SimpleTaskApp.Web.Controllers
	{
		public class HomeCustomerController : SimpleTaskAppControllerBase
		{
			private readonly IProductAppService _productAppService;
			private readonly ICategoryAppService _categoryAppService;
			//private readonly ICartAppService _cartAppService;
			private readonly IHomeCustomerAppService _homeCustomerAppService;
			private readonly SignInManager _signInManager;


			//private readonly ICartItemAppService _cartItemAppService;


			public HomeCustomerController(IProductAppService productAppService,
																IHomeCustomerAppService homeCustomerAppService,
																SignInManager signInManager,
																ICategoryAppService categoryAppService
																//ICartAppService cartAppService
																//ICartItemAppService cartItemAppService
																)
			{
				_homeCustomerAppService = homeCustomerAppService;
				//_cartItemAppService = cartItemAppService;
				//_cartAppService = cartAppService;
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

			//public async Task<IActionResult> DetailProductCusTomer(int productId)
			//{
			//	var product = await _productAppService.GetByIdProducts(new EntityDto<int>(productId));

			//	var model = new DetailProductModalViewModel(product)
			//	{
			//	};

			//	return View(model);
			//}

			// Trong HomeCustomerController.cs
			public async Task<ActionResult> SearchProductCustomer(SearchHomeCustomerDto input)
			{
				var output = await _homeCustomerAppService.GetAllProductHomeCustomers(input);

				var model = new HomeCustomerViewModel()
				{
					ProductsInfo = output.Items.ToList(),
					Filter = input.Filter,
					//CategoryName = input.
				};

				return View(model);
			}


			//// Thêm action cho form search
			//public async Task<ActionResult> LoginMember()
			//{
			//	return View();
			//}
			////[Authorize]
			//public async Task<ActionResult> Cart(GetCartInput input)
			//{
			//	// Kiểm tra user đã đăng nhập chưa
			//	if (!AbpSession.UserId.HasValue)
			//	{
			//		// Nếu chưa đăng nhập → trả về view có message
			//		ViewBag.Message = "Vui lòng đăng nhập để xem giỏ hàng.";
			//		return View("Cart"); // hoặc View("Cart") nếu bạn muốn hiển thị chung
			//	}

			//	// Có userId
			//	input.UserId = AbpSession.UserId.Value;

			//	// Gọi service lấy thông tin giỏ hàng
			//	var cart = await _cartAppService.GetCart(input);

			//	// Ánh xạ sang ViewModel
			//	var viewModel = new CartViewModel
			//	{
			//		UserId = cart.UserId,
			//		Id = cart.Id,
			//		CreationTime = cart.CreationTime,
			//		CartItems = cart.CartItems
			//	};

			//	return View(viewModel);
			//}

		}
	}
}