using Abp.Authorization;
using Acme.SimpleTaskApp.Authorization.Roles;
using Acme.SimpleTaskApp.Authorization.Users;

namespace Acme.SimpleTaskApp.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
