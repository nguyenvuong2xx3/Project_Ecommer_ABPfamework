using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace Acme.SimpleTaskApp.Localization
{
    public static class SimpleTaskAppLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(SimpleTaskAppConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(SimpleTaskAppLocalizationConfigurer).GetAssembly(),
                        "Acme.SimpleTaskApp.Localization.SourceFiles"
                    )
                )
            );
        }
    }
}
