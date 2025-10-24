using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Settings.Dtos
{
	public class MailSettingDto
	{
		public string Server { get; set; }
		public int SmtpPort { get; set; }
		public string SenderName { get; set; }
		public bool EnableSsl { get; set; }	
		public string SenderEmail { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
	}
}
