using Abp.Domain.Repositories;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.HomeCustomers;
using Acme.SimpleTaskApp.HomeCustomers.Dtos;
using Acme.SimpleTaskApp.Identity;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.ProductComments;
using Acme.SimpleTaskApp.ProductRatings;
using Acme.SimpleTaskApp.ProductRatings.Dtos;
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
using Abp.Notifications;
using Acme.SimpleTaskApp.ProductComments.Dtos;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	namespace Acme.SimpleTaskApp.Web.Controllers
	{
		[AllowAnonymous]
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
			private readonly IProductCommentAppService _productCommentAppService;
			private readonly IProductRatingAppService _productRatingAppService;
			private readonly INotificationPublisher _notificationPublisher;

			public HomeCustomerController(
				IProductAppService productAppService,
				IRepository<Category> categoryRepository,
				IHomeCustomerAppService homeCustomerAppService,
				SignInManager signInManager,
				ICategoryAppService categoryAppService,
				ICartAppService cartAppService,
				UserManager userManager,
				ILocationAppService locationAppService,
				IProductCommentAppService productCommentAppService,
				IProductRatingAppService productRatingAppService,
				INotificationPublisher notificationPublisher)
			{
				_locationAppService = locationAppService;
				_userManager = userManager;
				_homeCustomerAppService = homeCustomerAppService;
				_categoryRepository = categoryRepository;
				_cartAppService = cartAppService;
				_signInManager = signInManager;
				_productAppService = productAppService;
				_categoryAppService = categoryAppService;
				_productCommentAppService = productCommentAppService;
				_productRatingAppService = productRatingAppService;
				_notificationPublisher = notificationPublisher;
			}
			
			public async Task<ActionResult> SignOut()
			{
				await _signInManager.SignOutAsync();
				return RedirectToAction("Index");
			}
			
			public async Task<ActionResult> Index(SearchHomeCustomerDto input)
			{
				// Set default page size
				if (input.MaxResultCount <= 0)
					input.MaxResultCount = 12;
				
				// Calculate skip count from page number
				int currentPage = (input.SkipCount / input.MaxResultCount) + 1;
				if (currentPage < 1) currentPage = 1;

				var output = await _homeCustomerAppService.GetAllProductHomeCustomers(input);

				var model = new HomeCustomerViewModel()
				{
					ProductsInfo = output.Items.ToList(),
					TotalCount = output.TotalCount,
					CurrentPage = currentPage,
					PageSize = input.MaxResultCount
				};

				return View(model);
			}

			public async Task<IActionResult> DetailProductCustomer(int id)
			{
				var product = await _homeCustomerAppService.GetProductById(id);
				
				var comments = await _productCommentAppService.GetProductVariantCommentsTree(id);

				var ratingsResult = await _productRatingAppService.GetAllRatings(new GetProductRatingsInput
				{
					ProductVariantId = id,
					MaxResultCount = 10,
					IsApproved = true
				});

				var ratingStatistics = await _productRatingAppService.GetProductRatingStatistics(id);

				bool canRate = false;
				ProductRatingDto userRating = null;
				
				if (AbpSession.UserId.HasValue)
				{
					canRate = await _productRatingAppService.CanUserRateProduct(id);
					userRating = ratingsResult.Items.FirstOrDefault(r => r.UserId == AbpSession.UserId.Value);
				}

				var model = new HomeCustomerViewModel()
				{
					ProductInfo = product
				};

				// ViewBag đến view _ProductComments.cshtml
				ViewBag.ProductComments = comments;
				ViewBag.ProductVariantId = id;
				
				ViewBag.ProductRatings = ratingsResult.Items;
				ViewBag.RatingStatistics = ratingStatistics;
				ViewBag.CanRate = canRate;
				ViewBag.UserRating = userRating;

				return View(model);
			}

			/// <summary>
			/// Load comments partial view - AJAX call to refresh only comment section
			/// </summary>
			[HttpGet]
			public async Task<IActionResult> LoadCommentsPartial(int productVariantId)
			{
				var comments = await _productCommentAppService.GetProductVariantCommentsTree(productVariantId);
				
				ViewBag.ProductVariantId = productVariantId;
				
				return PartialView("Components/ProductComments/_CommentsList", comments);
			}

			/// <summary>
			/// Send notification to admin when new comment is created
			/// </summary>
			private async Task SendCommentNotificationToAdmin(int productVariantId, string productName, string userName, string commentContent)
			{
				// Get all admin users
				var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
				
				if (adminUsers != null && adminUsers.Any())
				{
					foreach (var admin in adminUsers)
					{
						// Create notification data
						var notificationData = new Abp.Notifications.NotificationData();
						notificationData["ProductVariantId"] = productVariantId;
						notificationData["ProductName"] = productName;
						notificationData["UserName"] = userName;
						notificationData["CommentContent"] = commentContent.Length > 50 
							? commentContent.Substring(0, 50) + "..." 
							: commentContent;
						notificationData["Url"] = $"/HomeCustomer/DetailProductCustomer?id={productVariantId}";
						
						// Publish notification
						await _notificationPublisher.PublishAsync(
							notificationName: "App.NewProductComment",
							data: notificationData,
							severity: NotificationSeverity.Info,
							userIds: new[] { new Abp.UserIdentifier(admin.TenantId, admin.Id) }
						);
					}
				}
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

			[HttpPost]
			public async Task<IActionResult> GetListProduct(SearchHomeCustomerDto input)
			{
				// 1. Gọi Service lấy dữ liệu như bình thường
				var result = await _homeCustomerAppService.GetAllProductHomeCustomers(input);

				ViewBag.TotalCount = result.TotalCount;
				return PartialView("_ProductList", result.Items);
			}
			
			public async Task<PartialViewResult> FilterAdvancedModal()
			{

				return PartialView("_FilterAdvancedModal");
			}

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