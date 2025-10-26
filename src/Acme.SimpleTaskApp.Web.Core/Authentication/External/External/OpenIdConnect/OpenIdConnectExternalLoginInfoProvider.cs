using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Authentication.External.External.OpenIdConnect
{
	public class OpenIdConnectExternalLoginInfoProvider : IExternalLoginInfoProvider
	{
		public string Name { get; } = "OpenIdConnect";


		protected string ClientId { get; set; }

		protected string ClientSecret { get; set; }

		protected string Authority { get; set; }

		protected string LoginUrl { get; set; }

		protected bool ValidateIssuer { get; set; }

		protected string ResponseType { get; set; }

		protected List<JsonClaimMap> JsonClaimMaps { get; set; }

		protected ExternalLoginProviderInfo ExternalLoginProviderInfo { get; set; }

		public OpenIdConnectExternalLoginInfoProvider(string clientId, string clientSecret, string authority, string loginUrl, bool validateIssuer, string responseType, List<JsonClaimMap> jsonClaimMaps)
		{
			ClientId = clientId;
			ClientSecret = clientSecret;
			Authority = authority;
			LoginUrl = loginUrl;
			ValidateIssuer = validateIssuer;
			ResponseType = responseType;
			JsonClaimMaps = jsonClaimMaps;
			CreateExternalLoginProviderInfo();
		}

		private void CreateExternalLoginProviderInfo()
		{
			ExternalLoginProviderInfo = new ExternalLoginProviderInfo("OpenIdConnect", ClientId, ClientSecret, typeof(OpenIdConnectAuthProviderApi), new Dictionary<string, string>
				{
						{ "Authority", Authority },
						{ "LoginUrl", LoginUrl },
						{
								"ValidateIssuer",
								ValidateIssuer.ToString()
						},
						{ "ResponseType", ResponseType }
				}, JsonClaimMaps);
		}

		public virtual ExternalLoginProviderInfo GetExternalLoginInfo()
		{
			return ExternalLoginProviderInfo;
		}
	}
}
