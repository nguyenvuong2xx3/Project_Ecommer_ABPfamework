using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications.Dtos
{
	public class NotificationSubscriptionDto
	{
		[Required]
		[MaxLength(96)]
		public string Name { get; set; }

		public bool IsSubscribed { get; set; }
	}
}
