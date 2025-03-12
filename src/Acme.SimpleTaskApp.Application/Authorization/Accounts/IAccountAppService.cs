using System.Threading.Tasks;
using Abp.Application.Services;
using Acme.SimpleTaskApp.Authorization.Accounts.Dto;

namespace Acme.SimpleTaskApp.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
