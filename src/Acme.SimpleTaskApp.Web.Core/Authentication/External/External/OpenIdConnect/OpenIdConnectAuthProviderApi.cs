using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Abp.Extensions;
using Abp.UI;
using Castle.Core.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Acme.SimpleTaskApp.Authentication.External.External.OpenIdConnect
{
    public class OpenIdConnectAuthProviderApi : ExternalAuthProviderApiBase
    {
        private class ValidateTokenResult
        {
            public JwtSecurityToken Token { get; set; }

            public ClaimsPrincipal Principal { get; set; }

            public ValidateTokenResult(JwtSecurityToken token, ClaimsPrincipal principal)
            {
                Token = token;
                Principal = principal;
            }
        }

        public const string Name = "OpenIdConnect";

        public ILogger Logger { get; set; }

        public OpenIdConnectAuthProviderApi()
        {
            Logger = NullLogger.Instance;
        }

        public override async Task<ExternalAuthUserInfo> GetUserInfo(string token)
        {
            string issuer = base.ProviderInfo.AdditionalParams["Authority"];
            Logger.Info("Using " + issuer + " as issuer for OpenID Connect");
            if (string.IsNullOrEmpty(issuer))
            {
                throw new ApplicationException("Authentication:OpenId:Issuer configuration is required.");
            }

            ConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(issuer.EnsureEndsWith('/') + ".well-known/openid-configuration", new OpenIdConnectConfigurationRetriever(), new HttpDocumentRetriever());
            Logger.Info("Validating retrieved token for OpenID Connect");
            ValidateTokenResult validatedTokenResult = await ValidateToken(token, issuer, configurationManager);
            Logger.Info("OpenID Connect token is validated");
            Logger.Info("Retrieved claims:");
            foreach (Claim claim in validatedTokenResult.Principal.Claims)
            {
                Logger.Info(claim.Type + " -> " + claim.Value);
            }

            Claim fullNameClaim = GetFullNameClaim(validatedTokenResult);
            Claim emailClaim = GetEmailClaim(validatedTokenResult);
            string[] fullNameParts = fullNameClaim.Value.Split(' ');
            return new ExternalAuthUserInfo
            {
                Provider = "OpenIdConnect",
                ProviderKey = validatedTokenResult.Token.Subject,
                Name = fullNameParts[0],
                Surname = ((fullNameParts.Length > 1) ? fullNameParts[1] : fullNameParts[0]),
                EmailAddress = emailClaim.Value,
                Claims = validatedTokenResult.Principal.Claims.Select((Claim c) => new ClaimKeyValue(c.Type, c.Value)).ToList()
            };
        }

        private Claim GetFullNameClaim(ValidateTokenResult validatedTokenResult)
        {
            Claim claim = validatedTokenResult.Principal.Claims.FirstOrDefault((Claim c) => c.Type == "name");
            if (claim != null)
            {
                return claim;
            }

            Logger.Warn("name claim is missing! You can use claims mapping to map one of the retrieved claims to name claim.");
            Logger.Info("Looking for http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name claim");
            claim = validatedTokenResult.Principal.Claims.FirstOrDefault((Claim c) => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            if (claim != null)
            {
                return claim;
            }

            throw new UserFriendlyException("Both name and http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name claims are missing! You can use claims mapping to map one of the retrieved claims to name or http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name claim.");
        }

        private Claim GetEmailClaim(ValidateTokenResult validatedTokenResult)
        {
            Claim claim = validatedTokenResult.Principal.Claims.FirstOrDefault((Claim c) => c.Type == "unique_name");
            if (claim != null)
            {
                return claim;
            }

            Logger.Warn("unique_name claim is missing! You can use claims mapping to map one of the retrieved claims to unique_name claim.");
            Logger.Info("Looking for http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress claim");
            claim = validatedTokenResult.Principal.Claims.FirstOrDefault((Claim c) => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            if (claim != null)
            {
                return claim;
            }

            throw new UserFriendlyException("Both name and http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress claims are missing! You can use claims mapping to map one of the retrieved claims to name or http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress claim.");
        }

        private async Task<ValidateTokenResult> ValidateToken(string token, string issuer, IConfigurationManager<OpenIdConnectConfiguration> configurationManager, CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");
            }

            if (string.IsNullOrEmpty(issuer))
            {
                throw new ArgumentNullException("issuer");
            }

            ICollection<SecurityKey> signingKeys = (await configurationManager.GetConfigurationAsync(ct)).SigningKeys;
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = bool.Parse(base.ProviderInfo.AdditionalParams["ValidateIssuer"]),
                ValidIssuer = issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5.0),
                ValidateAudience = false
            };
            SecurityToken rawValidatedToken;
            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out rawValidatedToken);
            principal.AddMappedClaims(base.ProviderInfo.ClaimMappings);
            Claim audienceClaim = principal.Claims.FirstOrDefault((Claim c) => c.Type == "aud");
            if (audienceClaim == null)
            {
                throw new UserFriendlyException("aud claim is missing ! You can use claims mapping to map one of the retrieved claims to aud claim.");
            }

            if (base.ProviderInfo.ClientId != audienceClaim.Value)
            {
                throw new ApplicationException("ClientId couldn't verified.");
            }

            return new ValidateTokenResult((JwtSecurityToken)rawValidatedToken, principal);
        }
    }
}
