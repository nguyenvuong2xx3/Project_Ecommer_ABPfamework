using System.Collections.Generic;
using Abp.Dependency;

namespace Acme.SimpleTaskApp.Authentication.External
{
	public class ExternalAuthConfiguration : IExternalAuthConfiguration, ISingletonDependency
	{
		public List<ExternalLoginProviderInfo> Providers { get; }

		public List<IExternalLoginInfoProvider> ExternalLoginInfoProviders { get; }
		public ExternalAuthConfiguration()
		{
			Providers = new List<ExternalLoginProviderInfo>();
			ExternalLoginInfoProviders = new List<IExternalLoginInfoProvider>();
		}
	}
}
