using Abp.MultiTenancy;
using Acme.SimpleTaskApp.Authorization.Users;

namespace Acme.SimpleTaskApp.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
