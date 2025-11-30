using System.Collections.Generic;
using Abp.Configuration;

namespace Acme.SimpleTaskApp.Configuration
{
	public class AppSettingProvider : SettingProvider
	{
		public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
		{
			return new[]
			{
						new SettingDefinition(AppSettingNames.UiTheme, "red", scopes: SettingScopes.Application | SettingScopes.Tenant | SettingScopes.User, clientVisibilityProvider: new VisibleSettingClientVisibilityProvider()),

						 new SettingDefinition(
								AppMailSettingNames.Server,
								"smtp.gmail.com",
								scopes: SettingScopes.Application | SettingScopes.Tenant,
								isVisibleToClients: false
						),
						new SettingDefinition(
								AppMailSettingNames.SmtpPort,
								"587",
								scopes: SettingScopes.Application | SettingScopes.Tenant,
								isVisibleToClients: false
						),
						new SettingDefinition(
								AppMailSettingNames.SenderName,
								"Your Application Name",
								scopes: SettingScopes.Application | SettingScopes.Tenant,
								isVisibleToClients: false
						),
						new SettingDefinition(
								AppMailSettingNames.SenderEmail,
								"noreply@yourapp.com",
								scopes: SettingScopes.Application | SettingScopes.Tenant,
								isVisibleToClients: false
						),
						new SettingDefinition(
								AppMailSettingNames.UserName,
								"",
								scopes: SettingScopes.Application | SettingScopes.Tenant,
								isVisibleToClients: false
						),
						new SettingDefinition(
								AppMailSettingNames.Password,
								"",
								scopes: SettingScopes.Application | SettingScopes.Tenant,
								isVisibleToClients: false
						),
						new SettingDefinition(
								AppMailSettingNames.EnableSsl,
								"true",
								scopes: SettingScopes.Application | SettingScopes.Tenant,
								isVisibleToClients: false
						),
						
						// Store Configuration Settings - NO DEFAULT VALUES
						new SettingDefinition(
								AppNameStore.NameStore,
								"", // Empty default - user must configure
								scopes: SettingScopes.Application | SettingScopes.Tenant,
								isVisibleToClients: true
						),
						new SettingDefinition(
								AppNameStore.UrlLogo,
								"", // Empty default - user must configure
								scopes: SettingScopes.Application | SettingScopes.Tenant,
								isVisibleToClients: true
						)
			};
		}
	}
}
