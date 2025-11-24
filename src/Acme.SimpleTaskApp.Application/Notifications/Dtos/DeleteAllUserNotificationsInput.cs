using Abp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications.Dtos
{
	public class DeleteAllUserNotificationsInput
	{
		public UserNotificationState? State { get; set; }

		public DateTime? StartDate { get; set; }

		public DateTime? EndDate { get; set; }
	}
}
