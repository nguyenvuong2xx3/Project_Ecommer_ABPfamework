using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Settings.Dtos
{
	public class GetAllSettingDto
	{
		public MailSettingDto MailSetting { get; set; }
		public StoreSettingDto StoreSetting { get; set; }
	}
}
