using Abp.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Authentication.External.External.Facebook
{
    public class FacebookAuthProviderApi : ExternalAuthProviderApiBase
    {
        public const string Name = "Facebook";

        public override async Task<ExternalAuthUserInfo> GetUserInfo(string accessCode)
        {
            string endpoint3 = QueryHelpers.AddQueryString("https://graph.facebook.com/v2.8/me", "access_token", accessCode);
            endpoint3 = QueryHelpers.AddQueryString(endpoint3, "appsecret_proof", GenerateAppSecretProof(accessCode));
            endpoint3 = QueryHelpers.AddQueryString(endpoint3, "fields", "email,last_name,first_name,middle_name");
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core OAuth middleware");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.Host = "graph.facebook.com";
            client.Timeout = TimeSpan.FromSeconds(30.0);
            client.MaxResponseContentBufferSize = 10485760L;
            HttpResponseMessage response = await client.GetAsync(endpoint3);
            response.EnsureSuccessStatusCode();
            JObject payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            string name = FacebookHelper.GetFirstName(payload);
            string middleName = FacebookHelper.GetMiddleName(payload);
            if (!middleName.IsNullOrEmpty())
            {
                name += middleName;
            }

            ExternalAuthUserInfo result = new ExternalAuthUserInfo
            {
                Name = name,
                EmailAddress = FacebookHelper.GetEmail(payload),
                Surname = FacebookHelper.GetLastName(payload),
                Provider = "Facebook",
                ProviderKey = FacebookHelper.GetId(payload)
            };
            FillClaimsFromJObject(result, payload);
            return result;
        }

        private string GenerateAppSecretProof(string accessToken)
        {
            using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.ASCII.GetBytes(base.ProviderInfo.ClientSecret));
            byte[] array = hMACSHA.ComputeHash(Encoding.ASCII.GetBytes(accessToken));
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            return stringBuilder.ToString();
        }
    }
}
