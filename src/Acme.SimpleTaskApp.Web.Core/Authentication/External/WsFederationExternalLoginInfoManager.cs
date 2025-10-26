using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Abp.Extensions;
using Acme.SimpleTaskApp.Authentication.External;

namespace Acme.SimpleTaskApp.Authentication.External
{
    public class WsFederationExternalLoginInfoManager : DefaultExternalLoginInfoManager
    {
        public override string GetUserNameFromClaims(List<Claim> claims)
        {
            var userName = claims.First(c => c.Type == ClaimTypes.WindowsAccountName)?.Value;
            if (!userName.IsNullOrEmpty())
            {
                return userName;
            }

            return base.GetUserNameFromClaims(claims);
        }
    }
}