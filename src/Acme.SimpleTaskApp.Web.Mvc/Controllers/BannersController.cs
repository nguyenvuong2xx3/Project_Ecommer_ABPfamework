using Acme.SimpleTaskApp.Banners;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.UploadFile;
using Acme.SimpleTaskApp.Web.Models.Banners;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class BannersController : SimpleTaskAppControllerBase
	{
		private readonly IBannerAppService _bannerAppService;
		private readonly IUploadFileAppService _uploadFileAppService;

		public BannersController(
			IBannerAppService bannerAppService,
			IUploadFileAppService uploadFileAppService)
		{
			_bannerAppService = bannerAppService;
			_uploadFileAppService = uploadFileAppService;
		}

		// GET: /Banners/Index
		public IActionResult Index()
		{
			return View();
		}

		// GET: /Banners/CreateModal
		public IActionResult CreateModal()
		{
			return PartialView("CreateModal");
		}

		// POST: /Banners/Create
		[HttpPost]
		public async Task<IActionResult> Create(Banner input)
		{
			var result = await _bannerAppService.CreateBanner(input);
			return Json(result);
		}

		public async Task<IActionResult> EditModal(int id)
		{
			var banner = await _bannerAppService.GetBannerById(id);
			return PartialView("EditModal", banner);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(Banner input)
		{
			var result = await _bannerAppService.UpdateBanner(input);
			return Json(result);
		}

		// GET: /Banners/DetailModal
		public async Task<IActionResult> DetailModal(int id)
		{
			var banner = await _bannerAppService.GetBannerById(id);
			return PartialView("DetailModal", banner);
		}

		// POST: /Banners/Delete
		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			await _bannerAppService.DeleteBanner(id);
			return Json(new { success = true });
		}
	}

	// Input Models
	//public class CreateBannerInput
	//{
	//	public string Title { get; set; }
	//	public IFormFile BannerImage { get; set; }
	//	public BannerPosition Position { get; set; }
	//	public int SortOrder { get; set; }
	//	public bool IsActive { get; set; }
	//}

	//public class EditBannerInput
	//{
	//	public int Id { get; set; }
	//	public string Title { get; set; }
	//	public IFormFile BannerImage { get; set; }
	//	public string CurrentImageUrl { get; set; }
	//	public BannerPosition Position { get; set; }
	//	public int SortOrder { get; set; }
	//	public bool IsActive { get; set; }
	//}
}
