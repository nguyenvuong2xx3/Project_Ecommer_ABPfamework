using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.ProductImport;
using Acme.SimpleTaskApp.ProductImport.Dtos;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.Web.Models.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Acme.SimpleTaskApp.Web.Controllers
{
	[AbpMvcAuthorize]
	public class ProductsController : SimpleTaskAppControllerBase
	{
		private readonly IProductImportExportAppService _productImportExportAppService;
		private readonly IProductAppService _productAppService;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly ICategoryAppService _categoryAppService;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<Product> _productRepository;
		public ProductsController(IProductAppService productAppService,
							ICategoryAppService categoryAppService,
							IWebHostEnvironment webHostEnvironment,
							IProductImportExportAppService productImportExportAppService,
							IRepository<Category> categoryRepository)
		{
			_productImportExportAppService = productImportExportAppService;
			_categoryRepository = categoryRepository;
			_productAppService = productAppService;
			_categoryAppService = categoryAppService;
			_webHostEnvironment = webHostEnvironment;
		}
		[AbpAuthorize(PermissionNames.Pages_products_view)]
		public async Task<ActionResult> Index()
		{
			var query = _categoryRepository.GetAll();
			var model = new ProductViewModel()
			{
				Categories = query.ToList()
			};
			return View(model);
		}

		[AbpAuthorize(PermissionNames.Pages_products_create)]
		public async Task<PartialViewResult> CreateModal()
		{
			var categories = await _categoryRepository.GetAllListAsync();

			var model = new ProductViewModel()
			{
				Categories = categories
			};

			return PartialView("_CreateProductModal", model);
		}

		[AbpAuthorize(PermissionNames.Pages_products_create)]
		public IActionResult CreateProduct(CreateProductDto model)
		{
			if (ModelState.IsValid)
			{
				var product = _productAppService.CreateProducts(model);
				//_productAppService.CreateProductVariants(product.Id, model.ProductVariants, model.Name);
				_productAppService.CreateGeneralProductImages(product.Id, model.ProductImages, model.Name);
				if (product != null)
				{
					return RedirectToAction("Index");
				}
			}
			return View("Create");
		}

		[AbpAuthorize(PermissionNames.Pages_products_update)]
		public async Task<PartialViewResult> EditModal(int productId)
		{
			var categories = await _categoryRepository.GetAllAsync();
			var product = await _productAppService.GetProductById(productId);

			var model = new ProductViewModel()
			{
				Categories = categories.ToList(),
				Product = product
			};
			return PartialView("_EditProductModal", model);
		}
		[AbpAuthorize(PermissionNames.Pages_products_update)]
		public async Task EditProduct(Product model)
		{
			var product = await _productAppService.EditProduct(model);
		}
		[AbpAuthorize(PermissionNames.Pages_products_delete)]
		public async Task Delete(int id)
		{
			await _productAppService.DeleteProduct(id);
		}
		public async Task<PartialViewResult> ExportModal()
		{
			var query = await _categoryRepository.GetAllListAsync();
			var model = new ProductViewModel()
			{
				Categories = query
			};
			return PartialView("_ExportProductModal", model);
		}
		public async Task<PartialViewResult> ImportModal()
		{
			return PartialView("_ImportDataModal");
		}

		public async Task<ImportResult> ImportData(IFormFile file)
		{
			ExcelPackage.License.SetNonCommercialPersonal("ImportForDoAn");

			var result = new ImportResult();
			int totalProducts = 0;
			int totalVariants = 0;
			int? currentProductId = null;

			if (file == null || file.Length == 0)
			{
				result.IsSuccess = false;
				result.Message = "File không tồn tại.";
				return result;
			}

			using (var stream = new MemoryStream())
			{
				await file.CopyToAsync(stream);
				using (var package = new ExcelPackage(stream))
				{
					var worksheet = package.Workbook.Worksheets.FirstOrDefault();
					if (worksheet == null)
					{
						result.IsSuccess = false;
						result.Message = "File không có worksheet.";
						return result;
					}

					int rowCount = worksheet.Dimension.End.Row;

					// Lặp từ dòng 2 (bỏ qua header)
					for (int row = 2; row <= rowCount; row++)
					{
						var productName = worksheet.Cells[row, 1].Text?.Trim();

						if (!string.IsNullOrWhiteSpace(productName))
						{
							totalProducts++;
							var productResult = await _productImportExportAppService.ProcessProductRowAsync(worksheet, row);

							if (productResult.IsSuccess && productResult.ProductId.HasValue)
							{
								currentProductId = productResult.ProductId.Value; // Set context cho các dòng biến thể tiếp theo
							}
							else
							{
								currentProductId = null; // Lỗi, reset context
								result.ErrorList.AddRange(productResult.Errors.Select(e => $"Dòng {row} (sản phẩm): {e}"));
							}
						}
						// Kịch bản 2: Đây là DÒNG BIẾN THỂ (Cột 1 rỗng)
						else
						{
							var variantColor = worksheet.Cells[row, 8].Text?.Trim();
							if (string.IsNullOrWhiteSpace(variantColor))
							{
								continue; // Đây là dòng trống, bỏ qua
							}

							if (currentProductId == null)
							{
								result.ErrorList.Add($"Dòng {row}: Biến thể '{variantColor}' không thuộc sản phẩm nào (thiếu dòng sản phẩm ở trên).");
								continue;
							}

							// Xử lý như một dòng chỉ chứa biến thể (và ảnh của biến thể)
							var varianttResult = await _productImportExportAppService.ProcessVariantRowAsync(worksheet, row, currentProductId.Value);
							if (varianttResult.IsSuccess)
							{
								totalVariants++;
							}
							else
							{
								result.ErrorList.AddRange(varianttResult.Errors.Select(e => $"Dòng {row} (biến thể): {e}"));
							}
						}

					}
				}
			}

			result.TotalProducts = totalProducts;
			result.TotalVariants = totalVariants;
			result.IsSuccess = !result.ErrorList.Any();
			result.Message = result.IsSuccess
							? $"Import thành công {totalProducts} sản phẩm và {totalVariants} biến thể."
							: $"Import thất bại. Có {result.ErrorList.Count} lỗi.";

			return result;
		}
	}
}
