using AuthApp.Data;
using AuthApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AuthApp.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAccount(Account model)
        {
            // Flag to determine whether any duplicate constraint violations occur
            bool hasDuplicateError = false;

            // Step 1: validate model; return immediately if a required field is missing
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Step 2: check duplicate username
            if (await _context.Accounts.AnyAsync(a => a.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists. Please select another Username.");
                hasDuplicateError = true;
            }

            // Step 3: check duplicate email
            if (await _context.Accounts.AnyAsync(a => a.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists. Please select another Email.");
                hasDuplicateError = true;
            }

            // Step 4: check duplicate phone number
            if (await _context.Accounts.AnyAsync(a => a.PhoneNumber == model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Phonenumber already exists. Please select another Phonenumber");
                hasDuplicateError = true;
            }

            // Step 5: if any duplicates were found, display validation errors
            if (hasDuplicateError)
            {
                return View("Index", model);
            }

            // Step 6: persist new account (password should be hashed in production)
            _context.Accounts.Add(model);
            await _context.SaveChangesAsync();

            // Redirect to login page after success
            return RedirectToAction("Index", "Login");
        }
    }
}