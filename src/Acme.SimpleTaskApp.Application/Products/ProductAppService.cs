using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using System;
using static Acme.SimpleTaskApp.Products.Product;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml.Drawing;
using OfficeOpenXml;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using Abp.Extensions;
//using Abp.Authorization;
//using Acme.SimpleTaskApp.Authorization;
namespace Acme.SimpleTaskApp.Products
{
	//[AbpAuthorize]
	public class ProductAppService : ApplicationService, IProductAppService
	{
		private readonly IRepository<Product> _productRepository;
		//private readonly IRepository<Category> _categoryRepository;
		private readonly IWebHostEnvironment _env;

		public ProductAppService(IRepository<Product> productRepository,
															 IWebHostEnvironment env) // IRepository<Category> categoryRepository,
		{
			_productRepository = productRepository;
			//_categoryRepository = categoryRepository;
			_env = env;
		}

		//[AbpAuthorize(PermissionNames.Pages_products)]
		public async Task<PagedResultDto<ProductListDto>> GetAllProducts(GetAllProductsInput input)
		{
			var products = _productRepository.GetAll();

			var count = await products.CountAsync();

			var productDtos = await products.OrderByDescending(x => x.CreationTime)
																			.PageBy(input)
																			.Select(p => new ProductListDto
																			{
																				Id = p.Id,
																				Name = p.Name,
																				Description = p.Description,
																				Price = p.Price,
																				State = p.State,
																				CreationTime = p.CreationTime,
																				Image = p.Image
																			}).ToListAsync();

			return new PagedResultDto<ProductListDto>(count, productDtos);
		}


		//[AbpAuthorize(PermissionNames.Pages_products_create)]
		public async Task<ProductListDto> CreateProducts(CreateProductDto input)
		{
			var product = new Product
			{
				Name = input.Name,
				Description = input.Description,
				Price = input.Price,
				State = input.State,
				CategoryId = input.CategoryId,
				Image = input.Image // Lưu đường dẫn ảnh
			};

			await _productRepository.InsertAsync(product);
			await CurrentUnitOfWork.SaveChangesAsync();

			return new ProductListDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				State = product.State,
				CreationTime = product.CreationTime,
				Image = product.Image
			};
		}


		//[AbpAuthorize(PermissionNames.Pages_products_delete)]
		public async Task DeleteProducts(EntityDto<int> input)
		{
			// Xóa sản phẩm
			await _productRepository.DeleteAsync(input.Id);
		}

		public async Task<ProductListDto> GetByIdProducts(EntityDto<int> input)
		{
			// Lấy sản phẩm theo Id
			var product = await _productRepository.GetAsync(input.Id);
			if (product == null)
			{
				throw new UserFriendlyException("Product not found!");
			}
			else
			{
				// Ánh xạ sang DTO
				return new ProductListDto
				{
					Id = product.Id,
					Name = product.Name,
					Description = product.Description,
					Price = product.Price,
					State = product.State,
					CreationTime = product.CreationTime,
					Image = product.Image,
					CategoryId = product.CategoryId
				};
			}
		}

