using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Email.Dtos
{
	[Serializable]
	public class SendEmailJobArgs
	{
		public long SenderUserId { get; set; }

		public long TargetUserId { get; set; }

		public string Subject { get; set; }

		public string Body { get; set; }
	}
}
