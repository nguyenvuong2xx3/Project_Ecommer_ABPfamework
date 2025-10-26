using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Authentication.External
{
	public interface IExternalLoginInfoProvider
	{
		string Name { get; }

		ExternalLoginProviderInfo GetExternalLoginInfo();
	}

	public interface IExternalAuthConfiguration
	{
		[Obsolete("Use IExternalLoginInfoProviders")]
		List<ExternalLoginProviderInfo> Providers { get; }

		List<IExternalLoginInfoProvider> ExternalLoginInfoProviders { get; }
	}

	//public interface IExternalAuthManager
	//{
	//	Task<bool> IsValidUser(string provider, string providerKey, string providerAccessCode);

	//	Task<ExternalAuthUserInfo> GetUserInfo(string provider, string accessCode);
	//}

	public class JsonClaimMap
	{
		public string Claim { get; set; }

		public string Key { get; set; }
	}
	public class ExternalLoginProviderInfo
	{
		public string Name { get; set; }

		public string ClientId { get; set; }

		public string ClientSecret { get; set; }

		public Type ProviderApiType { get; set; }

		public Dictionary<string, string> AdditionalParams { get; set; }

		public List<JsonClaimMap> ClaimMappings { get; set; }

		public ExternalLoginProviderInfo(string name, string clientId, string clientSecret, Type providerApiType, Dictionary<string, string> additionalParams = null, List<JsonClaimMap> claimMappings = null)
		{
			Name = name;
			ClientId = clientId;
			ClientSecret = clientSecret;
			ProviderApiType = providerApiType;
			AdditionalParams = additionalParams ?? new Dictionary<string, string>();
			ClaimMappings = claimMappings ?? new List<JsonClaimMap>();
		}
	}
}
