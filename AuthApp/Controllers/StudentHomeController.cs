using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Controllers
{
    public class StudentHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
