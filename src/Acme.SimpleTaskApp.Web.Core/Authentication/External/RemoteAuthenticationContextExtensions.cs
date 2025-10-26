using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Acme.SimpleTaskApp.Authentication.External
{
    public static class RemoteAuthenticationContextExtensions
    {
        public static void AddMappedClaims<TOptions>(this RemoteAuthenticationContext<TOptions> context, List<JsonClaimMap> mappings) where TOptions : RemoteAuthenticationOptions
        {
            if (!mappings.Any())
            {
                return;
            }

            foreach (JsonClaimMap claimMapping in mappings)
            {
                Claim claim = context.Principal!.Claims.ToList().FirstOrDefault((Claim c) => c.Type == claimMapping.Key);
                if (claim != null)
                {
                    context.Principal!.AddIdentity(new ClaimsIdentity(new List<Claim>
                    {
                        new Claim(claimMapping.Claim, claim.Value)
                    }));
                }
            }
        }

        public static void AddMappedClaims(this ClaimsPrincipal principal, List<JsonClaimMap> mappings)
        {
            if (!mappings.Any())
            {
                return;
            }

            foreach (JsonClaimMap claimMapping in mappings)
            {
                Claim claim = principal.Claims.ToList().FirstOrDefault((Claim c) => c.Type == claimMapping.Key);
                if (claim != null)
                {
                    principal.AddIdentity(new ClaimsIdentity(new List<Claim>
                    {
                        new Claim(claimMapping.Claim, claim.Value)
                    }));
                }
            }
        }
    }
}
