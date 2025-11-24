using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications.Dtos
{
	public class SetNotificationAsReadOutput
	{
		public bool Success { get; set; }

		public SetNotificationAsReadOutput(bool success)
		{
			Success = success;
		}
	}
}
