using Abp.Application.Services.Dto;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.CartItems;
using Acme.SimpleTaskApp.CartItems.Dtos;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Carts.Dtos;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.Web.Models.Carts;
using Acme.SimpleTaskApp.Web.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class HomeCustomerController : SimpleTaskAppControllerBase
	{
		private readonly IProductAppService _productAppService;
		private readonly IWebHostEnvironment webHostEnvironment;
		private readonly ICategoryAppService _categoryAppService;
		private readonly ICartAppService _cartAppService;
		//private readonly ICartItemAppService _cartItemAppService;


		public HomeCustomerController(IProductAppService productAppService,
															ICategoryAppService categoryAppService,
															IWebHostEnvironment webHostEnvironment,
															ICartAppService cartAppService
															//ICartItemAppService cartItemAppService
															)
		{
			//_cartItemAppService = cartItemAppService;
			_cartAppService = cartAppService;
			_productAppService = productAppService;
			_categoryAppService = categoryAppService;
			this.webHostEnvironment = webHostEnvironment;
		}

		public async Task<ActionResult> Index(int page = 1, int page_size = 12)
		{
			var input = new GetAllProductsInput
			{
				MaxResultCount = page_size,
				SkipCount = (page - 1) * page_size
			};

			var output = await _productAppService.GetAllProducts(input);

			// Tính toán số trang  
			int totalProducts = output.TotalCount;
			int totalPages = (int)Math.Ceiling((double)totalProducts / page_size);

			var model = new ProductViewModel(output.Items)
			{
				TotalPages = totalPages,
				PageNumber = page
			};

			return View(model);
		}

		public async Task<IActionResult> DetailProductCusTomer(int productId)
		{
			var product = await _productAppService.GetByIdProducts(new EntityDto<int>(productId));

			var model = new DetailProductModalViewModel(product)
			{
			};

			return View(model);
		}

		// Trong HomeCustomerController.cs
		public async Task<ActionResult> SearchProductCustomer(GetAllProductsInput input)
		{
			try
			{
				var result = await _productAppService.SearchProducts(input);

				var model = new ProductViewModel(result.Items)
				{
				
				};

				return View(model);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message, ex);
				throw new UserFriendlyException("Có lỗi xảy ra khi tìm kiếm sản phẩm");
			}
		}

		// Thêm action cho form search
		public async Task<ActionResult> LoginMember()
		{
			return  View();
		}
		[Authorize]
		public async Task<ActionResult> Cart(GetCartInput input)
		{
			try
			{
				// Lấy userId từ session
				var userId = AbpSession.UserId;
				if (userId == null)
				{
					throw new UserFriendlyException("Vui lòng đăng nhập để xem giỏ hàng");
				}
				input.UserId = (long)userId;
				// Gọi service lấy thông tin giỏ hàng
				var cart = await _cartAppService.GetCart(input);
				// Ánh xạ sang ViewModel
				var viewModel = new CartViewModel
				{
					UserId = cart.UserId,
					Id = cart.Id,
					CreationTime = cart.CreationTime,
					CartItems = cart.CartItems
				};

				return View(viewModel);
			}
			catch (UserFriendlyException ex)
			{
				Logger.Error(ex.Message, ex);
				return RedirectToAction("LoginMember");
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message, ex);
				throw new UserFriendlyException("Lỗi khi tải giỏ hàng");
			}

		}
	}
}