		//[AbpAuthorize(PermissionNames.Pages_products_update)]
		public async Task<ProductListDto> UpdateProducts(UpdateProductDto input)
		{
			// Lấy sản phẩm hiện có
			var product = await _productRepository.GetAsync(input.Id);
			// đang bị lỗi nếu product = null thì sẽ không trả ra exception và dư 1 ít code đoạn đầu ở controller
			if (product == null)
			{
				throw new Exception("Sản phẩm không tồn tại!");
			}

			// Cập nhật thông tin (trừ Image)
			product.Name = input.Name;
			product.Description = input.Description;
			product.Price = input.Price;
			product.State = input.State;
			product.CategoryId = input.CategoryId;

			// Chỉ cập nhật Image nếu có ảnh mới
			if (!string.IsNullOrEmpty(input.Image))
			{
				product.Image = input.Image;
			}

			// Lưu thay đổi vào database
			await _productRepository.UpdateAsync(product);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Ánh xạ sang DTO để trả về
			return new ProductListDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				State = product.State,
				CreationTime = product.CreationTime,
				Image = product.Image, // Giữ nguyên ảnh cũ nếu không cập nhật
				CategoryId = product.CategoryId
			};
		}


		//[AbpAuthorize(PermissionNames.Pages_products_search)]
		public async Task<PagedResultDto<ProductListDto>> SearchProducts(GetAllProductsInput input)
		{
			var productQuery = _productRepository.GetAll();
			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				string keywordLower = input.Keyword.ToLower();
				productQuery = productQuery.Where(p => p.Name.ToLower().Contains(keywordLower));
			}

			if (!string.IsNullOrWhiteSpace(input.Category))
			{
				int categoryId = Convert.ToInt32(input.Category);
				productQuery = productQuery.Where(x => x.CategoryId == categoryId);
			}
			if (!string.IsNullOrWhiteSpace(input.StateInput) && Enum.TryParse<ProductState>(input.StateInput, out var state))
			{
				productQuery = productQuery.Where(x => x.State == state);
			}
			var count = await productQuery.CountAsync();

			var productDtos = await productQuery.OrderByDescending(p => p.CreationTime)
																			.PageBy(input)
																			.Select(p => new ProductListDto
																			{
																				Id = p.Id,
																				Name = p.Name,
																				Description = p.Description,
																				Price = p.Price,
																				State = p.State,
																				CreationTime = p.CreationTime,
																				Image = p.Image
																			}).ToListAsync();

			return new PagedResultDto<ProductListDto>(count, productDtos);

			
		}

		public async Task<byte[]> ExportProductsToExcel(GetAllProductsInput input)
		{
			var query = _productRepository.GetAll()
								.Include(x => x.Categories)
								.WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
												 x => x.Name.ToLower().Contains(input.Keyword.ToLower()));

			if (int.TryParse(input.Category, out var categoryId))
			{
				query = query.Where(x => x.CategoryId == categoryId);
			}

			if (input.State != null)
			{
				query = query.Where(x => x.State == input.State.Value);
			}

			var products = await query
											.OrderByDescending(x => x.CreationTime)
											.ToListAsync();



			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Products");

				// Tiêu đề cột
				worksheet.Cells[1, 1].Value = "Tên sản phẩm";
				worksheet.Cells[1, 2].Value = "Mô tả";
				worksheet.Cells[1, 3].Value = "Thời gian tạo";
				worksheet.Cells[1, 4].Value = "Trạng thái";
				worksheet.Cells[1, 5].Value = "Giá";
				worksheet.Cells[1, 6].Value = "Danh mục";
				worksheet.Cells[1, 7].Value = "Ảnh sản phẩm";
				int row = 2; // bắt đầu từ dòng 2
				int i = 1;   // chỉ số ảnh
										 // Đổ dữ liệu
				foreach (var item in products) {

					

					worksheet.Cells[row, 1].Value = item.Name;
					worksheet.Cells[row, 2].Value = item.Description;
					worksheet.Cells[row, 3].Value = item.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
					worksheet.Cells[row, 4].Value = item.State.ToString();
					worksheet.Cells[row, 5].Value = item.Price; // nếu có
					worksheet.Cells[row, 6].Value = item.Categories.Name;

					// Thêm ảnh nếu có
					// Thêm ảnh nếu có
					if (!string.IsNullOrEmpty(item.Image))
					{
						try
						{
							var imagePath = Path.Combine(@"/img/products/", Path.GetFileName(item.Image));
							if (File.Exists(imagePath))
							{
								using var image = System.Drawing.Image.FromFile(imagePath);
								using var imageStream = new MemoryStream();
								image.Save(imageStream, image.RawFormat);
								imageStream.Position = 0;

								var picture = worksheet.Drawings.AddPicture($"img_{i}", imageStream);
								picture.SetPosition(row - 1, 0, 10, 0);
								picture.SetSize(100, 100);
							}
						}
						catch
						{
							// Bỏ qua nếu không load được ảnh
						}
					}
					row++;
					i++;
				}

				// AutoFit columns
				worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

				return package.GetAsByteArray();
			}
		}
		//public async Task<List<ImportProductResultDto>> ImportProductsFromExcel(IFormFile file)
		//{
		//	ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

		//	var results = new List<ImportProductResultDto>();
		//	var uploadsFolder = @"E:\UploadImgKho\";

		//	// Kiểm tra và tạo thư mục
		//	Directory.CreateDirectory(uploadsFolder);

		//	// Validate file
		//	if (file == null || file.Length == 0)
		//		throw new Exception("File không tồn tại");

		//	if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
		//		throw new Exception("Chỉ hỗ trợ file Excel (.xlsx)");

		//	using (var stream = new MemoryStream())
		//	{
		//		await file.CopyToAsync(stream);

		//		using (var package = new ExcelPackage(stream))
		//		{
		//			if (package.Workbook.Worksheets.Count == 0)
		//				throw new Exception("File Excel không có sheet nào");

		//			var worksheet = package.Workbook.Worksheets[0];
		//			int rowCount = worksheet.Dimension?.Rows ?? 0;

		//			for (int row = 2; row <= rowCount; row++)
		//			{
		//				var result = new ImportProductResultDto
		//				{
		//					RowNumber = row,
		//					Code = worksheet.Cells[row, 1]?.Text?.Trim(),
		//					Name = worksheet.Cells[row, 2]?.Text?.Trim()
		//				};

		//				try
		//				{
		//					var existingProduct = await _productRepository.FirstOrDefaultAsync(p => p.Code == result.Code);
		//					if (existingProduct != null)
		//					{
		//						// Cập nhật thông tin sản phẩm
		//						existingProduct.Name = worksheet.Cells[row, 2]?.Text?.Trim();
		//						// ... các thông tin khác

		//						await _productRepository.UpdateAsync(existingProduct);
		//						result.IsSuccess = true;
		//						result.Message = "Cập nhật sản phẩm thành công";
		//						results.Add(result);
		//						continue;
		//					}
		//					string categoryName = worksheet.Cells[row, 4]?.Text?.Trim();
		//					string supplierName = worksheet.Cells[row, 9]?.Text?.Trim();
		//					var category = getAllCategories
		//									.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

		//					if (category == null)
		//						throw new Exception($"Không tìm thấy danh mục: {categoryName}");

		//					var supplier = getAllSuppliers
		//							.FirstOrDefault(s => s.Name.Equals(supplierName, StringComparison.OrdinalIgnoreCase));

		//					if (supplier == null)
		//						throw new Exception($"Không tìm thấy nhà cung cấp: {supplierName}");
		//					var productDto = new CreateProductDto
		//					{
		//						Code = result.Code,
		//						Name = result.Name,
		//						CategoryId = category.Id,
		//						SupplierId = supplier.Id,
		//						Description = worksheet.Cells[row, 3]?.Text?.Trim(),
		//						Barcode = worksheet.Cells[row, 5]?.Text?.Trim(),
		//						Unit = worksheet.Cells[row, 6]?.Text?.Trim(),
		//						Weight = decimal.TryParse(worksheet.Cells[row, 7]?.Text, out var weight) ? weight : 0,
		//						Volume = decimal.TryParse(worksheet.Cells[row, 8]?.Text, out var volume) ? volume : 0,
		//						//IsActive = worksheet.Cells[row, 10]?.Text?.Trim().Equals("Có", StringComparison.OrdinalIgnoreCase) ?? false
		//						IsActive = bool.TryParse(worksheet.Cells[row, 10]?.Text, out var isActive) && isActive
		//					};

		//					// Xử lý ảnh
		//					//foreach (var drawing in worksheet.Drawings)
		//					//{
		//					//	if (drawing is ExcelPicture picture)
		//					//	{
		//					//		var imageName = $"{Guid.NewGuid()}.jpg"; // Mặc định là jpg
		//					//		var imagePath = Path.Combine(uploadsFolder, imageName);

		//					//		// Lưu trực tiếp bytes ra file
		//					//		File.WriteAllBytes(imagePath, picture.Image.ImageBytes);

		//					//		productDto.Image = $"/products/{imageName}";
		//					//	}
		//					//}
		//					foreach (var drawing in worksheet.Drawings)
		//					{
		//						if (drawing is ExcelPicture picture)
		//						{
		//							// Ánh xạ hình vào đúng dòng
		//							if (picture.From.Row + 1 == row)
		//							{
		//								var imageName = $"{Guid.NewGuid()}.jpg";
		//								var imagePath = Path.Combine(uploadsFolder, imageName);

		//								File.WriteAllBytes(imagePath, picture.Image.ImageBytes);
		//								productDto.Image = $"/products/{imageName}";
		//							}
		//						}
		//					}
		//					await Create(productDto);
		//					result.IsSuccess = true;
		//					result.Message = "Thành công";
		//				}
		//				catch (Exception ex)
		//				{
		//					result.IsSuccess = false;
		//					result.Message = $"Dòng {row}: {ex.Message}";
		//				}

		//				results.Add(result);
		//			}
		//		}
		//	}

		//	return results;
		//}
	}
}
