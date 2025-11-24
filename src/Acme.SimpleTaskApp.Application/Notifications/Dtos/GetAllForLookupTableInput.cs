// BBK.
using Abp.Application.Services.Dto;

namespace Acme.SimpleTaskApp.Notifications.Dtos
{
public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
{
	public string Filter { get; set; }
}
}