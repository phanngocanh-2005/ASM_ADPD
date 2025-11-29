using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthApp.Data;
using AuthApp.Models;

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
            string? username = HttpContext.Session.GetString("Username");
            if (username != null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult TeacherLogin(string returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true && User.IsInRole("Teacher"))
            {
                return RedirectToAction("Index", "TeacherHome");
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeacherLogin(string username, string password, bool rememberMe = false, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            var account = await _context.Accounts
                .FirstOrDefaultAsync(u => u.Username == username && 
                                       u.Password == password && 
                                       u.Role == "Teacher");

            if (account == null)
            {
                ViewBag.Username = username;
                ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim("Fullname", account.Fullname),
                new Claim(ClaimTypes.Role, "Teacher")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.Add(rememberMe ? 
                    TimeSpan.FromDays(7) : TimeSpan.FromMinutes(30))
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            HttpContext.Session.SetString("Username", account.Username);
            
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "TeacherHome");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, string role)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (account == null)
            {
                ViewBag.Username = username;
                ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                return View("Index");
            }

            if (account.Role != role)
            {
                ViewBag.Username = username;
                ViewBag.ErrorMessage = "Vui lòng chọn đúng vai trò đăng nhập.";
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

            HttpContext.Session.SetString("Username", account.Username);

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

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}