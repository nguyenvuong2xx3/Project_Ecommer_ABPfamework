//using System;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Threading.Tasks;
//using Abp.Extensions;
//using Abp.UI;
//using Castle.Core.Logging;
//using Microsoft.AspNetCore.WebUtilities;
//using Tweetinvi;
//using Tweetinvi.Models;

//namespace TNT.Mini.Web.Authentication.External.Twitter
//{
//    public class TwitterAuthProviderApi : ExternalAuthProviderApiBase
//    {
//        public const string Name = "Twitter";

//        private const string BaseApiUrl = "https://api.twitter.com/";

//        public ILogger Logger { get; set; }

//        public TwitterAuthProviderApi()
//        {
//            Logger = NullLogger.Instance;
//        }

//        public async Task<TwitterGetRequestTokenResponse> GetRequestToken(string apiKey, string apiKeySecret, string callbackUrl)
//        {
//            string endpoint = "https://api.twitter.com/".EnsureEndsWith('/') + "oauth/request_token";
//            if (!QueryHelpers.ParseQuery(callbackUrl).ContainsKey("twitter"))
//            {
//                callbackUrl = QueryHelpers.AddQueryString(callbackUrl, "twitter", "1");
//            }

//            TwitterGetTokenRequest requestInfo = new TwitterGetTokenRequest
//            {
//                ConsumerKey = apiKey,
//                CallbackUrl = callbackUrl
//            };
//            string parameterString = requestInfo.GetParameterString("&");
//            string base64Signature = new TwitterSignatureHelper().GetBase64Signature(endpoint, "POST", parameterString, apiKeySecret);
//            string authenticationHeaderValue = "oauth_signature=\"" + WebUtility.UrlEncode(base64Signature) + "\", " + requestInfo.GetParameterString(",");
//            HttpRequestMessage request = new HttpRequestMessage
//            {
//                Method = System.Net.Http.HttpMethod.Post,
//                RequestUri = new Uri(new Uri("https://api.twitter.com/"), endpoint)
//            };
//            request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authenticationHeaderValue);
//            using HttpResponseMessage httpResponseMessage = await new HttpClient().SendAsync(request);
//            string response = await httpResponseMessage.Content.ReadAsStringAsync();
//            if (httpResponseMessage.IsSuccessStatusCode)
//            {
//                TwitterGetRequestTokenResponse requestTokenResponse = new TwitterGetRequestTokenResponse(response);
//                requestTokenResponse.RedirectUrl = "https://api.twitter.com/".EnsureEndsWith('/') + "oauth/authenticate?oauth_token=" + requestTokenResponse.Token;
//                return requestTokenResponse;
//            }

//            Logger.Error("Twitter API error: " + response);
//            throw new UserFriendlyException("Can't connect to twitter.");
//        }

//        public async Task<TwitterGetAccessTokenResponse> GetAccessToken(string token, string verifier)
//        {
//            HttpRequestMessage request = new HttpRequestMessage
//            {
//                Method = System.Net.Http.HttpMethod.Post,
//                RequestUri = new Uri(new Uri("https://api.twitter.com/"), "/oauth/access_token?oauth_token=" + WebUtility.UrlEncode(token) + "&oauth_verifier=" + WebUtility.UrlEncode(verifier))
//            };
//            using HttpResponseMessage httpResponseMessage = await new HttpClient().SendAsync(request);

namespace Acme.SimpleTaskApp.Authentication.External.External.Twitter
{
//            string response = await httpResponseMessage.Content.ReadAsStringAsync();
//            if (httpResponseMessage.IsSuccessStatusCode)
//            {
//                return new TwitterGetAccessTokenResponse(response);
//            }

//            Logger.Error("Twitter API error: " + response);
//            throw new UserFriendlyException("Can't get access token from twitter.");
//        }

//        public override async Task<ExternalAuthUserInfo> GetUserInfo(string accessTokenAndSecret)
//        {
//            string[] values = accessTokenAndSecret.Split('&');
//            TwitterClient userClient = new TwitterClient(accessToken: values[0], accessSecret: values[1], consumerKey: base.ProviderInfo.ClientId, consumerSecret: base.ProviderInfo.ClientSecret);
//            IAuthenticatedUser user = await userClient.Users.GetAuthenticatedUserAsync();
//            return new ExternalAuthUserInfo
//            {
//                Name = user.Name,
//                Surname = user.ScreenName,
//                Provider = "Twitter",
//                EmailAddress = user.Email,
//                ProviderKey = user.IdStr
//            };
//        }
//    }
//}
}