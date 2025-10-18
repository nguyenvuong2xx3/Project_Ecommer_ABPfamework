using Acme.SimpleTaskApp.Roles.Dto;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Roles
{
	public class RoleEditTreeViewModel
	{
		public RoleEditDto Role { get; set; }
		//public List<TreePermissionDto> Permissions { get; set; }
		//public List<string> GrantedPermissionNames { get; set; }
	}
}
