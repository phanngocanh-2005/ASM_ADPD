using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Controllers
{
    public class TeacherHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
