using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.Tours;
using Acme.SimpleTaskApp.Tours.Dtos;
using Acme.SimpleTaskApp.Web.Models.Products;
using Acme.SimpleTaskApp.Web.Models.Tours;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.UI;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class ToursController : SimpleTaskAppControllerBase
	{
		private readonly ITourAppService _tourAppService;
		private readonly IWebHostEnvironment webHostEnvironment;
		public ToursController(ITourAppService tourAppService, IWebHostEnvironment webHostEnvironment)
		{
			this.webHostEnvironment = webHostEnvironment;
			_tourAppService = tourAppService;
		}
		public async Task<ActionResult> Index(GetAllTourInput input)
		{
			var output = await _tourAppService.GetAllTour(input);

			var model = new TourViewModel(output.Items)
			{

			};

			return View(model);
		}

		public async Task<PartialViewResult> EditTour(long tourId)
		{
			try
			{

				// Gọi service để lấy sản phẩm theo Id
				var tour = await _tourAppService.GetByIdTour(new EntityDto<long>(tourId));
				var editTourDto = new UpdateTourInput
				{
					Id = tour.Id,
					TourName = tour.TourName,
					MinGroupSize = tour.MinGroupSize,
					MaxGroupSize = tour.MaxGroupSize,
					TourTypeCid = tour.TourTypeCid,
					StartDate = tour.StartDate,
					EndDate = tour.EndDate,
					Transportation = tour.Transportation,
					TourPrice = tour.TourPrice,
					PhoneNumber = tour.PhoneNumber,
					Description = tour.Description,
					Attachment = tour.Attachment

				};
				// Chuyển đổi CategoryListDto sang SelectListItem   
				
				var viewModel = new EditTourViewModel
				{
					Tours = editTourDto,
				};

				return PartialView("_EditTourModal", viewModel);
			}
			catch (UserFriendlyException ex)
			{
				// Log lỗi và throw exception để client nhận thông báo
				Logger.Error(ex.Message, ex);
				throw;
			}
		}

		[HttpPost]
		public async Task<IActionResult> UpdateTour(UpdateTourInput model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var existingTour = await _tourAppService.GetByIdTour(new EntityDto<long>(model.Id));
					if (existingTour == null)
					{
						return Json(new { success = false, message = "Không tìm thấy Tour." }); // Trả về lỗi nếu không tìm thấy
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
						var oldImagePath = existingTour.Attachment;
						string uniqueFileName = UploadImage(model.ImageFile);
						model.Attachment = uniqueFileName;

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

					await _tourAppService.UpdateTour(model);
					return Json(new { success = true, message = "Cập nhật Tour thành công" });
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
		public async Task<IActionResult> Create(CreateTourInput model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					// Upload ảnh và lấy tên file duy nhất
					string uniqueFileName = UploadImage(model.ImageFile);

					// Gán đường dẫn file vào model
					model.Attachment = uniqueFileName;
					await _tourAppService.CreateTour(model);
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
		public async Task Delete(EntityDto<long> input)
		{
			var tour = await _tourAppService.GetByIdTour(input);
			if (tour == null)
			{
				throw new UserFriendlyException("Không tìm thấy sản phẩm");
			}

			// Xóa ảnh từ thư mục
			DeleteFile(tour.Attachment);

			await _tourAppService.DeleteTour(new EntityDto<long> { Id = tour.Id });
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

				string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "img/tours/");
				Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có

				string uniqueFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N") + fileExtension;
				string filePath = Path.Combine(uploadsFolder, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					ImageFile.CopyTo(fileStream);
				}

				return "/img/tours/" + uniqueFileName;
			}

			return "/img/tours/default.png"; // Trả về ảnh mặc định nếu không có ảnh upload
		}

		private void DeleteFile(string imagePath)
		{
			if (imagePath != null && !imagePath.Equals("/img/tours/default.png"))
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
