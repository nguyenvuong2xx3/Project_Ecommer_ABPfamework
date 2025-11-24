using Abp.Application.Services.Dto;
using Abp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications.Dtos
{
	public class GetPublishedNotificationsOutput : PagedResultDto<GetNotificationsCreatedByUserOutput>
	{
		public GetPublishedNotificationsOutput(List<GetNotificationsCreatedByUserOutput> notificationsCreatedByUserOutput)
			: base(notificationsCreatedByUserOutput.Count, (IReadOnlyList<GetNotificationsCreatedByUserOutput>)notificationsCreatedByUserOutput)
		{
		}
	}
}
