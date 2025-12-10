using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Acme.SimpleTaskApp.Authorization;
using Acme.SimpleTaskApp.Controllers;
using Acme.SimpleTaskApp.Roles;
using Acme.SimpleTaskApp.Roles.Dto;
using Acme.SimpleTaskApp.Web.Models.Roles;
using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Controllers
{
	public class RolesController : SimpleTaskAppControllerBase
	{
		private readonly IRoleAppService _roleAppService;

		public RolesController(IRoleAppService roleAppService)
		{
			_roleAppService = roleAppService;
		}

		public async Task<IActionResult> Index()
		{
			return View();
		}

		public async Task<ActionResult> EditModal(int roleId)
		{
			var output = await _roleAppService.GetRoleForEdit(new EntityDto(roleId));
			var model = new RoleEditTreeViewModel
			{
				Role = output.Role,
				//Permissions = output.Permissions,
				//GrantedPermissionNames = output.GrantedPermissionNames
			};
			return PartialView("_EditModal", model);
		}

		public async Task<ActionResult> CreateModal()
		{
			return PartialView("_CreateModal");
		}
	}
	//{
	//	private readonly IRoleAppService _roleAppService;
	//	private readonly IPermissionAppService _permissionAppService;

	//	public RolesController(
	//			IRoleAppService roleAppService,
	//			IPermissionAppService permissionAppService)
	//	{
	//		_roleAppService = roleAppService;
	//		_permissionAppService = permissionAppService;
	//	}

	//	public ActionResult Index()
	//	{
	//		var permissions = _permissionAppService.GetAllPermissions().Items.ToList();

	//		var model = new RoleListViewModel
	//		{
	//			Permissions = ObjectMapper.Map<List<FlatPermissionDto>>(permissions).OrderBy(p => p.DisplayName).ToList(),
	//			GrantedPermissionNames = new List<string>()
	//		};

	//		return View(model);
	//	}

	//	[AbpMvcAuthorize(PermissionNames.Pages_Roles_Create, PermissionNames.Pages_Roles_Edit)]

	//	public async Task<PartialViewResult> CreateOrEditModal(int? id)
	//	{
	//		var output = await _roleAppService.GetRoleForEdit(new NullableIdDto { Id = id });
	//		var viewModel = ObjectMapper.Map<CreateOrEditRoleModalViewModel>(output);

	//		return PartialView("_CreateOrEditModal", viewModel);
	//	}
	//}
}
