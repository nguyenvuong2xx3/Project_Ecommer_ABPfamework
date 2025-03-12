using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Acme.SimpleTaskApp.Authorization;

namespace Acme.SimpleTaskApp
{
    [DependsOn(
        typeof(SimpleTaskAppCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class SimpleTaskAppApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<SimpleTaskAppAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(SimpleTaskAppApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
