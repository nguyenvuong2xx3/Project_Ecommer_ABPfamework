using Acme.SimpleTaskApp.ProductImport;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Acme.SimpleTaskApp.Controllers;
using Microsoft.AspNetCore.Http;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class ProductImportController : SimpleTaskAppControllerBase
	{
		private readonly IProductImportAppService _productImportAppService;

		public ProductImportController(IProductImportAppService productImportAppService)
		{
			_productImportAppService = productImportAppService;
		}

		/// <summary>
		/// Import sản phẩm từ file Excel với hình ảnh nhúng
		/// </summary>
		///
		[HttpPost]
		public async Task<IActionResult> ImportProducts(IFormFile file)
		{
			try
			{
				if (file == null || file.Length == 0)
				{
					return Json(new { success = false, message = "Vui lòng chọn file Excel" });
				}

				// Kiểm tra định dạng file
				var fileName = file.FileName.ToLower();
				if (!fileName.EndsWith(".xlsx") && !fileName.EndsWith(".xls"))
				{
					return Json(new { success = false, message = "Chỉ hỗ trợ file Excel (.xlsx, .xls)" });
				}

				// Import sản phẩm - THÊM ĐOẠN NÀY

				var result = await _productImportAppService.ImportProductsWithImagesAsync(file);

				if (!result.IsSuccess)
				{
					return Json(new
					{
						success = false,
						message = result.ErrorMessage,
						details = result.ErrorDetails,
						warnings = result.Warnings,
						warningCount = result.WarningCount
					});
				}

				return Json(new
				{
					success = true,
					message = $"Import thành công {result.SuccessCount}/{result.TotalCount} sản phẩm",
					successCount = result.SuccessCount,
					totalCount = result.TotalCount,
					warnings = result.Warnings,
					warningCount = result.WarningCount
				});
			}
			catch (System.Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Lỗi server: " + ex.Message,
					details = ex.StackTrace
				});
			}
		}
	}
}
