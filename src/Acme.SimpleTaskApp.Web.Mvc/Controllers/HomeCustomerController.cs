using Abp.Application.Services.Dto;
using Abp.UI;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.Web.Models.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class HomeCustomerController : SimpleTaskAppControllerBase
	{
		private readonly IProductAppService _productAppService;
		private readonly IWebHostEnvironment webHostEnvironment;
		private readonly ICategoryAppService _categoryAppService;


		public HomeCustomerController(IProductAppService productAppService,
															ICategoryAppService categoryAppService,
															IWebHostEnvironment webHostEnvironment)
		{
			_productAppService = productAppService;
			_categoryAppService = categoryAppService;
			this.webHostEnvironment = webHostEnvironment;
		}
		public async Task<ActionResult> Index(GetAllProductsInput input, GetAllCategoryDto input1)
		{
			var output = await _productAppService.GetAllProducts(input);
			var categories = await _categoryAppService.GetAllCategories(input1);

			// Chuyển đổi CategoryListDto sang SelectListItem
			var categoriesSelectList = categories.Items
					.Select(c => new SelectListItem
					{
						Value = c.Id.ToString(),
						Text = c.Name
					})
					.ToList();

			var model = new ProductViewModel(output.Items)
			{
				Categories = categoriesSelectList // Gán danh sách đã chuyển đổi
			};

			return View(model);
		}
		//public async Task<ActionResult> DetailProductCusTomer(int productId)
		//{
		//	try
		//	{
		//		// Lấy thông tin sản phẩm
		//		var product = await _productAppService.GetByIdProducts(new EntityDto<int>(productId));

		//		// Lấy DANH SÁCH TẤT CẢ CATEGORIES
		//		var categoriesResult = await _categoryAppService.GetAllCategories(new GetAllCategoryDto { MaxResultCount = 12 });

		//		// Tạo ProductListDto và lấy CategoryName từ danh sách categories
		//		var detailProductDto = new ProductListDto
		//		{
		//			Id = product.Id,
		//			Name = product.Name,
		//			Description = product.Description,
		//			Price = product.Price,
		//			Image = product.Image,
		//			State = product.State,
		//			CreationTime = product.CreationTime,
		//			CategoryId = product.CategoryId,
		//			// Lấy CategoryName từ danh sách categories dựa trên CategoryId
		//			CategoryName = categoriesResult.Items.FirstOrDefault(c => c.Id == product.CategoryId)?.Name ?? "Không xác định"
		//		};

		//		// Truyền cả danh sách categories vào ViewModel
		//		var viewModel = new DetailProductModalViewModel
		//		{
		//			Product = detailProductDto,
		//			Categories = categoriesResult.Items // Thêm danh sách categories vào ViewModel
		//		};

		//		return View(viewModel);
		//	}
		//	catch (UserFriendlyException ex)
		//	{
		//		Logger.Error(ex.Message, ex);
		//		throw;
		//	}
		//}

		// Trong HomeCustomerController.cs
		public async Task<ActionResult> SearchProductCustomer(GetAllProductsInput input)
		{
			try
			{
				var result = await _productAppService.SearchProducts(input);
				var categories = await _categoryAppService.GetAllCategories(new GetAllCategoryDto());

				var model = new ProductViewModel(result.Items)
				{
					Categories = categories.Items.Select(c => new SelectListItem
					{
						Value = c.Id.ToString(),
						Text = c.Name
					}).ToList(),
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
	}
}
