using Acme.SimpleTaskApp.Banners;
using Acme.SimpleTaskApp.Banners.Dtos;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.UploadFile;
using Acme.SimpleTaskApp.Web.Models.Banners;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Acme.SimpleTaskApp.Authorization;

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
		[AbpMvcAuthorize(PermissionNames.Pages_Banners_Create)]
		public IActionResult CreateModal()
		{
			return PartialView("_CreateModal");
		}

		// POST: /Banners/Create
		[AbpMvcAuthorize(PermissionNames.Pages_Banners_Create)]
		[HttpPost]
		public async Task<IActionResult> Create([FromForm] CreateBannerDto input)
		{
			var result = await _bannerAppService.CreateBanner(input);
			return Json(result);
		}

		[AbpMvcAuthorize(PermissionNames.Pages_Banners_Edit)]
		public async Task<IActionResult> EditModal(int id)
		{
			var banner = await _bannerAppService.GetBannerById(id);
			var viewModel = new BannerViewModel
			{
				Banner = banner
			};
			return PartialView("_EditModal", viewModel);
		}

		[AbpMvcAuthorize(PermissionNames.Pages_Banners_Edit)]
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
			var viewModel = new BannerViewModel
			{
				Banner = banner
			};
			return PartialView("_DetailModal", viewModel);
		}

		// POST: /Banners/Delete
		[AbpMvcAuthorize(PermissionNames.Pages_Banners_Delete)]
		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			await _bannerAppService.DeleteBanner(id);
			return Json(new { success = true });
		}
	}
}
