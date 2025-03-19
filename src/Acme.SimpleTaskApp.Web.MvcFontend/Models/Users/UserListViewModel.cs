using System.Collections.Generic;
using Acme.SimpleTaskApp.Roles.Dto;

namespace Acme.SimpleTaskApp.Web.Models.Users
{
    public class UserListViewModel
    {
        public IReadOnlyList<RoleDto> Roles { get; set; }
    }
}
