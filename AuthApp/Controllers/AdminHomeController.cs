using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Controllers
{
    public class AdminHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
