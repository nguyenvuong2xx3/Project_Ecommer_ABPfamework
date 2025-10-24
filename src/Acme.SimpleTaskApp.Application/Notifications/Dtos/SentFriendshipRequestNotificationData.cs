using Abp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications.Dtos
{
	[Serializable]
	public class SentFriendshipRequestNotificationData : NotificationData
	{
		public string SenderUserName { get; set; }

		public string FriendshipMessage { get; set; }

		public SentFriendshipRequestNotificationData(string senderUserName, string friendshipMessage)
		{
			SenderUserName = senderUserName;
			FriendshipMessage = friendshipMessage;
		}
	}
}
