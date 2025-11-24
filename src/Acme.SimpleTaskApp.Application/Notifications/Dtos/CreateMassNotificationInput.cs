using Abp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications.Dtos
{
	public class CreateMassNotificationInput
	{
		public string Message { get; set; }

		public NotificationSeverity Severity { get; set; }

		public long[] UserIds { get; set; }

		public long[] OrganizationUnitIds { get; set; }

		public string[] TargetNotifiers { get; set; }
	}

}
