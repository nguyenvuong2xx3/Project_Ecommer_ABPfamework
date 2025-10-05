using Microsoft.AspNetCore.Mvc;

namespace Acme.SimpleTaskApp.Web.Controllers
{
    public static class ControllerExtensions
    {
        public static void Flash(this Controller controller, string message, string type = "info")
        {
            controller.TempData["FlashMessage"] = message;
            controller.TempData["FlashMessageType"] = type; // success, info, warning, error
        }
    }
}