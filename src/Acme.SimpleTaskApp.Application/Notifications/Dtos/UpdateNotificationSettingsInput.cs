using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications.Dtos
{
	public class UpdateNotificationSettingsInput
	{
		public bool ReceiveNotifications { get; set; }

		public List<NotificationSubscriptionDto> Notifications { get; set; }
	}
}
