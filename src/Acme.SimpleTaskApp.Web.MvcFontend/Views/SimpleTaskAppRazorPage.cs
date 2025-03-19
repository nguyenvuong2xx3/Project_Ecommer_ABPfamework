using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Acme.SimpleTaskApp.Web.Views
{
    public abstract class SimpleTaskAppRazorPage<TModel> : AbpRazorPage<TModel>
    {
        [RazorInject]
        public IAbpSession AbpSession { get; set; }

        protected SimpleTaskAppRazorPage()
        {
            LocalizationSourceName = SimpleTaskAppConsts.LocalizationSourceName;
        }
    }
}
