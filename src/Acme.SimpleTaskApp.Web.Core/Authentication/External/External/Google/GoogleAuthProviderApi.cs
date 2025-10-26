using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Authentication.External.External.Google;

public class GoogleAuthProviderApi : ExternalAuthProviderApiBase
{
	public const string Name = "Google";

	public override async Task<ExternalAuthUserInfo> GetUserInfo(string accessCode)
	{
		string userInfoEndpoint = base.ProviderInfo.AdditionalParams["UserInfoEndpoint"];
		if (string.IsNullOrEmpty(userInfoEndpoint))
		{
			throw new ApplicationException("Authentication:Google:UserInfoEndpoint configuration is required.");
		}

		using HttpClient client = new HttpClient();
		client.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core OAuth middleware");
		client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
		client.Timeout = TimeSpan.FromSeconds(30.0);
		client.MaxResponseContentBufferSize = 10485760L;
		HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessCode);
		HttpResponseMessage response = await client.SendAsync(request);
		response.EnsureSuccessStatusCode();
		JObject payload = JObject.Parse(await response.Content.ReadAsStringAsync());
		ExternalAuthUserInfo result = new ExternalAuthUserInfo
		{
			Name = GoogleHelper.GetName(payload),
			EmailAddress = GoogleHelper.GetEmail(payload),
			Surname = GoogleHelper.GetFamilyName(payload),
			ProviderKey = GoogleHelper.GetId(payload),
			Provider = "Google"
		};
		FillClaimsFromJObject(result, payload);
		return result;
	}
}
