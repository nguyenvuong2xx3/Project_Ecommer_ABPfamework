using Abp.Application.Services.Dto;

namespace Acme.SimpleTaskApp.Roles.Dto
{
    public class PagedRoleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}

