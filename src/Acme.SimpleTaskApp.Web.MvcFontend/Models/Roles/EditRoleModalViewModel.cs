using Abp.AutoMapper;
using Acme.SimpleTaskApp.Roles.Dto;
using Acme.SimpleTaskApp.Web.Models.Common;

namespace Acme.SimpleTaskApp.Web.Models.Roles
{
    [AutoMapFrom(typeof(GetRoleForEditOutput))]
    public class EditRoleModalViewModel : GetRoleForEditOutput, IPermissionsEditViewModel
    {
        public bool HasPermission(FlatPermissionDto permission)
        {
            return GrantedPermissionNames.Contains(permission.Name);
        }
    }
}
