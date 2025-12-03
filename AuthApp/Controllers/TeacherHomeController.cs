using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AuthApp.Data;
using AuthApp.Models;
using AuthApp.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
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
            try
            {
                var teacherId = await GetCurrentTeacherId();
                if (teacherId == 0)
                {
                    TempData["ErrorMessage"] = "Teacher profile not found. Please contact the administrator to create your profile.";
                    return View();
                }

                // Dashboard page - no data loading needed
                return View();
            }
            catch (Exception ex)
            {
                // Log the exception (you can add logging here)
                TempData["ErrorMessage"] = "An error occurred while loading your page. Please try again.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var teacher = await LoadTeacherAsync(includeRelations: true);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher profile not found. Please contact the administrator to create your profile.";
                return RedirectToAction("Index", "TeacherHome");
            }

            return View(teacher);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var teacher = await LoadTeacherAsync();
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher profile not found. Please contact the administrator to create your profile.";
                return RedirectToAction("Index", "TeacherHome");
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
                TempData["ErrorMessage"] = "Teacher profile not found. Please contact the administrator to create your profile.";
                return RedirectToAction("Index", "TeacherHome");
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

        [HttpGet]
        public async Task<IActionResult> Schedule()
        {
            try
            {
                var teacherId = await GetCurrentTeacherId();
                if (teacherId == 0)
                {
                    TempData["ErrorMessage"] = "Teacher profile not found. Please contact the administrator to create your profile.";
                    return RedirectToAction("Index", "TeacherHome");
                }

                // Load schedules directly from Schedule Management table,
                // filtered by the current teacher and active status.
                var schedules = await _context.Schedules
                    .Include(s => s.Course)
                    .Where(s => s.TeacherId == teacherId && s.Status == "Active")
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .ToListAsync();

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

        private async Task<int> GetCurrentTeacherId()
        {
            var accountId = GetCurrentAccountId();
            if (accountId == 0)
            {
                return 0;
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.AccountId == accountId);

            return teacher?.Id ?? 0;
        }

        private async Task<Teacher?> LoadTeacherAsync(bool includeRelations = false)
        {
            try
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
            catch (Exception)
            {
                // Return null if there's an error loading teacher
                // The calling method will handle the null case
                return null;
            }
        }

        private IActionResult HandleMissingProfile()
        {
            // Set error message and redirect to Home
            // HomeController will check for error message and not redirect back
            TempData["ErrorMessage"] = "Teacher profile not found. Please contact the administrator to create your profile.";
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
