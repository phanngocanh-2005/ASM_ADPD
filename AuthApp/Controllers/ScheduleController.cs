using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthApp.Data;
using AuthApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all schedules
        public async Task<IActionResult> Index()
        {
            try
            {
                var schedules = await _context.Schedules
                    .Include(s => s.Course)
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .ToListAsync();

                return View(schedules);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("Invalid object name 'Schedules'"))
            {
                TempData["ErrorMessage"] = "The Schedules table does not exist in the database. Please run the database setup script: CreateSchedulesTable.sql";
                return View(new List<Schedule>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(new List<Schedule>());
            }
        }

        // Create new schedule
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateCourseLookups();
            PopulateDayOfWeekOptions();
            PopulateClassTypeOptions();
            PopulateStatusOptions();

            return View(new Schedule
            {
                Status = "Active",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(9, 30, 0)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCourseLookups();
                PopulateDayOfWeekOptions();
                PopulateClassTypeOptions();
                PopulateStatusOptions();
                return View(model);
            }

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(nameof(Schedule.EndTime), "End time must be after start time.");
                await PopulateCourseLookups();
                PopulateDayOfWeekOptions();
                PopulateClassTypeOptions();
                PopulateStatusOptions();
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            _context.Schedules.Add(model);
            await _context.SaveChangesAsync();

            // Check if any teachers are assigned to this course
            var assignedTeachers = await _context.CourseAssignments
                .Where(ca => ca.CourseId == model.CourseId && ca.Status == "Active")
                .Include(ca => ca.Teacher)
                .Select(ca => ca.Teacher.FullName)
                .ToListAsync();

            var message = "Schedule created successfully.";
            if (assignedTeachers.Any())
            {
                message += $" Teachers assigned to this course ({string.Join(", ", assignedTeachers)}) will now see this schedule in their weekly view.";
            }
            else
            {
                message += " Note: No teachers are currently assigned to this course. Assign a teacher via Course Assignment for them to see this schedule.";
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // Edit schedule
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            await PopulateCourseLookups();
            PopulateDayOfWeekOptions();
            PopulateClassTypeOptions();
            PopulateStatusOptions();

            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Schedule model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateCourseLookups();
                PopulateDayOfWeekOptions();
                PopulateClassTypeOptions();
                PopulateStatusOptions();
                return View(model);
            }

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(nameof(Schedule.EndTime), "End time must be after start time.");
                await PopulateCourseLookups();
                PopulateDayOfWeekOptions();
                PopulateClassTypeOptions();
                PopulateStatusOptions();
                return View(model);
            }

            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            schedule.CourseId = model.CourseId;
            schedule.DayOfWeek = model.DayOfWeek;
            schedule.StartTime = model.StartTime;
            schedule.EndTime = model.EndTime;
            schedule.Room = model.Room;
            schedule.Building = model.Building;
            schedule.ClassType = model.ClassType;
            schedule.Status = model.Status;
            schedule.UpdatedAt = DateTime.UtcNow;

            _context.Update(schedule);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Schedule updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Delete schedule
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Schedule deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCourseLookups()
        {
            ViewBag.Courses = await _context.Courses
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.CourseCode)
                .Select(c => new SelectListItem($"{c.CourseCode} - {c.CourseName}", c.Id.ToString()))
                .ToListAsync();
        }

        private void PopulateDayOfWeekOptions()
        {
            ViewBag.DayOfWeekOptions = new[]
            {
                new SelectListItem("Monday", "Monday"),
                new SelectListItem("Tuesday", "Tuesday"),
                new SelectListItem("Wednesday", "Wednesday"),
                new SelectListItem("Thursday", "Thursday"),
                new SelectListItem("Friday", "Friday"),
                new SelectListItem("Saturday", "Saturday"),
                new SelectListItem("Sunday", "Sunday")
            };
        }

        private void PopulateClassTypeOptions()
        {
            ViewBag.ClassTypeOptions = new[]
            {
                new SelectListItem("Lecture", "Lecture"),
                new SelectListItem("Lab", "Lab"),
                new SelectListItem("Tutorial", "Tutorial"),
                new SelectListItem("Seminar", "Seminar"),
                new SelectListItem("Workshop", "Workshop")
            };
        }

        private void PopulateStatusOptions()
        {
            ViewBag.StatusOptions = new[]
            {
                new SelectListItem("Active", "Active"),
                new SelectListItem("Inactive", "Inactive")
            };
        }
    }
}

