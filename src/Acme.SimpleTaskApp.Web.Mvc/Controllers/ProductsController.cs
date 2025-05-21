using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.Web.Models.Products;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Http;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Abp.UI;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Abp.AspNetCore.Mvc.Authorization;


namespace Acme.SimpleTaskApp.Web.Controllers
{
	[AbpMvcAuthorize]
	public class ProductsController : SimpleTaskAppControllerBase
	{
		private readonly IProductAppService _productAppService;
		private readonly IWebHostEnvironment webHostEnvironment;
		private readonly ICategoryAppService _categoryAppService;
		public ProductsController(IProductAppService productAppService,
															ICategoryAppService categoryAppService,
															IWebHostEnvironment webHostEnvironment)
		{
			_productAppService = productAppService;
			_categoryAppService = categoryAppService;
			this.webHostEnvironment = webHostEnvironment;
		}
		[AbpMvcAuthorize]
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
		public async Task<FileResult> ExportToExcel([FromBody] GetAllProductsInput input)
		{
			var excelBytes = await _productAppService.ExportProductsToExcel(input);
			return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Danh_sach_san_pham.xlsx");
		}
		private string UploadImage(IFormFile ImageFile)
		{
			if (ImageFile != null && ImageFile.Length > 0)
			{
				// Kiểm tra định dạng ảnh
				string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
				string fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();
				if (!allowedExtensions.Contains(fileExtension))
				{
					throw new ArgumentException("Định dạng ảnh không hợp lệ. Vui lòng chọn ảnh có định dạng hợp lệ.");
				}

				string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "img/products/");
				Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có

				string uniqueFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N") + fileExtension;
				string filePath = Path.Combine(uploadsFolder, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					ImageFile.CopyTo(fileStream);
				}

				return "/img/products/" + uniqueFileName;
			}

			return "/img/products/default.png"; // Trả về ảnh mặc định nếu không có ảnh upload
		}

		[AbpMvcAuthorize]
		[HttpPost]
		public async Task<IActionResult> Create(CreateProductDto model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					// Upload ảnh và lấy tên file duy nhất
					string uniqueFileName = UploadImage(model.ImageFile);

					// Gán đường dẫn file vào model
					model.Image = uniqueFileName;
					await _productAppService.CreateProducts(model);
					return Json(new { success = true, message = "Thêm sản phẩm thành công" });
				}

