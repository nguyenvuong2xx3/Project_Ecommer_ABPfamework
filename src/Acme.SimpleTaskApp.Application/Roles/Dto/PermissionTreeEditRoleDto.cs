using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Roles.Dto
{
	public class PermissionTreeEditRoleDto
	{
		public RoleEditDto Role { get; set; }
		public List<TreePermissionDto> Permissions { get; set; }
		public List<string> GrantedPermissionNames { get; set; }

	}
}
