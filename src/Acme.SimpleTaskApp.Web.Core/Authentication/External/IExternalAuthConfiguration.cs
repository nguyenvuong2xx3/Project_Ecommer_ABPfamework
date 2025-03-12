using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Authentication.External
{
    public interface IExternalAuthConfiguration
    {
        List<ExternalLoginProviderInfo> Providers { get; }
    }
}
