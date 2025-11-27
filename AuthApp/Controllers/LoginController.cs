using AuthApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AuthApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string role)
        {
            ViewBag.Username = username;

            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (account == null)
            {
                ViewBag.ErrorMessage = "<strong>Incorrect Username or Password.</strong> Please check your Username and Password.";
                return View("Index");
            }

            if (account.Role != role)
            {
                ViewBag.ErrorMessage = "<strong>Incorrect Role.</strong> The selected role does not match your account type. Please select the correct role.";
                return View("Index");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim("Fullname", account.Fullname),
                new Claim(ClaimTypes.Role, account.Role)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            switch (account.Role)
            {
                case "Admin":
                    return RedirectToAction("Index", "AdminHome");
                case "Teacher":
                    return RedirectToAction("Index", "TeacherHome");
                case "Student":
                    return RedirectToAction("Index", "StudentHome");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}