				var errors = ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage)
						.ToList();

				return Json(new { success = false, errors });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		[HttpPost]
		[AbpMvcAuthorize]
		[Route("Products/EditModal")] // Đảm bảo route đúng
		public async Task<PartialViewResult> EditModal(int productId)
		{
			try
			{

				// Gọi service để lấy sản phẩm theo Id
				var product = await _productAppService.GetByIdProducts(new EntityDto<int>(productId));
				var categories = await _categoryAppService.GetAllCategories(new GetAllCategoryDto());
				var editProductDto = new UpdateProductDto
				{
					Id = product.Id,
					Name = product.Name,
					Description = product.Description,
					Price = product.Price,
					Image = product.Image,
					State = product.State,
					CreationTime = product.CreationTime,
					CategoryId = product.CategoryId

				};
				// Chuyển đổi CategoryListDto sang SelectListItem   
				var categoriesSelectList = categories.Items
					.Select(c => new SelectListItem
					{
						Value = c.Id.ToString(),
						Text = c.Name
					})
					.ToList();
				var viewModel = new EditProductModalViewModel
				{
					Product = editProductDto,
					Categories = categoriesSelectList // Thêm danh sách categories vào view model
				};

				return PartialView("_EditProductModal", viewModel);
			}
			catch (UserFriendlyException ex)
			{
				// Log lỗi và throw exception để client nhận thông báo
				Logger.Error(ex.Message, ex);
				throw;
			}
		}

		//. UpdateProductDto model đamg chuyển image null chưa lấy được img sẵn có
		[AbpMvcAuthorize]
		[HttpPost]
		public async Task<IActionResult> Update(UpdateProductDto model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var existingProduct = await _productAppService.GetByIdProducts(new EntityDto<int>(model.Id));
					if (existingProduct == null)
					{
						return Json(new { success = false, message = "Không tìm thấy sản phẩm." }); // Trả về lỗi nếu không tìm thấy
					}

					// Nếu có upload ảnh mới
					if (model.ImageFile != null && model.ImageFile.Length > 0)
					{
						// Danh sách các định dạng ảnh được phép tải lên
						string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".jfif" };
						string fileExtension = Path.GetExtension(model.ImageFile.FileName).ToLower();

						// Kiểm tra xem ảnh có thuộc định dạng hợp lệ không
						if (!allowedExtensions.Contains(fileExtension))
						{
							return Json(new { success = false, message = "Định dạng ảnh không hợp lệ. Vui lòng chọn file .jpg, .png, .gif." });
						}

						// Upload ảnh mới và xóa ảnh cũ (nếu không phải ảnh mặc định)
						var oldImagePath = existingProduct.Image;
						string uniqueFileName = UploadImage(model.ImageFile);
						model.Image = uniqueFileName;

						// xóa ảnh cũ
						if (oldImagePath != null && !oldImagePath.Equals("/img/products/default.png"))
						{
							string oldImageFullPath = Path.Combine(webHostEnvironment.WebRootPath, oldImagePath.TrimStart('/'));
							if (System.IO.File.Exists(oldImageFullPath))
							{
								System.IO.File.Delete(oldImageFullPath);
							}
						}
					}

					await _productAppService.UpdateProducts(model);
					return Json(new { success = true, message = "Cập nhật sản phẩm thành công" });
				}

				var errors = ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage)
						.ToList();

				return Json(new { success = false, errors });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		[HttpGet]
		[Route("Products/DetailModal")]
		public async Task<PartialViewResult> DetailProduct(int productId)
		{
			try
			{
				var product = await _productAppService.GetByIdProducts(new EntityDto<int>(productId));
				string categoryName = "chưa có danh mục"; // Giá trị mặc định

				// Chỉ lấy category nếu CategoryId != null
				if (product.CategoryId != null)
				{
					var category = await _categoryAppService.GetByIdCategory(new EntityDto<int>(product.CategoryId.Value));
					categoryName = category?.Name ?? "chưa có danh mục"; // Xử lý cả trường hợp category null
				}

				var detailProductDto = new ProductListDto
				{
					Id = product.Id,
					Name = product.Name,
					Description = product.Description,
					Price = product.Price,
					Image = product.Image,
					State = product.State,
					CreationTime = product.CreationTime,
					CategoryId = product.CategoryId,
					CategoryName = categoryName // Sử dụng giá trị đã xử lý
				};


				var viewModel = new DetailProductModalViewModel(detailProductDto);
				return PartialView("_DetailProductModal", viewModel);
			}
			catch (UserFriendlyException ex)
			{
				Logger.Error(ex.Message, ex);
				throw;
			}
		}

		//// làm xóa ảnh
		[AbpMvcAuthorize]
		[HttpPost]
		public async Task Delete(EntityDto<int> input)
		{
			var product = await _productAppService.GetByIdProducts(input);
			if (product == null)
			{
				throw new UserFriendlyException("Không tìm thấy sản phẩm");
			}

			// Xóa ảnh từ thư mục
			DeleteFile(product.Image);

			await _productAppService.DeleteProducts(product);
		}

		private void DeleteFile(string imagePath)
		{
			if (imagePath != null && !imagePath.Equals("/img/products/default.png"))
			{
				string fullPath = Path.Combine(webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
				if (System.IO.File.Exists(fullPath))
				{
					System.IO.File.Delete(fullPath);
				}
			}
		}

	}
}
