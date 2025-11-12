using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.ProductImport.Dtos;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.UploadFile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using OfficeOpenXml.Drawing; // Đảm bảo có using này
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductImport
{
	public class ProductImportExportAppService : ApplicationService, IProductImportExportAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<ProductVariant> _productVariantRepository;
		private readonly IRepository<ProductImage> _productImageRepository;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IUploadFileAppService _uploadFileAppService;
		private readonly IWebHostEnvironment _env;
		private readonly HttpClient _httpClient;

		public ProductImportExportAppService(
			IRepository<Product> productRepository,
			IRepository<ProductVariant> productVariantRepository,
			IRepository<ProductImage> productImageRepository,
			IRepository<Category> categoryRepository,
			IWebHostEnvironment env,
			IUploadFileAppService uploadFileAppService)
		{
			_env = env;
			_productRepository = productRepository;
			_productVariantRepository = productVariantRepository;
			_productImageRepository = productImageRepository;
			_categoryRepository = categoryRepository;
			_uploadFileAppService = uploadFileAppService;
			_httpClient = new HttpClient();
		}

		/// <summary>
		/// Xử lý dữ liệu từ các cột Sản phẩm (1-7) và Ảnh (13).
		/// </summary>
		//public async Task<ImportProductRowResult> ProcessProductRowAsync(ExcelWorksheet worksheet, int row)
		//{
		//	var result = new ImportProductRowResult();
		//	var errors = new List<string>();
		//	var productName = worksheet.Cells[row, 1].Text?.Trim();
		//	var categoryName = worksheet.Cells[row, 3].Text?.Trim();

		//	if (string.IsNullOrWhiteSpace(productName)) errors.Add("Tên Sản Phẩm (Name) là bắt buộc.");
		//	if (string.IsNullOrWhiteSpace(categoryName)) errors.Add("Tên Danh Mục (CategoryName) là bắt buộc.");

		//	var category = await _categoryRepository.FirstOrDefaultAsync(c => c.Name == categoryName);
		//	if (category == null) errors.Add($"Không tìm thấy Danh Mục '{categoryName}'.");

		//	if (errors.Any())
		//	{
		//		result.Errors = errors;
		//		result.IsSuccess = false;
		//		return result;
		//	}

		//	// Tìm check sản phẩm đã tồn tại
		//	var product = await _productRepository.FirstOrDefaultAsync(p => p.Name == productName);

		//	if (product == null)
		//	{
		//		product = new Product
		//		{
		//			Name = productName,
		//			Description = worksheet.Cells[row, 2].Text?.Trim(),
		//			CategoryId = category.Id,
		//			Screen = worksheet.Cells[row, 4].Text?.Trim(),
		//			Processor = worksheet.Cells[row, 5].Text?.Trim(),
		//			CameraSystem = worksheet.Cells[row, 6].Text?.Trim(),
		//			Battery = worksheet.Cells[row, 7].Text?.Trim()
		//		};
		//		product.Id = await _productRepository.InsertAndGetIdAsync(product);
		//	}
		//	else
		//	{
		//		product.Description = worksheet.Cells[row, 2].Text?.Trim();
		//		product.CategoryId = category.Id;
		//		product.Screen = worksheet.Cells[row, 4].Text?.Trim();
		//		product.Processor = worksheet.Cells[row, 5].Text?.Trim();
		//		product.CameraSystem = worksheet.Cells[row, 6].Text?.Trim();
		//		product.Battery = worksheet.Cells[row, 7].Text?.Trim();
		//		await _productRepository.UpdateAsync(product);
		//	}
		//	await _productImageRepository.DeleteAsync(x => x.ProductId == product.Id);

		//	// Xử lý ảnh 
		//	var imageUrls = new List<string>();

		//	// Duyệt qua các cột từ 13 trở đi
		//	int col = 13;
		//	int imageIndex = 0;
		//	while (col <= worksheet.Dimension.End.Column)
		//	{
		//		// Kiểm tra xem ô này có chứa ảnh không
		//		var picture = worksheet.Drawings
		//		.FirstOrDefault(p => p.From.Row == row - 1 && p.From.Column == col - 1);

		//		if (picture != null)
		//		{
		//			string uploadsFolder = Path.Combine(_env.WebRootPath, "img", "products", "general");
		//			Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có
		//			string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
		//			string uniqueGuid = Guid.NewGuid().ToString("N").Substring(0, 12); // Lấy 8 ký tự đầu

		//			if (picture is ExcelPicture excelPicture)
		//			{
		//				string extension = excelPicture.Image.Type.ToString().ToLower() switch
		//				{
		//					"jpeg" => ".jpg",
		//					"png" => ".png",
		//					"bmp" => ".bmp",
		//					_ => ".jpg"
		//				};
		//				string uniqueFileName = $"{timestamp}_{uniqueGuid}{extension}";
		//				string filePath = Path.Combine(uploadsFolder, uniqueFileName);

		//				using (var imageStream = new MemoryStream(excelPicture.Image.ImageBytes))
		//				using (var fileStream = new FileStream(filePath, FileMode.Create))
		//				{
		//					await imageStream.CopyToAsync(fileStream);
		//				}
		//				string relativePath = Path.Combine("img", "products", "general", uniqueFileName);
		//				var productImage = new ProductImage
		//				{
		//					ProductId = product.Id,
		//					ProductVariantId = null,
		//					ImageUrl = relativePath,
		//					SortOrder = 0,
		//					AltText = productName
		//				};
		//				await _productImageRepository.InsertAsync(productImage);
		//				imageUrls.Add(filePath); // Use filePath instead of undefined imageUrl
		//				imageIndex++;
		//			}
		//			//// Đảm bảo thư mục tồn tại
		//			//var directory = Path.GetDirectoryName(uploadsFolder);
		//			//if (!Directory.Exists(directory))
		//			//{
		//			//	Directory.CreateDirectory(directory);
		//			//}
		//		}
		//		col++; // Increment column to avoid infinite loop
		//	}
		//	result.ProductId = (int?)product.Id;
		//	result.ProductVariantId = null;
		//	result.IsSuccess = true;
		//	result.Errors = errors;

		//	return result;
		//}

		public async Task<ImportProductRowResult> ProcessProductRowAsync(ExcelWorksheet worksheet, int row)
		{
			var result = new ImportProductRowResult();
			var errors = new List<string>();
			var productName = worksheet.Cells[row, 1].Text?.Trim();
			var categoryName = worksheet.Cells[row, 3].Text?.Trim();

			if (string.IsNullOrWhiteSpace(productName)) errors.Add("Tên Sản Phẩm (Name) là bắt buộc.");
			if (string.IsNullOrWhiteSpace(categoryName)) errors.Add("Tên Danh Mục (CategoryName) là bắt buộc.");

			var category = await _categoryRepository.FirstOrDefaultAsync(c => c.Name == categoryName);
			if (category == null) errors.Add($"Không tìm thấy Danh Mục '{categoryName}'.");

			if (errors.Any())
			{
				result.Errors = errors;
				result.IsSuccess = false;
				return result;
			}

			// Tìm check sản phẩm đã tồn tại
			var existingProduct = await _productRepository.FirstOrDefaultAsync(p => p.Name == productName);
			bool isNewProduct = existingProduct == null;

			Product product;

			if (isNewProduct)
			{
				product = new Product
				{
					Name = productName,
					Description = worksheet.Cells[row, 2].Text?.Trim(),
					CategoryId = category.Id,
					Screen = worksheet.Cells[row, 4].Text?.Trim(),
					Processor = worksheet.Cells[row, 5].Text?.Trim(),
					CameraSystem = worksheet.Cells[row, 6].Text?.Trim(),
					Battery = worksheet.Cells[row, 7].Text?.Trim()
				};
				product.Id = await _productRepository.InsertAndGetIdAsync(product);
			}
			else
			{
				product = existingProduct;
				product.Description = worksheet.Cells[row, 2].Text?.Trim();
				product.CategoryId = category.Id;
				product.Screen = worksheet.Cells[row, 4].Text?.Trim();
				product.Processor = worksheet.Cells[row, 5].Text?.Trim();
				product.CameraSystem = worksheet.Cells[row, 6].Text?.Trim();
				product.Battery = worksheet.Cells[row, 7].Text?.Trim();
				await _productRepository.UpdateAsync(product);
			}

			await _productImageRepository.DeleteAsync(x => x.ProductId == product.Id && x.ProductVariantId == null);

			// Xử lý ảnh (giữ nguyên logic hiện tại)
			var imageUrls = new List<string>();
			int col = 13;
			int imageIndex = 0;
			while (col <= worksheet.Dimension.End.Column)
			{
				var picture = worksheet.Drawings.FirstOrDefault(p => p.From.Row == row - 1 && p.From.Column == col - 1);

				if (picture != null && picture is ExcelPicture excelPicture)
				{
					// Logic xử lý ảnh giữ nguyên
					string uploadsFolder = Path.Combine(_env.WebRootPath, "img", "products", "general");
					Directory.CreateDirectory(uploadsFolder);
					string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
					string uniqueGuid = Guid.NewGuid().ToString("N").Substring(0, 12);

					string extension = excelPicture.Image.Type.ToString().ToLower() switch
					{
						"jpeg" => ".jpg",
						"png" => ".png",
						"bmp" => ".bmp",
						_ => ".jpg"
					};

					string uniqueFileName = $"{timestamp}_{uniqueGuid}{extension}";
					string filePath = Path.Combine(uploadsFolder, uniqueFileName);

					using (var imageStream = new MemoryStream(excelPicture.Image.ImageBytes))
					using (var fileStream = new FileStream(filePath, FileMode.Create))
					{
						await imageStream.CopyToAsync(fileStream);
					}

					string relativePath = Path.Combine("img", "products", "general", uniqueFileName);
					var productImage = new ProductImage
					{
						ProductId = product.Id,
						ProductVariantId = null,
						ImageUrl = relativePath,
						SortOrder = imageIndex,
						AltText = productName
					};
					await _productImageRepository.InsertAsync(productImage);
					imageIndex++;
				}
				col++;
			}

			result.ProductId = product.Id;
			result.IsSuccess = true;
			result.Errors = errors;

			return result;
		}

		public async Task<ImportProductRowResult> ProcessVariantRowAsync(ExcelWorksheet worksheet, int row, int productId)
		{
			var result = new ImportProductRowResult();
			var errors = new List<string>();
			var color = worksheet.Cells[row, 8].Text?.Trim();
			var ram = worksheet.Cells[row, 9].Text?.Trim();
			var storage = worksheet.Cells[row, 10].Text?.Trim();

			// Nếu không có màu, nghĩa là dòng này không có dữ liệu biến thể
			if (string.IsNullOrWhiteSpace(color)) return new ImportProductRowResult();

			if (string.IsNullOrWhiteSpace(ram)) errors.Add("RAM là bắt buộc.");
			if (string.IsNullOrWhiteSpace(storage)) errors.Add("Bộ Nhớ (Storage) là bắt buộc.");

			if (!decimal.TryParse(worksheet.Cells[row, 11].Text?.Trim(), out decimal price)) errors.Add("Giá không hợp lệ.");
			if (!int.TryParse(worksheet.Cells[row, 12].Text?.Trim(), out int stock)) errors.Add("Số lượng không hợp lệ.");

			if (errors.Any())
			{
				result.Errors = errors;
				result.IsSuccess = false;
				return result;
			}

			// Tìm hoặc tạo mới Biến thể
			var existingVariant = await _productVariantRepository.FirstOrDefaultAsync(v =>
											v.ProductId == productId &&
											v.Color == color &&
											v.Ram == ram &&
											v.Storage == storage);

			bool isNewVariant = existingVariant == null;
			ProductVariant variant;

			if (isNewVariant)
			{
				variant = new ProductVariant();
				variant.ProductId = productId;
				variant.Color = color;
				variant.Ram = ram;
				variant.Storage = storage;
				variant.Price = price;
				variant.StockQuantity = stock;
				variant.Id = await _productVariantRepository.InsertAndGetIdAsync(variant);
			}
			else
			{
				variant = existingVariant;
				variant.ProductId = productId;
				variant.Color = color;
				variant.Ram = ram;
				variant.Storage = storage;
				variant.Price = price;
				variant.StockQuantity = stock;
				await _productVariantRepository.UpdateAsync(variant);
			}

			await _productImageRepository.DeleteAsync(x => x.ProductVariantId == variant.Id);

			// Xử lý ảnh variant (giữ nguyên logic hiện tại)
			int col = 13;
			int imageIndex = 0;
			var imageUrls = new List<string>();
			while (col <= worksheet.Dimension.End.Column)
			{
				var picture = worksheet.Drawings.FirstOrDefault(p => p.From.Row == row - 1 && p.From.Column == col - 1);

				if (picture != null && picture is ExcelPicture excelPicture)
				{
					// Logic xử lý ảnh giữ nguyên
					string uploadsFolder = Path.Combine(_env.WebRootPath, "img", "products", "variants");
					Directory.CreateDirectory(uploadsFolder);
					string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
					string uniqueGuid = Guid.NewGuid().ToString("N").Substring(0, 12);

					string extension = excelPicture.Image.Type.ToString().ToLower() switch
					{
						"jpeg" => ".jpg",
						"png" => ".png",
						"bmp" => ".bmp",
						_ => ".jpg"
					};

					string uniqueFileName = $"{timestamp}_{uniqueGuid}{extension}";
					string filePath = Path.Combine(uploadsFolder, uniqueFileName);

					using (var imageStream = new MemoryStream(excelPicture.Image.ImageBytes))
					using (var fileStream = new FileStream(filePath, FileMode.Create))
					{
						await imageStream.CopyToAsync(fileStream);
					}

					string relativePath = Path.Combine("img", "products", "variants", uniqueFileName);
					var productImage = new ProductImage
					{
						ProductId = productId,
						ProductVariantId = variant.Id,
						ImageUrl = relativePath,
						SortOrder = imageIndex,
						AltText = $"{color} variant image {imageIndex + 1}"
					};
					await _productImageRepository.InsertAsync(productImage);
					imageIndex++;
				}
				col++;
			}

			result.ProductId = productId;
			result.ProductVariantId = variant.Id;
			result.IsSuccess = true;
			result.Errors = errors;
			return result;
		}

		//public async Task<ImportProductRowResult> ProcessVariantRowAsync(ExcelWorksheet worksheet, int row, int productId)
		//{
		//	var result = new ImportProductRowResult();
		//	var errors = new List<string>();
		//	var color = worksheet.Cells[row, 8].Text?.Trim();
		//	var ram = worksheet.Cells[row, 9].Text?.Trim();
		//	var storage = worksheet.Cells[row, 10].Text?.Trim();

		//	// Nếu không có màu, nghĩa là dòng này không có dữ liệu biến thể
		//	if (string.IsNullOrWhiteSpace(color)) return new ImportProductRowResult();

		//	if (string.IsNullOrWhiteSpace(ram)) errors.Add("RAM là bắt buộc.");
		//	if (string.IsNullOrWhiteSpace(storage)) errors.Add("Bộ Nhớ (Storage) là bắt buộc.");

		//	if (!decimal.TryParse(worksheet.Cells[row, 11].Text?.Trim(), out decimal price)) errors.Add("Giá không hợp lệ.");
		//	if (!int.TryParse(worksheet.Cells[row, 12].Text?.Trim(), out int stock)) errors.Add("Số lượng không hợp lệ.");

		//	if (errors.Any())
		//	{
		//		result.Errors = errors;
		//		result.IsSuccess = false;
		//		return result;
		//	}

		//	// Tìm hoặc tạo mới Biến thể
		//	var variant = await _productVariantRepository.FirstOrDefaultAsync(v =>
		//					v.ProductId == productId &&
		//					v.Color == color &&
		//					v.Ram == ram &&
		//					v.Storage == storage);
		//	if (variant == null)
		//	{
		//		variant = new ProductVariant();
		//		variant.ProductId = productId;
		//		variant.Color = color;
		//		variant.Ram = ram;
		//		variant.Storage = storage;
		//		variant.Price = price;
		//		variant.StockQuantity = stock;
		//		variant.Id = await _productVariantRepository.InsertAndGetIdAsync(variant);
		//	}
		//	else
		//	{
		//		variant.ProductId = productId;
		//		variant.Color = color;
		//		variant.Ram = ram;
		//		variant.Storage = storage;
		//		variant.Price = price;
		//		variant.StockQuantity = stock;
		//		await _productVariantRepository.UpdateAsync(variant);
		//	}

		//	await _productImageRepository.DeleteAsync(x => x.ProductVariantId == variant.Id);

		//	// Duyệt qua các cột từ 13 trở đi
		//	int col = 13;
		//	int imageIndex = 0;
		//	var imageUrls = new List<string>();
		//	while (col <= worksheet.Dimension.End.Column)
		//	{
		//		// Kiểm tra xem ô này có chứa ảnh không
		//		var picture = worksheet.Drawings
		//				.FirstOrDefault(p => p.From.Row == row - 1 && p.From.Column == col - 1);

		//		if (picture != null)
		//		{
		//			string uploadsFolder = Path.Combine(_env.WebRootPath, "img", "products", "variants");
		//			Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có
		//			string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
		//			string uniqueGuid = Guid.NewGuid().ToString("N").Substring(0, 12); // Lấy 8 ký tự đầu

		//			if (picture is ExcelPicture excelPicture)
		//			{
		//				string extension = excelPicture.Image.Type.ToString().ToLower() switch
		//				{
		//					"jpeg" => ".jpg",
		//					"png" => ".png",
		//					"bmp" => ".bmp",
		//					_ => ".jpg"
		//				};
		//				string uniqueFileName = $"{timestamp}_{uniqueGuid}{extension}";
		//				string filePath = Path.Combine(uploadsFolder, uniqueFileName);

		//				using (var imageStream = new MemoryStream(excelPicture.Image.ImageBytes))
		//				using (var fileStream = new FileStream(filePath, FileMode.Create))
		//				{
		//					await imageStream.CopyToAsync(fileStream);
		//				}

		//				string relativePath = Path.Combine("img", "products", "variants", uniqueFileName);
		//				var productImage = new ProductImage
		//				{
		//					ProductId = productId,
		//					ProductVariantId = variant.Id,
		//					ImageUrl = relativePath,
		//					SortOrder = 0,
		//					AltText = $"{color} variant image {imageIndex + 1}"
		//				};
		//				await _productImageRepository.InsertAsync(productImage);
		//				imageUrls.Add(filePath); // Use filePath instead of undefined imageUrl
		//				imageIndex++;
		//			}
		//		}
		//		col++;
		//	}
		//	result.ProductId = productId;
		//	result.ProductVariantId = variant.Id;
		//	result.IsSuccess = true;
		//	result.Errors = errors;
		//	return result;
		//}

		private string GetImageExtension(ExcelDrawing picture)
		{
			if (picture is ExcelPicture excelPicture)
			{
				var imageBytes = excelPicture.Image.ImageBytes;
				using (var ms = new MemoryStream(imageBytes))
				{
					var image = Image.FromStream(ms);
					if (ImageFormat.Jpeg.Equals(image.RawFormat)) return ".jpg";
					if (ImageFormat.Png.Equals(image.RawFormat)) return ".png";
					if (ImageFormat.Gif.Equals(image.RawFormat)) return ".gif";
					if (ImageFormat.Bmp.Equals(image.RawFormat)) return ".bmp";
				}
			}
			return ".jpg";
		}
		// Export
		public async Task<ExportResult> ExportProducts(ExportProductInput input)
		{
			ExcelPackage.License.SetNonCommercialPersonal("ImportForDoAn");

			var result = new ExportResult();

			var query = _productRepository.GetAll();

			// THÊM FILTER CHO CATEGORY VÀ CREATION TIME
			if (input.CategoryId.HasValue && input.CategoryId.Value > 0)
			{
				query = query.Where(p => p.CategoryId == input.CategoryId.Value);
			}

			if (input.StartDate.HasValue && input.EndDate.HasValue)
			{
				query = query.Where(p => p.CreationTime >= input.StartDate.Value && p.CreationTime <= input.EndDate.Value);
			}

			var products = query.OrderBy(p => p.Name).ToList();

			if (!products.Any())
			{
				result.IsSuccess = false;
				result.ErrorMessage = "Không có sản phẩm nào để export.";
				return result;
			}

			// Get all related data
			var productIds = products.Select(p => p.Id).ToList();
			var variants = _productVariantRepository.GetAll()
							.Where(v => productIds.Contains(v.ProductId))
							.OrderBy(v => v.ProductId)
							.ThenBy(v => v.Color)
							.ToList();

			var allImages = _productImageRepository.GetAll().Where(i => productIds.Contains(i.ProductId)).ToList();

			var categories = _categoryRepository.GetAll().ToDictionary(c => c.Id, c => c.Name);

			// Create Excel package
			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Products");

				// Header row (ĐÃ THAY ĐỔI: "Ảnh" là cột 13, gộp 2 cột ảnh cũ)
				var headers = new[]
				{
												"Tên Sản Phẩm (Name)*", // 1
                        "Mô Tả (Description)", // 2
                        "Tên Danh Mục (CategoryName)*", // 3
                        "Màn Hình (Screen)", // 4
                        "Bộ Xử Lý (Processor)", // 5
                        "Camera (CameraSystem)", // 6
                        "Pin (Battery)", // 7
                        "Màu Biến Thể (Color)*", // 8
                        "RAM (Ram)*", // 9
                        "Bộ Nhớ (Storage)*", // 10
                        "Giá Biến Thể (VariantPrice)*", // 11
                        "Số Lượng Biến Thể (VariantStock)*", // 12
                        "Ảnh" // 13 (CỘT MỚI)
                };

				// Write header
				for (int col = 1; col <= headers.Length; col++)
				{
					var cell = worksheet.Cells[1, col];
					cell.Value = headers[col - 1];
					cell.Style.Font.Bold = true;
					cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
					cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
					cell.Style.Font.Color.SetColor(Color.White);
					cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
					cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
					cell.Style.WrapText = true;
				}

				worksheet.Cells[1, 13].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 192, 0));

				int currentRow = 2;
				int totalProducts = 0;
				int totalVariants = 0;

				// Write data
				foreach (var product in products)
				{
					totalProducts++;

					var categoryName = categories.ContainsKey(product.CategoryId ?? 0)
									? categories[product.CategoryId ?? 0]
									: "";
					var productImages = allImages
									.Where(i => i.ProductId == product.Id && i.ProductVariantId == null)
									.ToList();

					worksheet.Row(currentRow).Height = 80;

					worksheet.Cells[currentRow, 1].Value = product.Name;
					worksheet.Cells[currentRow, 2].Value = product.Description;
					worksheet.Cells[currentRow, 3].Value = categoryName;
					worksheet.Cells[currentRow, 4].Value = product.Screen;
					worksheet.Cells[currentRow, 5].Value = product.Processor;
					worksheet.Cells[currentRow, 6].Value = product.CameraSystem;
					worksheet.Cells[currentRow, 7].Value = product.Battery;
					worksheet.Cells[currentRow, 8].Value = "";
					worksheet.Cells[currentRow, 9].Value = "";
					worksheet.Cells[currentRow, 10].Value = "";
					worksheet.Cells[currentRow, 11].Value = "";
					worksheet.Cells[currentRow, 12].Value = "";

					await AddImagesToCell(worksheet, currentRow, 13, productImages);

					currentRow++;

					var productVariants = variants.Where(v => v.ProductId == product.Id).ToList();

					if (productVariants.Any())
					{
						foreach (var variant in productVariants)
						{
							totalVariants++;

							worksheet.Row(currentRow).Height = 80;

							worksheet.Cells[currentRow, 1].Value = "";
							worksheet.Cells[currentRow, 2].Value = "";
							worksheet.Cells[currentRow, 3].Value = "";
							worksheet.Cells[currentRow, 4].Value = "";
							worksheet.Cells[currentRow, 5].Value = "";
							worksheet.Cells[currentRow, 6].Value = "";
							worksheet.Cells[currentRow, 7].Value = "";
							worksheet.Cells[currentRow, 8].Value = variant.Color;
							worksheet.Cells[currentRow, 9].Value = variant.Ram;
							worksheet.Cells[currentRow, 10].Value = variant.Storage;
							worksheet.Cells[currentRow, 11].Value = variant.Price;
							worksheet.Cells[currentRow, 12].Value = variant.StockQuantity;

							var variantImages = allImages
											.Where(i => i.ProductVariantId == variant.Id)
											.ToList();

							await AddImagesToCell(worksheet, currentRow, 13, variantImages);

							currentRow++;
						}
					}
					else
					{
						worksheet.Row(currentRow).Height = 30;
						worksheet.Cells[currentRow, 8].Value = "Không có biến thể";
						worksheet.Cells[currentRow, 8].Style.Font.Italic = true;
						worksheet.Cells[currentRow, 8].Style.Font.Color.SetColor(Color.Gray);
						currentRow++;
					}
				}

				var stream = new MemoryStream();
				package.SaveAs(stream);
				stream.Position = 0;

				result.IsSuccess = true;
				result.FileContent = stream.ToArray();
				result.FileName = $"Products_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
				result.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				result.Message = $"Đã export thành công {totalProducts} sản phẩm với {totalVariants} biến thể.";
				result.TotalProducts = totalProducts;
				result.TotalVariants = totalVariants;
			}

			return result;
		}

		private async Task AddImagesToCell(ExcelWorksheet worksheet, int row, int startCol, List<ProductImage> images)
		{
			if (images?.Any() != true) return;

			int currentCol = startCol;
			foreach (var image in images.Where(img => !string.IsNullOrEmpty(img.ImageUrl) && img.ImageUrl != "/img/products/default.png"))
			{
				var imageBytes = await GetImageBytesAsync(image.ImageUrl);
				if (imageBytes?.Length > 0)
				{
					using var stream = new MemoryStream(imageBytes);
					var picture = worksheet.Drawings.AddPicture($"Img_{row}_{currentCol}", stream);

					picture.From.Column = currentCol - 1;
					picture.From.Row = row - 1;
					picture.From.ColumnOff = 5 * 9525;
					picture.From.RowOff = 5 * 9525;

					picture.SetSize(80, 80);

					// Đặt độ rộng cột cho ảnh này
					worksheet.Column(currentCol).Width = 15;
				}

				currentCol++; // CHUYỂN SANG CỘT TIẾP THEO CHO ẢNH KẾ TIẾP
			}
		}

		private async Task<byte[]> GetImageBytesAsync(string imageUrl)
		{
			// 1. Kiểm tra đầu vào
			if (string.IsNullOrWhiteSpace(imageUrl))
			{
				return null;
			}

			try
			{
				var relativePath = imageUrl.Split('?')[0].TrimStart('/', '\\');

				// 3. (FIX QUAN TRỌNG)
				// Chuẩn hóa dấu gạch chéo cho đúng với hệ điều hành
				// Nó sẽ đổi tất cả '/' hoặc '\' thành Path.DirectorySeparatorChar
				// (trên Windows là '\', trên Linux là '/')
				var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar)
																				 .Replace('\\', Path.DirectorySeparatorChar);

				// 4. Kết hợp đường dẫn an toàn
				var physicalPath = Path.Combine(_env.WebRootPath, normalizedPath);

				// 5. Kiểm tra và đọc file
				if (File.Exists(physicalPath))
				{
					return await File.ReadAllBytesAsync(physicalPath);
				}
				else
				{
					// Có thể thêm log ở đây để debug nếu cần
					// _logger.LogWarning($"Không tìm thấy file ảnh tại: {physicalPath}");
					return null;
				}
			}
			catch (Exception ex)
			{
				// Bạn nên log lỗi ra thay vì "catch" rỗng
				// _logger.LogError(ex, $"Lỗi khi đọc file ảnh: {imageUrl}");
				return null;
			}
		}

		private string GetFileNameHash(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				return "noname";
			}

			// Chỉ lấy các ký tự alphanumeric
			string cleaned = new string(fileName.Where(c => char.IsLetterOrDigit(c)).ToArray());

			if (cleaned.Length > 10)
			{
				cleaned = cleaned.Substring(0, 10);
			}

			// Tạo hash code từ chuỗi
			int hashCode = fileName.GetHashCode();
			string hash = Math.Abs(hashCode).ToString("X").Substring(0, Math.Min(6, Math.Abs(hashCode).ToString("X").Length));

			return $"{cleaned}_{hash}".ToLower();
		}

	}


	public class ExportResult
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public byte[] FileContent { get; set; }
		public string FileName { get; set; }
		public string ContentType { get; set; }
		public int TotalProducts { get; set; }
		public int TotalVariants { get; set; }
		public string ErrorMessage { get; set; }
		public string ErrorDetails { get; set; }
	}
	public class ProductImportItem
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string CategoryName { get; set; }
		public string Screen { get; set; }
		public string Processor { get; set; }
		public string CameraSystem { get; set; }
		public string Battery { get; set; }
		public List<byte[]> MainImages { get; set; } = new();
		public List<ProductVariantImportItem> Variants { get; set; } = new();
	}

	public class ProductVariantImportItem
	{
		public string Color { get; set; }
		public string Ram { get; set; }
		public string Storage { get; set; }
		public decimal Price { get; set; }
		public int StockQuantity { get; set; }
		public List<byte[]> Images { get; set; } = new();
	}

	// Fake IFormFile (Không đổi)
	public class FakeFormFile : IFormFile
	{
		private readonly Stream _baseStream;
		private readonly string _fileName;

		public FakeFormFile(Stream baseStream, string fileName)
		{
			_baseStream = baseStream;
			_fileName = fileName;
		}

		public string ContentType => GetContentType(_fileName);
		public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{_fileName}\"";
		public IHeaderDictionary Headers => new HeaderDictionary();
		public long Length => _baseStream.Length;
		public string Name => "file";
		public string FileName => _fileName;

		public void CopyTo(Stream target) => _baseStream.CopyTo(target);
		public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
			=> await _baseStream.CopyToAsync(target, cancellationToken);
		public Stream OpenReadStream() => _baseStream;

		private string GetContentType(string fileName)
		{
			var ext = Path.GetExtension(fileName).ToLower();
			return ext switch
			{
				".jpg" or ".jpeg" => "image/jpeg",
				".png" => "image/png",
				".gif" => "image/gif",
				".webp" => "image/webp",
				_ => "application/octet-stream"
			};
		}
	}
}