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
				ExcelPackage.License.SetNonCommercialPersonal("ImportForDoAn");

				using (var package = new ExcelPackage())
				{
					var worksheet = package.Workbook.Worksheets.Add("Sản phẩm");

					// Set column widths
					worksheet.Column(1).Width = 25;  // Name
					worksheet.Column(2).Width = 30;  // Description
					worksheet.Column(3).Width = 20;  // CategoryName
					worksheet.Column(4).Width = 30;  // Screen
					worksheet.Column(5).Width = 20;  // Processor
					worksheet.Column(6).Width = 35;  // CameraSystem
					worksheet.Column(7).Width = 25;  // Battery
					worksheet.Column(8).Width = 15;  // Color
					worksheet.Column(9).Width = 15;  // Ram
					worksheet.Column(10).Width = 15; // Storage
					worksheet.Column(11).Width = 15; // VariantPrice
					worksheet.Column(12).Width = 15; // VariantStock
					worksheet.Column(13).Width = 20; // ProductImages (cell để insert ảnh)
					worksheet.Column(14).Width = 20; // VariantImages (cell để insert ảnh)

					// Set row height cho các hàng dữ liệu (để chứa ảnh)
					worksheet.Row(2).Height = 80;
					worksheet.Row(3).Height = 80;
					worksheet.Row(4).Height = 80;

					// Header row
					var headers = new[]
					{
						// Product fields
						"Tên Sản Phẩm (Name)*",
						"Mô Tả (Description)",
						"Tên Danh Mục (CategoryName)*",
						"Màn Hình (Screen)",
						"Bộ Xử Lý (Processor)",
						"Camera (CameraSystem)",
						"Pin (Battery)",
						// ProductVariant fields
						"Màu Biến Thể (Color)*",
						"RAM (Ram)*",
						"Bộ Nhớ (Storage)*",
						"Giá Biến Thể (VariantPrice)*",
						"Số Lượng Biến Thể (VariantStock)*",
						// Images - INSERT ẢNH VÀO CELL NÀY
						"Ảnh Sản Phẩm\n(Insert ảnh vào đây)",
						"Ảnh Biến Thể\n(Insert ảnh vào đây)"
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

					// Làm nổi bật cột ảnh
					worksheet.Cells[1, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 192, 0)); // Màu cam
					worksheet.Cells[1, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 192, 0)); // Màu cam

					// Example row 1: Product with first variant
					int row = 2;
					// Product fields
					worksheet.Cells[row, 1].Value = "Samsung Galaxy A12";
					worksheet.Cells[row, 2].Value = "Điện thoại thông minh Android, màn hình 6.5 inch";
					worksheet.Cells[row, 3].Value = "Điện thoại";
					worksheet.Cells[row, 4].Value = "6.5 inch, HD+, PLS LCD";
					worksheet.Cells[row, 5].Value = "MediaTek Helio P35";
					worksheet.Cells[row, 6].Value = "Chính 48MP, Macro 5MP, Góc rộng 5MP, Độ sâu 2MP";
					worksheet.Cells[row, 7].Value = "5000mAh, sạc nhanh 15W";
				
					worksheet.Cells[row, 13].Value = "← Insert ảnh Product vào đây";
					worksheet.Cells[row, 14].Value = "← Insert ảnh Variant vào đây";

					// Example row 2: Same product, different variant
					row = 3;
					// Product fields (empty - cùng product)
					// ProductVariant fields (different variant)
					worksheet.Cells[row, 8].Value = "Trắng";
					worksheet.Cells[row, 9].Value = "4GB";
					worksheet.Cells[row, 10].Value = "64GB";
					worksheet.Cells[row, 11].Value = 3500000;
					worksheet.Cells[row, 12].Value = 50;
					// Images
					worksheet.Cells[row, 14].Value = "← Insert ảnh Variant vào đây";
					row = 4;
					// ProductVariant fields
					worksheet.Cells[row, 8].Value = "Đen";
					worksheet.Cells[row, 9].Value = "4GB";
					worksheet.Cells[row, 10].Value = "64GB";
					worksheet.Cells[row, 11].Value = 3500000;
					worksheet.Cells[row, 12].Value = 100;
					worksheet.Cells[row, 14].Value = "← Insert ảnh Variant vào đây";
					// Images cells - để trống, người dùng sẽ insert ảnh vào đây

					// Example row 3: Different product
					row = 5;
					// Product fields
					worksheet.Cells[row, 1].Value = "iPhone 13 Pro";
					worksheet.Cells[row, 2].Value = "Điện thoại Apple thế hệ mới, màn hình Super Retina XDR";
					worksheet.Cells[row, 3].Value = "Điện thoại";
					worksheet.Cells[row, 4].Value = "6.1 inch, Super Retina XDR, ProMotion 120Hz";
					worksheet.Cells[row, 5].Value = "Apple A15 Bionic";
					worksheet.Cells[row, 6].Value = "Hệ thống 3 camera: 12MP Chính, 12MP Góc siêu rộng, 12MP Telephoto";
					worksheet.Cells[row, 7].Value = "Xem video lên đến 22 giờ";
					// ProductVariant fields
					worksheet.Cells[row, 8].Value = "Bạc";
					worksheet.Cells[row, 9].Value = "6GB";
					worksheet.Cells[row, 10].Value = "128GB";
					worksheet.Cells[row, 11].Value = 29990000;
					worksheet.Cells[row, 12].Value = 50;
					// Images
					worksheet.Cells[row, 13].Value = "← Insert ảnh Product vào đây";
					worksheet.Cells[row, 14].Value = "← Insert ảnh Variant vào đây";

					// Add instructions sheet
					var instructionSheet = package.Workbook.Worksheets.Add("Hướng Dẫn");

					row = 1;
					instructionSheet.Cells[row, 1].Value = "HƯỚNG DẪN IMPORT SẢN PHẨM - TỰ ĐỘNG NHẬN DIỆN ẢNH";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Size = 16;
					instructionSheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);

					row = 3;
					instructionSheet.Cells[row, 1].Value = "🎉 ĐƠN GIẢN HƠN - KHÔNG CẦN NHẬP TÊN FILE";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Size = 14;
					instructionSheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.Green);

					row = 4;
					instructionSheet.Cells[row, 1].Value = "Chỉ cần INSERT ẢNH trực tiếp vào cột M (Ảnh SP) hoặc N (Ảnh Variant)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;

					row = 6;
					instructionSheet.Cells[row, 1].Value = "CÁCH NHẬP ẢNH - CỰC KỲ ĐƠN GIẢN";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Size = 14;
					instructionSheet.Cells[row, 1].Style.Font.UnderLine = true;

					row = 7;
					instructionSheet.Cells[row, 1].Value = "Bước 1: Click vào cell M (Ảnh Sản Phẩm) hoặc N (Ảnh Variant) của hàng bạn muốn";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 8;
					instructionSheet.Cells[row, 1].Value = "Bước 2: Vào Insert > Picture > This Device";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 9;
					instructionSheet.Cells[row, 1].Value = "Bước 3: Chọn ảnh từ máy tính (có thể chọn nhiều ảnh cùng lúc)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 10;
					instructionSheet.Cells[row, 1].Value = "Bước 4: Đảm bảo ảnh nằm trong ĐÚNG CELL của hàng tương ứng";
					instructionSheet.Cells[row, 1].Style.WrapText = true;
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);

					row = 11;
					instructionSheet.Cells[row, 1].Value = "Bước 5: Lưu file và import - HỆ THỐNG TỰ ĐỘNG NHẬN DIỆN ẢNH!";
					instructionSheet.Cells[row, 1].Style.WrapText = true;
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;

					row = 13;
					instructionSheet.Cells[row, 1].Value = "⚡ LƯU Ý QUAN TRỌNG";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Size = 14;
					instructionSheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);

					row = 14;
					instructionSheet.Cells[row, 1].Value = "✓ Ảnh PHẢI nằm trong cell M (hàng có Product) hoặc N (mỗi variant)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 15;
					instructionSheet.Cells[row, 1].Value = "✓ Một cell có thể chứa NHIỀU ảnh (kéo thả nhiều ảnh vào cùng cell)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 16;
					instructionSheet.Cells[row, 1].Value = "✓ Ảnh sẽ được resize tự động khi lưu lên server";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 17;
					instructionSheet.Cells[row, 1].Value = "✓ KHÔNG CẦN nhập tên file - hệ thống tự động tạo tên unique";
					instructionSheet.Cells[row, 1].Style.WrapText = true;
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;

					row = 19;
					instructionSheet.Cells[row, 1].Value = "═══════════════════════════════════════════════════════════════════";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;

					row = 21;
					instructionSheet.Cells[row, 1].Value = "CẤU TRÚC DỮ LIỆU";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Size = 14;

					row = 22;
					instructionSheet.Cells[row, 1].Value = "Thứ tự cột: Product (A-G) → ProductVariant (H-L) → Images (M-N)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;

					row = 24;
					instructionSheet.Cells[row, 1].Value = "PHẦN 1: PRODUCT (A-G)";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.UnderLine = true;

					row = 25;
					instructionSheet.Cells[row, 1].Value = "A. Tên SP* | B. Mô tả | C. Danh mục* | D. Màn hình | E. CPU | F. Camera | G. Pin";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 27;
					instructionSheet.Cells[row, 1].Value = "PHẦN 2: VARIANT (H-L)";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.UnderLine = true;

					row = 28;
					instructionSheet.Cells[row, 1].Value = "H. Màu* | I. RAM* | J. Bộ nhớ* | K. Giá* | L. Số lượng*";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 30;
					instructionSheet.Cells[row, 1].Value = "PHẦN 3: IMAGES (M-N) - CỰC KỲ QUAN TRỌNG";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.UnderLine = true;
					instructionSheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);

					row = 31;
					instructionSheet.Cells[row, 1].Value = "M. Ảnh Sản Phẩm - INSERT ẢNH vào cell này (hàng đầu tiên của Product)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 32;
					instructionSheet.Cells[row, 1].Value = "N. Ảnh Biến Thể - INSERT ẢNH vào cell này (mỗi hàng variant)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 34;
					instructionSheet.Cells[row, 1].Value = "CÁCH NHẬP BIẾN THỂ";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Size = 14;

					row = 35;
					instructionSheet.Cells[row, 1].Value = "- Hàng đầu: Điền đầy đủ Product (A-G) + Variant (H-L) + Ảnh (M, N)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 36;
					instructionSheet.Cells[row, 1].Value = "- Hàng tiếp (variant khác): BỎ TRỐNG Product (A-G, M), chỉ điền Variant (H-L, N)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;

					row = 37;
					instructionSheet.Cells[row, 1].Value = "- Khi có Tên mới (A) = Product mới";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 39;
					instructionSheet.Cells[row, 1].Value = "VÍ DỤ MINH HỌA";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Size = 14;

					row = 40;
					instructionSheet.Cells[row, 1].Value = "Hàng 2: Samsung A12 | Mô tả | ... | Đen | 4GB | 64GB | 3.5tr | 100 | [ẢNH PRODUCT] | [ẢNH ĐEN]";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 41;
					instructionSheet.Cells[row, 1].Value = "Hàng 3: [TRỐNG] | ... | Trắng | 4GB | 64GB | 3.5tr | 50 | [TRỐNG] | [ẢNH TRẮNG]";
					instructionSheet.Cells[row, 1].Style.WrapText = true;
					instructionSheet.Cells[row, 1].Style.Font.Italic = true;

					row = 43;
					instructionSheet.Cells[row, 1].Value = "TRƯỜNG BẮT BUỘC (*)";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Size = 14;

					row = 44;
					instructionSheet.Cells[row, 1].Value = "- Tên Sản Phẩm, Tên Danh Mục, Màu, RAM, Bộ Nhớ, Giá, Số Lượng";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 46;
					instructionSheet.Cells[row, 1].Value = "💡 MẸO HAY";
					instructionSheet.Cells[row, 1].Style.Font.Bold = true;
					instructionSheet.Cells[row, 1].Style.Font.Size = 14;

					row = 47;
					instructionSheet.Cells[row, 1].Value = "- Tăng chiều cao hàng để dễ xem ảnh (click chuột phải vào số hàng > Row Height > 100)";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 48;
					instructionSheet.Cells[row, 1].Value = "- Có thể chọn nhiều ảnh cùng lúc khi insert";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					row = 49;
					instructionSheet.Cells[row, 1].Value = "- Nếu không có ảnh, hệ thống sẽ dùng ảnh mặc định";
					instructionSheet.Cells[row, 1].Style.WrapText = true;

					instructionSheet.Column(1).Width = 120;

					// Save to byte array
					var excelBytes = await Task.Run(() => package.GetAsByteArray());

					return File(
						excelBytes,
						"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
						$"Template_Import_San_Pham_{DateTime.Now:ddMMyyyy_HHmm}.xlsx"
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