using Abp.Application.Services;
using Acme.SimpleTaskApp.MultiTenancy.Dto;

namespace Acme.SimpleTaskApp.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

