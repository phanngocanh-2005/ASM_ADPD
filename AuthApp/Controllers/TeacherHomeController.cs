using System;
using System.Linq;
using System.Security.Claims;
using AuthApp.Data;
using AuthApp.Models;
using AuthApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherHomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherHomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var teacher = await LoadTeacherAsync(includeRelations: true);
            if (teacher == null)
            {
                return HandleMissingProfile();
            }

            var dashboard = new TeacherDashboardViewModel
            {
                Teacher = teacher,
                TotalAssignments = teacher.CourseAssignments?.Count ?? 0,
                ActiveAssignments = teacher.CourseAssignments?.Count(ca => ca.Status == "Active") ?? 0,
                TotalGradedRecords = teacher.AcademicRecords?.Count ?? 0,
                UniqueStudents = teacher.AcademicRecords?
                    .Where(ar => ar.StudentId != 0)
                    .Select(ar => ar.StudentId)
                    .Distinct()
                    .Count() ?? 0,
                RecentAssignments = teacher.CourseAssignments?
                    .OrderByDescending(ca => ca.AssignmentDate)
                    .Take(5)
                    .ToList() ?? new List<CourseAssignment>(),
                RecentGrades = teacher.AcademicRecords?
                    .OrderByDescending(ar => ar.GradedDate ?? ar.CreatedAt)
                    .Take(5)
                    .ToList() ?? new List<AcademicRecord>()
            };

            return View(dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var teacher = await LoadTeacherAsync(includeRelations: true);
            if (teacher == null)
            {
                return HandleMissingProfile();
            }

            return View(teacher);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var teacher = await LoadTeacherAsync();
            if (teacher == null)
            {
                return HandleMissingProfile();
            }

            var viewModel = MapToEditViewModel(teacher);
            PopulateGenderOptions(teacher.Gender);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(TeacherProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateGenderOptions(model.Gender);
                return View(model);
            }

            var accountId = GetCurrentAccountId();
            var teacher = await _context.Teachers
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == model.Id && t.AccountId == accountId);

            if (teacher == null)
            {
                return HandleMissingProfile();
            }

            teacher.FullName = model.FullName;
            teacher.DateOfBirth = model.DateOfBirth;
            teacher.Gender = model.Gender;
            teacher.PhoneNumber = model.PhoneNumber;
            teacher.Email = model.Email;
            teacher.Department = model.Department;
            teacher.Specialization = model.Specialization;
            teacher.UpdatedAt = DateTime.UtcNow;

            if (teacher.Account != null && !string.IsNullOrWhiteSpace(model.AccountEmail))
            {
                teacher.Account.Email = model.AccountEmail!;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Your profile has been updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        private async Task<Teacher?> LoadTeacherAsync(bool includeRelations = false)
        {
            var accountId = GetCurrentAccountId();
            if (accountId == 0)
            {
                return null;
            }

            IQueryable<Teacher> query = _context.Teachers;

            if (includeRelations)
            {
                query = query
                    .Include(t => t.Account)
                    .Include(t => t.AcademicProgram)
                    .Include(t => t.CourseAssignments).ThenInclude(ca => ca.Course)
                    .Include(t => t.AcademicRecords).ThenInclude(ar => ar.Course)
                    .Include(t => t.AcademicRecords).ThenInclude(ar => ar.Student);
            }
            else
            {
                query = query
                    .Include(t => t.Account)
                    .Include(t => t.AcademicProgram);
            }

            return await query.FirstOrDefaultAsync(t => t.AccountId == accountId);
        }

        private IActionResult HandleMissingProfile()
        {
            TempData["ErrorMessage"] = "Teacher profile not found. Please contact the administrator.";
            return RedirectToAction("Index", "Home");
        }

        private int GetCurrentAccountId()
        {
            var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claimValue, out var accountId) ? accountId : 0;
        }

        private TeacherProfileEditViewModel MapToEditViewModel(Teacher teacher)
        {
            return new TeacherProfileEditViewModel
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                DateOfBirth = teacher.DateOfBirth,
                Gender = teacher.Gender,
                PhoneNumber = teacher.PhoneNumber,
                Email = teacher.Email ?? teacher.Account?.Email,
                Department = teacher.Department,
                Specialization = teacher.Specialization,
                AccountEmail = teacher.Account?.Email
            };
        }

        private void PopulateGenderOptions(string? selectedValue)
        {
            ViewBag.GenderOptions = new List<SelectListItem>
            {
                new("Male", "Male"),
                new("Female", "Female"),
                new("Other", "Other")
            }.Select(item =>
            {
                item.Selected = item.Value == selectedValue;
                return item;
            }).ToList();
        }
    }
}
