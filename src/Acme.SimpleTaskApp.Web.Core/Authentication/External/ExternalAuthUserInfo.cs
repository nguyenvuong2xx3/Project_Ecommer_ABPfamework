using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Authentication.External
{
	public class ExternalAuthUserInfo
	{
		public string ProviderKey { get; set; }

		public string Name { get; set; }

		public string EmailAddress { get; set; }

		public string Surname { get; set; }

		public string Provider { get; set; }
		public List<ClaimKeyValue> Claims { get; internal set; } = new List<ClaimKeyValue>();

	}
	public class ClaimKeyValue
	{
		public string Type { get; set; }

		public string Value { get; set; }

		public ClaimKeyValue(string type, string value)
		{
			Type = type;
			Value = value;
		}
	}
}
