using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Roles.Dto
{
	public class TreePermissionDto
	{
		public string DisplayName { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? ParentName { get; set; }
		public List<TreePermissionDto> Children { get; set; } = new();
	}
}
