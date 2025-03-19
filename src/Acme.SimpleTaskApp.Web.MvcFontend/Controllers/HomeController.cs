using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using Acme.SimpleTaskApp.Controllers;

namespace Acme.SimpleTaskApp.Web.Controllers
{
    [AbpMvcAuthorize]
    public class HomeController : SimpleTaskAppControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
