using Abp.AutoMapper;
using Acme.SimpleTaskApp.Sessions.Dto;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.TenantChange
{
    [AutoMapFrom(typeof(GetCurrentLoginInformationsOutput))]
    public class TenantChangeViewModel
    {
        public TenantLoginInfoDto Tenant { get; set; }
    }
}
