using Acme.SimpleTaskApp.Sessions.Dto;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.User
{
	public class UserViewModel
	{
		public GetCurrentLoginInformationsOutput LoginInformations { get; set; }


		public bool IsMultiTenancyEnabled { get; set; }

		public string GetShownLoginName()
		{
			if (LoginInformations.User != null)
			{
				var userName = LoginInformations.User.Name;

				if (!IsMultiTenancyEnabled)
				{
					return userName;
				}

				return userName;
			}
			else
			{
				return "";
			}
		}
	}
}
