using System.Collections.Generic;
using Acme.SimpleTaskApp.Roles.Dto;

namespace Acme.SimpleTaskApp.Web.Models.Roles
{
    public class RoleListViewModel
    {
        public IReadOnlyList<PermissionDto> Permissions { get; set; }
    }
}
