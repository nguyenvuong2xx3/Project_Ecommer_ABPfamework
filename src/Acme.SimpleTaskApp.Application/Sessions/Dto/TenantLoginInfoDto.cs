using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Acme.SimpleTaskApp.MultiTenancy;

namespace Acme.SimpleTaskApp.Sessions.Dto
{
    [AutoMapFrom(typeof(Tenant))]
    public class TenantLoginInfoDto : EntityDto
    {
        public string TenancyName { get; set; }

        public string Name { get; set; }
    }
}
