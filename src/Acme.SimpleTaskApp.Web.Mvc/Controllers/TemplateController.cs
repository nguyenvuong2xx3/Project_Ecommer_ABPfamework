using Acme.SimpleTaskApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class TemplateController : SimpleTaskAppControllerBase
	{
		
		/// <summary>
		/// Tạo file Excel template cho import sản phẩm
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> GenerateProductImportTemplate()
		{
			try
			{
				// Set EPPlus license context

				using (var package = new ExcelPackage())
				{
					var worksheet = package.Workbook.Worksheets.Add("Sản phẩm");

					// Set column widths
					worksheet.Column(1).Width = 25;  // Name
					worksheet.Column(2).Width = 30;  // Description
					worksheet.Column(3).Width = 15;  // Price
					worksheet.Column(4).Width = 15;  // StockQuantity
					worksheet.Column(5).Width = 15;  // CategoryId
					worksheet.Column(6).Width = 15;  // Color
					worksheet.Column(7).Width = 15;  // Ram
					worksheet.Column(8).Width = 15;  // Storage
					worksheet.Column(9).Width = 15;  // VariantPrice
					worksheet.Column(10).Width = 15; // VariantStock
					worksheet.Column(11).Width = 30; // Images

					// Header row - CẬP NHẬT: Bỏ SKU, dùng Name làm định danh
					var headers = new[]
					{
						"Tên Sản Phẩm (Name)*",
						"Mô Tả (Description)",
						"Giá Mặc Định (Price)",
						"Số Lượng (StockQuantity)",
						"ID Danh Mục (CategoryId)",
						"Màu Biến Thể (Color)",
						"RAM (Ram)",
						"Bộ Nhớ (Storage)",
						"Giá Biến Thể (VariantPrice)",
						"Số Lượng Biến Thể (VariantStock)",
						"Danh Sách Ảnh (Images - tên file cách bằng dấu phẩy)"
					};

					// Ghi header
					for (int col = 1; col <= headers.Length; col++)
					{
						var cell = worksheet.Cells[1, col];
						cell.Value = headers[col - 1];
						cell.Style.Font.Bold = true;
						cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
						cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 112, 192));
						cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
						cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
						cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
						cell.Style.WrapText = true;
					}

					// Example row 1: Product with 1 variant
					int row = 2;
					worksheet.Cells[row, 1].Value = "Samsung Galaxy A12";
					worksheet.Cells[row, 2].Value = "Điện thoại thông minh Android, màn hình 6.5 inch";
					worksheet.Cells[row, 3].Value = 3500000;
					worksheet.Cells[row, 4].Value = 100;
					worksheet.Cells[row, 5].Value = 1;
					worksheet.Cells[row, 6].Value = "Đen";
					worksheet.Cells[row, 7].Value = "4GB";
					worksheet.Cells[row, 8].Value = "64GB";
					worksheet.Cells[row, 9].Value = 3500000;
					worksheet.Cells[row, 10].Value = 100;
					worksheet.Cells[row, 11].Value = "image1.jpg, image2.jpg";

					// Example row 2: Same product, different color (variant)
					row = 3;
					worksheet.Cells[row, 6].Value = "Trắng";
					worksheet.Cells[row, 7].Value = "4GB";
					worksheet.Cells[row, 8].Value = "64GB";
					worksheet.Cells[row, 9].Value = 3500000;
					worksheet.Cells[row, 10].Value = 50;
					worksheet.Cells[row, 11].Value = "image3.jpg, image4.jpg";

					// Example row 3: Different product
					row = 4;
					worksheet.Cells[row, 1].Value = "iPhone 13 Pro";
					worksheet.Cells[row, 2].Value = "Điện thoại Apple thế hệ mới, màn hình Super Retina XDR";
					worksheet.Cells[row, 3].Value = 29990000;
					worksheet.Cells[row, 4].Value = 50;
					worksheet.Cells[row, 5].Value = 1;
					worksheet.Cells[row, 6].Value = "Bạc";
					worksheet.Cells[row, 7].Value = "6GB";
					worksheet.Cells[row, 8].Value = "128GB";
					worksheet.Cells[row, 9].Value = 29990000;
					worksheet.Cells[row, 10].Value = 50;
					worksheet.Cells[row, 11].Value = "image5.jpg, image6.jpg, image7.jpg";

					// Add instructions sheet
					var instructionSheet = package.Workbook.Worksheets.Add("Hướng Dẫn");

					row = 1;
					instructionSheet.Cells[row, 1].Value = "HƯỚNG DẪN IMPORT SẢN PHẨM";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Size = 14;

					row = 3;
					instructionSheet.Cells[row, 1].Value = "1. CẤU TRÚC FILE EXCEL";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;

					row = 4;
					instructionSheet.Cells[row, 1].Value = "- Sheet 'Sản phẩm' chứa dữ liệu import";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 5;
					instructionSheet.Cells[row, 1].Value = "- Hàng 1 là header (tiêu đề cột)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 6;
					instructionSheet.Cells[row, 1].Value = "- Từ hàng 2 trở đi là dữ liệu sản phẩm";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 8;
					instructionSheet.Cells[row, 1].Value = "2. HÌNH ẢNH";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;

					row = 9;
					instructionSheet.Cells[row, 1].Value = "- Nhúng hình ảnh trực tiếp trong file Excel";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 10;
					instructionSheet.Cells[row, 1].Value = "- Cột 'Danh Sách Ảnh' chứa tên file cách nhau bởi dấu phẩy (vd: image1.jpg, image2.jpg)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 11;
					instructionSheet.Cells[row, 1].Value = "- Hình ảnh sẽ được tự động trích xuất từ thư mục xl/media/ của file Excel";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 13;
					instructionSheet.Cells[row, 1].Value = "3. BIẾN THỂ SẢN PHẨM";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;

					row = 14;
					instructionSheet.Cells[row, 1].Value = "- Một sản phẩm có thể có nhiều biến thể (khác màu, bộ nhớ, v.v)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 15;
					instructionSheet.Cells[row, 1].Value = "- Hàng đầu tiên của sản phẩm chứa thông tin sản phẩm chính";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 16;
					instructionSheet.Cells[row, 1].Value = "- Các hàng tiếp theo cùng loại sản phẩm (không có tên) sẽ được coi là biến thể";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 17;
					instructionSheet.Cells[row, 1].Value = "- Mỗi biến thể được định danh bởi: Màu + RAM + Bộ nhớ";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 19;
					instructionSheet.Cells[row, 1].Value = "4. TRƯỜNG BẮT BUỘC";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;

					row = 20;
					instructionSheet.Cells[row, 1].Value = "- Tên Sản Phẩm (Name) * - Dùng để định danh sản phẩm";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 21;
					instructionSheet.Cells[row, 1].Value = "- Màu Biến Thể (Color) * - Dùng để định danh biến thể";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 22;
					instructionSheet.Cells[row, 1].Value = "- RAM (Ram) * - Dùng để định danh biến thể";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 23;
					instructionSheet.Cells[row, 1].Value = "- Bộ Nhớ (Storage) * - Dùng để định danh biến thể";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 25;
					instructionSheet.Cells[row, 1].Value = "5. LƯU Ý KHÁC";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;

					row = 26;
					instructionSheet.Cells[row, 1].Value = "- Định dạng file: .xlsx hoặc .xls";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 27;
					instructionSheet.Cells[row, 1].Value = "- Giá và số lượng phải là số (không chứa ký tự đặc biệt)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 28;
					instructionSheet.Cells[row, 1].Value = "- CategoryId là ID danh mục trong hệ thống";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 29;
					instructionSheet.Cells[row, 1].Value = "- Sản phẩm được định danh bởi TÊN (không dùng SKU)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);

					instructionSheet.Column(1).Width = 100;

					// Save to byte array
					var excelBytes = await Task.Run(() => package.GetAsByteArray());

					return File(
						excelBytes,
						"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
						"ProductImportTemplate.xlsx"
					);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Lỗi tạo template Excel", ex);
				return Json(new { success = false, message = "Lỗi tạo template: " + ex.Message });
			}
		}
	}
}
