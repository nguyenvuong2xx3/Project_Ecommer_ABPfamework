using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Authentication.External.External.Google
{
    public class GoogleExternalLoginInfoProvider : IExternalLoginInfoProvider
    {
        public string Name { get; } = "Google";


        protected string ClientId { get; set; }

        protected string ClientSecret { get; set; }

        protected string UserInfoEndpoint { get; set; }

        protected ExternalLoginProviderInfo ExternalLoginProviderInfo { get; set; }

        public GoogleExternalLoginInfoProvider(string clientId, string clientSecret, string userInfoEndpoint)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            UserInfoEndpoint = userInfoEndpoint;
            CreateExternalLoginInfo();
        }

        private void CreateExternalLoginInfo()
        {
            ExternalLoginProviderInfo = new ExternalLoginProviderInfo("Google", ClientId, ClientSecret, typeof(GoogleAuthProviderApi), new Dictionary<string, string> { { "UserInfoEndpoint", UserInfoEndpoint } });
        }

        public virtual ExternalLoginProviderInfo GetExternalLoginInfo()
        {
            return ExternalLoginProviderInfo;
        }
    }
}
