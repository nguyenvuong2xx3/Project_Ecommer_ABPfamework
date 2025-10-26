using Abp.Dependency;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;

namespace Acme.SimpleTaskApp.Authentication.External
{
	public interface IExternalLoginInfoManager : ITransientDependency
	{
		string GetUserNameFromClaims(List<Claim> claims);

		string GetUserNameFromExternalAuthUserInfo(ExternalAuthUserInfo userInfo);

		(string name, string surname) GetNameAndSurnameFromClaims(List<Claim> claims, IdentityOptions identityOptions);
	}
}
