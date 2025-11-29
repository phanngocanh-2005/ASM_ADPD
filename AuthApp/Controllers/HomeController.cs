using System.Diagnostics;
using AuthApp.Data;
using AuthApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Controllers
{
    public class HomeController : Controller
    {
        private bool isConnected = false;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Nếu đã đăng nhập, redirect đến dashboard tương ứng
            if (User?.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "AdminHome");
                }
                else if (User.IsInRole("Teacher"))
                {
                    return RedirectToAction("Index", "TeacherHome");
                }
                else if (User.IsInRole("Student"))
                {
                    return RedirectToAction("Index", "StudentHome");
                }
            }

            this.isConnected = _context.Database.CanConnect();
            ViewBag.IsConnected = isConnected;
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
