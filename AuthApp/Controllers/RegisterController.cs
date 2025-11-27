using AuthApp.Data;
using AuthApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RegisterAccount(Account account)
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return View("Success");
        }
    }
}