using System.Collections.Generic;
using Acme.SimpleTaskApp.Roles.Dto;

namespace Acme.SimpleTaskApp.Web.Models.Common
{
    public interface IPermissionsEditViewModel
    {
        List<FlatPermissionDto> Permissions { get; set; }
    }
}