using AuthApp.Data;
using AuthApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthApp.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentHomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentHomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var student = await _context.Students
                .Include(s => s.Account)
                .Include(s => s.AcademicProgram)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.AcademicRecords)
                    .ThenInclude(ar => ar.Course)
                .Include(s => s.AcademicRecords)
                    .ThenInclude(ar => ar.Teacher)
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student == null)
            {
                // If the current account has no linked student profile,
                // redirect the user to create their own profile.
                TempData["ErrorMessage"] = "Student profile not found. Please create your profile.";
                return RedirectToAction(nameof(CreateProfile));
            }

            return View(student);
        }

        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var student = await _context.Students
                .Include(s => s.AcademicProgram)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.AcademicRecords)
                    .ThenInclude(ar => ar.Course)
                .Include(s => s.AcademicRecords)
                    .ThenInclude(ar => ar.Teacher)
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found. Please create your profile first.";
                return RedirectToAction(nameof(CreateProfile));
            }

            return View(student);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProfile()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (existingStudent != null)
            {
                // Profile already exists, go back to profile page
                return RedirectToAction(nameof(Profile));
            }

            await PopulateStudentLookups();

            var model = new Student
            {
                AccountId = accountId,
                EnrollmentDate = DateTime.UtcNow,
                Status = "Active",
                DateOfBirth = DateTime.UtcNow.AddYears(-18)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(Student model)
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Force-link to the currently logged-in account
            model.AccountId = accountId;

            if (!ModelState.IsValid)
            {
                await PopulateStudentLookups();
                return View(model);
            }

            if (!model.AcademicProgramId.HasValue || model.AcademicProgramId.Value <= 0)
            {
                model.AcademicProgramId = null;
            }

            // Ensure this account does not already have a student profile
            var existingForAccount = await _context.Students
                .AnyAsync(s => s.AccountId == accountId);

            if (existingForAccount)
            {
                TempData["ErrorMessage"] = "A student profile already exists for this account.";
                return RedirectToAction(nameof(Profile));
            }

            // Ensure StudentCode is unique
            var duplicateCode = await _context.Students
                .AnyAsync(s => s.StudentCode == model.StudentCode);

            if (duplicateCode)
            {
                ModelState.AddModelError(nameof(Student.StudentCode),
                    "Student Code already exists. Please use a different code.");
                await PopulateStudentLookups();
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;

            _context.Students.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your student profile has been created successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            var student = await _context.Students
                .Include(s => s.Account)
                .Include(s => s.AcademicProgram)
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found. Please contact administrator.";
                return RedirectToAction("Profile");
            }

            await PopulateStudentLookups();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Student model)
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            if (!ModelState.IsValid)
            {
                await PopulateStudentLookups();
                return View(model);
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction("Profile");
            }

            // Only allow editing certain fields (not AccountId, StudentCode, AcademicProgramId)
            student.FullName = model.FullName;
            student.DateOfBirth = model.DateOfBirth;
            student.Gender = model.Gender;
            student.PhoneNumber = model.PhoneNumber;
            student.Address = model.Address;
            student.UpdatedAt = DateTime.UtcNow;

            // Update account email if provided
            if (student.AccountId.HasValue)
            {
                var account = await _context.Accounts.FindAsync(student.AccountId.Value);
                if (account != null && !string.IsNullOrEmpty(model.Account?.Email))
                {
                    account.Email = model.Account.Email;
                }
            }

            _context.Update(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your profile has been updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProfile()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            var student = await _context.Students
                .Include(s => s.Account)
                .Include(s => s.AcademicProgram)
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction("Profile");
            }

            return View(student);
        }

        [HttpPost, ActionName("DeleteProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfileConfirmed()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction("Profile");
            }

            var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.StudentId == student.Id);
            if (hasEnrollments)
            {
                TempData["ErrorMessage"] = "Cannot delete profile. You have enrollment history. Please contact administrator.";
                return RedirectToAction("Profile");
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your profile has been deleted successfully.";
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }

        private async Task PopulateStudentLookups()
        {
            ViewBag.AcademicPrograms = await _context.AcademicPrograms
                .Where(p => p.Status == "Active")
                .OrderBy(p => p.ProgramName)
                .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.ProgramCode} - {p.ProgramName}"
                })
                .ToListAsync();

            ViewBag.StudentStatuses = new[]
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Active", "Active"),
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Graduated", "Graduated"),
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Suspended", "Suspended"),
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Withdrawn", "Withdrawn")
            };

            ViewBag.Genders = new[]
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Male", "Male"),
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Female", "Female"),
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem("Other", "Other")
            };
        }
    }
}
