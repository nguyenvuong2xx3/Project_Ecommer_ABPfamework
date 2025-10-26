using Abp.Dependency;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Authentication.External
{
	public abstract class ExternalAuthProviderApiBase : IExternalAuthProviderApi, ITransientDependency
	{
		public ExternalLoginProviderInfo ProviderInfo { get; set; }

		public void Initialize(ExternalLoginProviderInfo providerInfo)
		{
			ProviderInfo = providerInfo;
		}

		public async Task<bool> IsValidUser(string userId, string accessCode)
		{
			var userInfo = await GetUserInfo(accessCode);
			return userInfo.ProviderKey == userId;
		}
		protected virtual void FillClaimsFromJObject(ExternalAuthUserInfo userInfo, JObject payload)
		{
			List<ClaimKeyValue> list = new List<ClaimKeyValue>();
			foreach (KeyValuePair<string, JToken> item in payload)
			{
				list.Add(new ClaimKeyValue(item.Key, item.Value?.ToString()));
			}

			userInfo.Claims = list;
		}
		public abstract Task<ExternalAuthUserInfo> GetUserInfo(string accessCode);
	}
}
