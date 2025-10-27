using Acme.SimpleTaskApp.Authentication.External;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Startup
{
	public static class AuthConfigurer
	{
		public static void Configure(IServiceCollection services, IConfiguration configuration)
		{
			var authenticationBuilder = services.AddAuthentication();

			if (bool.Parse(configuration["Authentication:Google:IsEnabled"]))
			{
				if (bool.Parse(configuration["Authentication:AllowSocialLoginSettingsPerTenant"]))
				{
					services.AddSingleton<IOptionsMonitor<GoogleOptions>>();
				}

				authenticationBuilder.AddGoogle(options =>
				{

					options.ClientId = configuration["Authentication:Google:ClientId"];
					options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
					options.CallbackPath = "/signin-google";
					options.UserInformationEndpoint = configuration["Authentication:Google:UserInfoEndpoint"];

					options.SaveTokens = true;

					options.ClaimActions.Clear();
					options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
					options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
					options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
					options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
					options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");


					options.Scope.Add("profile");
					options.Scope.Add("email");
					//options.Events = new OAuthEvents
					//{
					//	OnTicketReceived = context =>
					//	{
					//		// Redirect đến trang Register sau khi đăng nhập thành công
					//		context.ReturnUri = "/HomeCustomer";
					//		return Task.CompletedTask;
					//	}
					//};
				});
			}


			services.ConfigureApplicationCookie(options =>
						{
							options.AccessDeniedPath = "/Account/Forbidden";
						});

			if (bool.Parse(configuration["Authentication:JwtBearer:IsEnabled"]))
			{
				authenticationBuilder
						.AddJwtBearer(options =>
						{
							options.Audience = configuration["Authentication:JwtBearer:Audience"];

							options.TokenValidationParameters = new TokenValidationParameters
							{
								// The signing key must match!
								ValidateIssuerSigningKey = true,
								IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Authentication:JwtBearer:SecurityKey"])),

								// Validate the JWT Issuer (iss) claim
								ValidateIssuer = true,
								ValidIssuer = configuration["Authentication:JwtBearer:Issuer"],

								// Validate the JWT Audience (aud) claim
								ValidateAudience = true,
								ValidAudience = configuration["Authentication:JwtBearer:Audience"],

								// Validate the token expiry
								ValidateLifetime = true,

								// If you want to allow a certain amount of clock drift, set that here
								ClockSkew = TimeSpan.Zero
							};
						});
			}
		}
	}
}
