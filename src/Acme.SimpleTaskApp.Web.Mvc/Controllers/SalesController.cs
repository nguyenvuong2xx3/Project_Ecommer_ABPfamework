using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.ProductVariants;
using Acme.SimpleTaskApp.Sales;
using Acme.SimpleTaskApp.Sales.Dtos;
using Acme.SimpleTaskApp.Web.Models.Sales;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Acme.SimpleTaskApp.Authorization;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class SalesController : SimpleTaskAppControllerBase
	{
		private readonly ISaleAppService _saleAppService;
		private readonly ICategoryAppService _categoryAppService;
		private readonly IProductAppService _productAppService;
		private readonly IProductVariantAppService _productVariantAppService;

		public SalesController(
			ISaleAppService saleAppService,
			ICategoryAppService categoryAppService,
			IProductAppService productAppService,
			IProductVariantAppService productVariantAppService)
		{
			_saleAppService = saleAppService;
			_categoryAppService = categoryAppService;
			_productAppService = productAppService;
			_productVariantAppService = productVariantAppService;
		}

		// GET: /Sales/Index
		public IActionResult Index()
		{
			return View();
		}

		// GET: /Sales/CreateModal
		[AbpMvcAuthorize(PermissionNames.Pages_Sales_Create)]
		public async Task<IActionResult> CreateModal()
		{
			var categories = await _categoryAppService.GetAllCategoriesForSelect();
			var products = await _productAppService.GetAllProductsForSelect();
			var variants = await _productVariantAppService.GetAllProductVariantsForSelect();

			var viewModel = new SaleViewModel
			{
				Categories = categories,
				Products = products,
				ProductVariants = variants
			};

			return PartialView("_CreateModal", viewModel);
		}

		// POST: /Sales/Create
		[AbpMvcAuthorize(PermissionNames.Pages_Sales_Create)]
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateSaleDto input)
		{
			var result = await _saleAppService.CreateSale(input);
			return Json(result);
		}

		// GET: /Sales/EditModal
		[AbpMvcAuthorize(PermissionNames.Pages_Sales_Edit)]
		public async Task<IActionResult> EditModal(int id)
		{
			var sale = await _saleAppService.GetSaleById(id);
			var categories = await _categoryAppService.GetAllCategoriesForSelect();
			var products = await _productAppService.GetAllProductsForSelect();
			var variants = await _productVariantAppService.GetAllProductVariantsForSelect();

			var viewModel = new SaleViewModel
			{
				Sale = sale,
				Categories = categories,
				Products = products,
				ProductVariants = variants
			};

			return PartialView("_EditModal", viewModel);
		}

		// POST: /Sales/Edit
		[AbpMvcAuthorize(PermissionNames.Pages_Sales_Edit)]
		[HttpPost]
		public async Task<IActionResult> Edit([FromBody] UpdateSaleDto input)
		{
			var result = await _saleAppService.UpdateSale(input);
			return Json(result);
		}

		// GET: /Sales/DetailModal
		public async Task<IActionResult> DetailModal(int id)
		{
			var sale = await _saleAppService.GetSaleById(id);
			var viewModel = new SaleViewModel
			{
				Sale = sale
			};
			return PartialView("_DetailModal", viewModel);
		}

		// POST: /Sales/Delete
		[AbpMvcAuthorize(PermissionNames.Pages_Sales_Delete)]
		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			await _saleAppService.DeleteSale(id);
			return Json(new { success = true });
		}

		// GET: /Sales/ToggleActive - Bật/tắt trạng thái active
		[AbpMvcAuthorize(PermissionNames.Pages_Sales_ToggleActive)]
		[HttpPost]
		public async Task<IActionResult> ToggleActive(int id)
		{
			var sale = await _saleAppService.GetSaleById(id);
			var updateDto = new UpdateSaleDto
			{
				Id = sale.Id,
				Name = sale.Name,
				Description = sale.Description,
				DiscountType = sale.DiscountType,
				VoucherCode = sale.VoucherCode,
				UsageLimit = sale.UsageLimit,
				StartDate = sale.StartDate,
				EndDate = sale.EndDate,
				DiscountPercentage = sale.DiscountPercentage,
				MaximumDiscountAmount = sale.MaximumDiscountAmount,
				MinimumOrderValue = sale.MinimumOrderValue,
				ApplyTo = sale.ApplyTo,
				CategoryIds = sale.CategoryIds,
				ProductIds = sale.ProductIds,
				ProductVariantIds = sale.ProductVariantIds,
				IsActive = !sale.IsActive // Toggle
			};

			var result = await _saleAppService.UpdateSale(updateDto);
			return Json(result);
		}

		// GET: /Sales/GetStatistics
		public async Task<IActionResult> GetStatistics(int id)
		{
			var stats = await _saleAppService.GetSaleStatistics(id);
			return Json(stats);
		}
	}
}
