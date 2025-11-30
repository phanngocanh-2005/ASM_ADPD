using AuthApp.Data;
using AuthApp.Models;
using AuthApp.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var student = await _context.Students
                .Include(s => s.Account)
                .Include(s => s.AcademicProgram)
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student == null)
            {
                // If the current account has no linked student profile,
                // redirect the user to create their own profile.
                TempData["ErrorMessage"] = "Student profile not found. Please create your profile.";
                return RedirectToAction(nameof(CreateProfile));
            }

            return View("Profile", student);
        }

        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var student = await _context.Students
                .Include(s => s.AcademicProgram)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found. Please create your profile first.";
                return RedirectToAction(nameof(CreateProfile));
            }

            // Explicitly return Courses view
            return View("Courses", student);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProfile()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (existingStudent != null)
            {
                // Profile already exists, redirect to edit profile instead
                TempData["InfoMessage"] = "You already have a profile. You can edit it here.";
                return RedirectToAction(nameof(EditProfile));
            }

            await PopulateStudentLookups();

            var model = new Student
            {
                AccountId = accountId,
                EnrollmentDate = DateTime.UtcNow,
                Status = "Active",
                DateOfBirth = DateTime.UtcNow.AddYears(-18)
            };

            return View("CreateProfile", model);
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
                return View("CreateProfile", model);
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
                return View("CreateProfile", model);
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
            return View("EditProfile", student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Student model)
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            var student = await _context.Students
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.AccountId == accountId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction("Profile");
            }

            // Remove validation for fields that shouldn't be changed
            ModelState.Remove(nameof(Student.AccountId));
            ModelState.Remove(nameof(Student.StudentCode));
            ModelState.Remove(nameof(Student.AcademicProgramId));
            ModelState.Remove(nameof(Student.EnrollmentDate));
            ModelState.Remove(nameof(Student.Status));
            ModelState.Remove(nameof(Student.GPA));
            ModelState.Remove(nameof(Student.CreatedAt));

            if (!ModelState.IsValid)
            {
                // Reload the student with all related data for the view
                student = await _context.Students
                    .Include(s => s.Account)
                    .Include(s => s.AcademicProgram)
                    .FirstOrDefaultAsync(s => s.AccountId == accountId);
                await PopulateStudentLookups();
                return View("EditProfile", student);
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
                if (account != null)
                {
                    // Get email from form if provided
                    if (Request.Form.ContainsKey("Account.Email"))
                    {
                        var email = Request.Form["Account.Email"].ToString();
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            account.Email = email;
                            _context.Update(account);
                        }
                    }
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

            return View("DeleteProfile", student);
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

        [HttpGet]
        public async Task<IActionResult> Schedule()
        {
            try
            {
                var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                var student = await _context.Students
                    .Include(s => s.Enrollments)
                        .ThenInclude(e => e.Course)
                    .FirstOrDefaultAsync(s => s.AccountId == accountId);

                if (student == null)
                {
                    TempData["ErrorMessage"] = "Student profile not found. Please create your profile first.";
                    return RedirectToAction(nameof(CreateProfile));
                }

                // Get all schedules for enrolled courses
                var enrolledCourseIds = student.Enrollments
                    .Where(e => e.Status == "Enrolled")
                    .Select(e => e.CourseId)
                    .ToList();

                var schedules = await _context.Schedules
                    .Include(s => s.Course)
                    .Where(s => enrolledCourseIds.Contains(s.CourseId) && s.Status == "Active")
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .ToListAsync();

                // Explicitly return Schedule view with schedules data
                return View("Schedule", schedules);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("Invalid object name 'Schedules'"))
            {
                TempData["ErrorMessage"] = "The Schedules table does not exist in the database. Please contact the administrator to run the database setup script (CreateSchedulesTable.sql).";
                return View("Schedule", new List<Schedule>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading your schedule: {ex.Message}";
                return View("Schedule", new List<Schedule>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Grades()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var student = await _context.Students
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

            // Group grades by course
            var gradesByCourse = student.AcademicRecords
                .GroupBy(ar => ar.Course)
                .Select(g => new CourseGradesViewModel
                {
                    Course = g.Key!,
                    Records = g.OrderByDescending(r => r.GradedDate ?? r.CreatedAt).ToList(),
                    AverageScore = g.Average(r => r.Score),
                    TotalRecords = g.Count()
                })
                .OrderBy(g => g.Course?.CourseCode)
                .ToList();

            var viewModel = new StudentGradesViewModel
            {
                Student = student,
                GradesByCourse = gradesByCourse
            };

            // Explicitly return Grades view with grades data
            return View("Grades", viewModel);
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
