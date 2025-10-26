using Abp.Application.Services.Dto;
using Abp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications.Dtos
{
	public class GetNotificationsOutput : PagedResultDto<UserNotification>
	{
		public int UnreadCount { get; set; }

		public GetNotificationsOutput(int totalCount, int unreadCount, List<UserNotification> notifications)
			: base(totalCount, (IReadOnlyList<UserNotification>)notifications)
		{
			UnreadCount = unreadCount;
		}
	}
}
