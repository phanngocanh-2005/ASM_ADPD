using AuthApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthApp.Controllers
{
    // Restrict access to admins only
    [Authorize(Roles = "Admin")]
    public class AdminHomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminHomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalAccounts = await _context.Accounts.CountAsync();
            var adminCount = await _context.Accounts.CountAsync(a => a.Role == "Admin");
            var teacherCount = await _context.Accounts.CountAsync(a => a.Role == "Teacher");
            var studentCount = await _context.Accounts.CountAsync(a => a.Role == "Student");
            var programCount = await _context.AcademicPrograms.CountAsync();
            var studentProfiles = await _context.Students.CountAsync();

            ViewBag.TotalAccounts = totalAccounts;
            ViewBag.AdminCount = adminCount;
            ViewBag.TeacherCount = teacherCount;
            ViewBag.StudentCount = studentCount;
            ViewBag.ProgramCount = programCount;
            ViewBag.StudentProfiles = studentProfiles;

            return View();
        }

        public Task<IActionResult> ListAccounts(string role = "All")
        {
            return RenderAccountList(role, allowRoleFilter: true, header: "User Account Management");
        }

        public Task<IActionResult> TeacherAccounts()
        {
            return RenderAccountList("Teacher", allowRoleFilter: false, header: "Teacher Accounts");
        }

        public Task<IActionResult> StudentAccounts()
        {
            return RenderAccountList("Student", allowRoleFilter: false, header: "Student Accounts");
        }

        public Task<IActionResult> AdminAccounts()
        {
            return RenderAccountList("Admin", allowRoleFilter: false, header: "Admin Accounts");
        }

        private async Task<IActionResult> RenderAccountList(string role, bool allowRoleFilter, string header)
        {
            var normalizedRole = string.IsNullOrWhiteSpace(role) ? "All" : role;

            var query = _context.Accounts.AsQueryable();
            if (!normalizedRole.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => a.Role == normalizedRole);
            }

            var accounts = await query
                .OrderBy(a => a.Role)
                .ThenBy(a => a.Username)
                .ToListAsync();

            ViewBag.RoleFilter = normalizedRole;
            ViewBag.AllowRoleFilter = allowRoleFilter;
            ViewBag.HeaderTitle = header;
            ViewBag.RoleLabel = normalizedRole switch
            {
                var r when r.Equals("Admin", StringComparison.OrdinalIgnoreCase) => "Admins",
                var r when r.Equals("Teacher", StringComparison.OrdinalIgnoreCase) => "Teachers",
                var r when r.Equals("Student", StringComparison.OrdinalIgnoreCase) => "Students",
                _ => "All"
            };

            ViewData["Title"] = header;
            return View("ListAccounts", accounts);
        }

        private static string? NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return null;
            }

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return "Admin";
            }
            if (role.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
            {
                return "Teacher";
            }
            if (role.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                return "Student";
            }

            return null;
        }

        [HttpGet]
        public IActionResult CreateAccount(string? role = null)
        {
            var fixedRole = NormalizeRole(role);
            ViewBag.FixedRole = fixedRole;
            ViewBag.Roles = fixedRole == null ? new List<string> { "Admin", "Teacher", "Student" } : null;

            var model = new AuthApp.Models.Account();
            if (fixedRole != null)
            {
                model.Role = fixedRole;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(string? role, AuthApp.Models.Account model)
        {
            // TODO: add password hashing before storing credentials

            var fixedRole = NormalizeRole(role);
            if (fixedRole != null)
            {
                model.Role = fixedRole;
                ViewBag.FixedRole = fixedRole;
            }

            if (ModelState.IsValid)
            {
                var existingAccount = await _context.Accounts.AnyAsync(a => a.Username == model.Username);
                if (existingAccount)
                {
                    ViewBag.ErrorMessage = "Username already exists. Please choose another one.";
                    ViewBag.Roles = fixedRole == null ? new List<string> { "Admin", "Teacher", "Student" } : null;
                    return View(model);
                }

                model.CreatedAt = DateTime.Now;

                _context.Accounts.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Account {model.Username} ({model.Role}) was created successfully.";
                return RedirectToAction(fixedRole switch
                {
                    "Admin" => "AdminAccounts",
                    "Teacher" => "TeacherAccounts",
                    "Student" => "StudentAccounts",
                    _ => "ListAccounts"
                });
            }

            ViewBag.Roles = fixedRole == null ? new List<string> { "Admin", "Teacher", "Student" } : null;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditAccount(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            ViewBag.Roles = new List<string> { "Admin", "Teacher", "Student" };
            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(int id, AuthApp.Models.Account model)
        {
            // Password can stay unchanged, so skip validation when empty
            ModelState.Remove("Password");

            if (id != model.Id)
            {
                return NotFound();
            }

            try
            {
                var accountToUpdate = await _context.Accounts.FindAsync(id);

                if (accountToUpdate == null)
                {
                    return NotFound();
                }

                accountToUpdate.Fullname = model.Fullname;
                accountToUpdate.Username = model.Username;
                accountToUpdate.Email = model.Email;
                accountToUpdate.PhoneNumber = model.PhoneNumber;
                accountToUpdate.Role = model.Role;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    // TODO: add password hashing before storing credentials
                    accountToUpdate.Password = model.Password;
                }

                _context.Update(accountToUpdate);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Account {accountToUpdate.Username} was updated successfully.";
                return RedirectToAction("ListAccounts");
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "A concurrency error occurred while saving. Please try again.";
            }

            ViewBag.Roles = new List<string> { "Admin", "Teacher", "Student" };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAccount(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        [HttpPost, ActionName("DeleteAccount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return RedirectToAction("ListAccounts");
            }

            if (User.FindFirstValue(ClaimTypes.NameIdentifier) == account.Id.ToString())
            {
                TempData["ErrorMessage"] = "You cannot delete the currently signed-in administrator.";
                return RedirectToAction("ListAccounts");
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Account {account.Username} was deleted successfully.";
            return RedirectToAction("ListAccounts");
        }
    }
}
