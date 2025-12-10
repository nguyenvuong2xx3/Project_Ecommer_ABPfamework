using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.UI;
using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Web.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Acme.SimpleTaskApp.Authorization;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class CategoriesController : SimpleTaskAppControllerBase
	{
		private readonly ICategoryAppService _categoryAppService;
		private readonly IRepository<Category> _categoryRepository;

		public CategoriesController(ICategoryAppService categoryAppService, IRepository<Category> categoryRepository)
		{
			_categoryAppService = categoryAppService;
			_categoryRepository = categoryRepository;
		}

		[AbpMvcAuthorize(PermissionNames.Pages_Categories_View)]
		public async Task<ActionResult> Index()
		{
			var output = await _categoryRepository.GetAllAsync();
			var model = new CategoryViewModel
			{
				Categories = output.ToList()
			};
			return View(model);
		}

		[AbpMvcAuthorize(PermissionNames.Pages_Categories_View)]
		public async Task<PartialViewResult> CreateModal()
		{
			// chuẩn bị model nếu cần dữ liệu phụ
			var output = await _categoryRepository.GetAllAsync();
			var model = new CategoryViewModel
			{
				Categories = output.ToList()
			};
			return PartialView("_CreateCategoryModal", model);
		}

		[AbpMvcAuthorize(PermissionNames.Pages_Categories_Update)]
		public async Task<PartialViewResult> EditModal(int categoryId)
		{
			var input = new EntityDto<int> { Id = categoryId };

			var category = await _categoryAppService.GetByIdCategory(input);
			var getAllCategories = await _categoryRepository.GetAllAsync();
			if (category == null)
			{
				throw new UserFriendlyException("Category not found");
			}
			var model = new CategoryViewModel
			{
				Category = category,
				Categories = getAllCategories.ToList()
			};
			return PartialView("_EditCategoryModal", model);
		}
	}
}
