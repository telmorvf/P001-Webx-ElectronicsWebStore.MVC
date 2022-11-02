using Microsoft.AspNetCore.Mvc;

namespace Webx.Web.Controllers
{
    public class AdminPanelController : Controller
    {

        public AdminPanelController()
        {

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
