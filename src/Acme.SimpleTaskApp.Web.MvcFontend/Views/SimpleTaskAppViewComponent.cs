using Abp.AspNetCore.Mvc.ViewComponents;

namespace Acme.SimpleTaskApp.Web.Views
{
    public abstract class SimpleTaskAppViewComponent : AbpViewComponent
    {
        protected SimpleTaskAppViewComponent()
        {
            LocalizationSourceName = SimpleTaskAppConsts.LocalizationSourceName;
        }
    }
}
