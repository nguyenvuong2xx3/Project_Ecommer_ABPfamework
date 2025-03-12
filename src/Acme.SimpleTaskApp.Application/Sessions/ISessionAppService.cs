using System.Threading.Tasks;
using Abp.Application.Services;
using Acme.SimpleTaskApp.Sessions.Dto;

namespace Acme.SimpleTaskApp.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
