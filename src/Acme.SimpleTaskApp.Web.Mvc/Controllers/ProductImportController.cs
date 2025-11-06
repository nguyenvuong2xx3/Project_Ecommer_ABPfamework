using Acme.SimpleTaskApp.ProductImport;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Acme.SimpleTaskApp.Controllers;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
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
		[HttpPost("import-excel")]
		public async Task<IActionResult> ImportProducts()
		{
			try
			{
				var file = Request.Form.Files["file"];

				if (file == null || file.Length == 0)
				{
					return Json(new {
						success = false,
						message = "Vui lòng chọn file Excel"
					});
				}

				// Kiểm tra định dạng file
				var fileName = file.FileName.ToLower();
				if (!fileName.EndsWith(".xlsx") && !fileName.EndsWith(".xls"))
				{
					return Json(new { 
						success = false, 
						message = "Chỉ hỗ trợ file Excel (.xlsx, .xls)" 
					});
				}

				// Import sản phẩm
				using (var stream = file.OpenReadStream())
				{
					var result = await _productImportAppService.ImportProductsWithImagesAsync(stream);

					if (!result.IsSuccess)
					{
						return Json(new { 
							success = false, 
							message = result.ErrorMessage,
							details = result.ErrorDetails
						});
					}

					return Json(new { 
						success = true, 
						message = $"Import thành công {result.SuccessCount}/{result.TotalCount} sản phẩm",
						successCount = result.SuccessCount,
						totalCount = result.TotalCount,
						warnings = result.Warnings,
						warningCount = result.WarningCount
					});
				}
			}
			catch (System.Exception ex)
			{
				return Json(new { 
					success = false, 
					message = "Lỗi server: " + ex.Message 
				});
			}
		}
	}
}
