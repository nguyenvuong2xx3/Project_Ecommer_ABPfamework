using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.ProductImport;
using Acme.SimpleTaskApp.ProductImport.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class ProductImportExportController : SimpleTaskAppControllerBase
	{
		private readonly IProductImportExportAppService _productImportExportAppService;

		public ProductImportExportController(IProductImportExportAppService productImportExportAppService)
		{
			_productImportExportAppService = productImportExportAppService;
		}

		/// Import sản phẩm từ file Excel với hình ảnh nhúng
		//[HttpPost]
		//public async Task<IActionResult> ImportProducts(IFormFile file)
		//{
		//	try
		//	{
		//		if (file == null || file.Length == 0)
		//		{
		//			return Json(new { success = false, message = "Vui lòng chọn file Excel" });
		//		}

		//		// Kiểm tra định dạng file
		//		var fileName = file.FileName.ToLower();
		//		if (!fileName.EndsWith(".xlsx") && !fileName.EndsWith(".xls"))
		//		{
		//			return Json(new { success = false, message = "Chỉ hỗ trợ file Excel (.xlsx, .xls)" });
		//		}

		//		// Import sản phẩm - THÊM ĐOẠN NÀY

		//		var result = await _productImportAppService.ImportProductsWithImagesAsync(file);

		//		if (!result.IsSuccess)
		//		{
		//			return Json(new
		//			{
		//				success = false,
		//				message = result.ErrorMessage,
		//				details = result.ErrorDetails,
		//				warnings = result.Warnings,
		//				warningCount = result.WarningCount
		//			});
		//		}

		//		return Json(new
		//		{
		//			success = true,
		//			message = $"Import thành công {result.SuccessCount}/{result.TotalCount} sản phẩm",
		//			successCount = result.SuccessCount,
		//			totalCount = result.TotalCount,
		//			warnings = result.Warnings,
		//			warningCount = result.WarningCount
		//		});
		//	}
		//	catch (System.Exception ex)
		//	{
		//		return Json(new
		//		{
		//			success = false,
		//			message = "Lỗi server: " + ex.Message,
		//			details = ex.StackTrace
		//		});
		//	}
		//}

		[HttpPost]
		public async Task<IActionResult> ExportProducts([FromBody] ExportProductInput input)
		{
			try
			{
				var result = await _productImportExportAppService.ExportProducts(input);

				if (result.IsSuccess)
				{
					return File(
						result.FileContent,
						result.ContentType,
						result.FileName
					);
				}
				else
				{
					return Json(new { success = false, message = result.ErrorMessage });
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Error exporting products", ex);
				return Json(new { success = false, message = "Lỗi export sản phẩm: " + ex.Message });
			}
		}
	}
}
