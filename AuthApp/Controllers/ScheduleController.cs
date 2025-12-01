using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthApp.Data;
using AuthApp.Models;
using AuthApp.Patterns.Adapter;
using AuthApp.Patterns.Factory;
using AuthApp.Patterns.Singleton;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Controllers
{
    // Controller quản lý Thời khóa biểu (Schedule) cho Admin
    [Authorize(Roles = "Admin")]
    public class ScheduleController : Controller
    {
        // DbContext làm việc với database
        private readonly ApplicationDbContext _context;
        // Adapter pattern: chuyển Teacher -> SelectListItem cho dropdown
        private readonly ITeacherSelectListAdapter _teacherAdapter = new TeacherSelectListAdapter();

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
                    .Include(s => s.Teacher)
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

        // Hiển thị form tạo mới Schedule
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Load danh sách Course, Teacher, DayOfWeek,... cho dropdown
            await PopulateScheduleLookups();

            // Lấy slot mặc định từ Singleton (slot đầu tiên trong ngày)
            var defaultSlot = ScheduleSlotSingleton.Instance.GetStandardSlots().First();
            return View(new Schedule
            {
                Status = "Active",
                StartTime = defaultSlot.Start,
                EndTime = defaultSlot.End
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Xử lý submit form tạo mới Schedule
        public async Task<IActionResult> Create(Schedule model)
        {
            // Nếu model không hợp lệ (thiếu dữ liệu, sai kiểu, ...)
            if (!ModelState.IsValid)
            {
                await PopulateScheduleLookups(model.CourseId, model.TeacherId);
                return View(model);
            }

            // Kiểm tra logic: giờ kết thúc phải sau giờ bắt đầu
            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(nameof(Schedule.EndTime), "End time must be after start time.");
                await PopulateScheduleLookups(model.CourseId, model.TeacherId);
                return View(model);
            }

            try
            {
                // Gán thời điểm tạo, thêm vào DbContext rồi lưu xuống DB
                model.CreatedAt = DateTime.UtcNow;
                _context.Schedules.Add(model);
                await _context.SaveChangesAsync();

                // Lấy tên giáo viên & kiểm tra xem giáo viên đã được gán môn chưa
                var teacherName = await _context.Teachers
                    .Where(t => t.Id == model.TeacherId)
                    .Select(t => t.FullName)
                    .FirstOrDefaultAsync();

                var isTeacherAssignedToCourse = await _context.CourseAssignments
                    .AnyAsync(ca => ca.CourseId == model.CourseId && ca.TeacherId == model.TeacherId && ca.Status == "Active");

                // Tạo message cơ bản
                var baseMessage = $"Schedule created successfully for {(teacherName ?? "the selected teacher")}.";
                if (!isTeacherAssignedToCourse)
                {
                    baseMessage += " Note: this teacher is not yet linked to the course via Course Assignment.";
                }

                // Dùng Factory Method để tạo thông báo thành công
                var notification = NotificationFactory.CreateSuccess(baseMessage);
                TempData["SuccessMessage"] = notification.Message;
                return RedirectToAction(nameof(Index));
            }
            // Bắt lỗi khi lưu DB (vi phạm FK, constraint, ...)
            catch (DbUpdateException ex)
            {
                var error = NotificationFactory.CreateError(ex.InnerException?.Message ?? ex.Message);
                ModelState.AddModelError(string.Empty, $"Could not save schedule: {error.Message}");
                await PopulateScheduleLookups(model.CourseId, model.TeacherId);
                return View(model);
            }
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

            await PopulateScheduleLookups(schedule.CourseId, schedule.TeacherId);

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
                await PopulateScheduleLookups(model.CourseId, model.TeacherId);
                return View(model);
            }

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(nameof(Schedule.EndTime), "End time must be after start time.");
                await PopulateScheduleLookups(model.CourseId, model.TeacherId);
                return View(model);
            }

            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            schedule.CourseId = model.CourseId;
            schedule.TeacherId = model.TeacherId;
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
                .Include(s => s.Teacher)
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

        private async Task PopulateCourseLookups(int? selectedCourseId = null)
        {
            ViewBag.Courses = await _context.Courses
                .Where(c => c.Status == "Active" || (selectedCourseId != null && c.Id == selectedCourseId))
                .OrderBy(c => c.CourseCode)
                .Select(c => new SelectListItem($"{c.CourseCode} - {c.CourseName}", c.Id.ToString()))
                .ToListAsync();
        }

        private async Task PopulateTeacherLookups(int? selectedTeacherId = null)
        {
            var teachers = await _context.Teachers
                .Where(t => t.Status == "Active" || (selectedTeacherId != null && t.Id == selectedTeacherId))
                .ToListAsync();

            ViewBag.Teachers = _teacherAdapter.Adapt(teachers).ToList();
        }

        private async Task PopulateScheduleLookups(int? selectedCourseId = null, int? selectedTeacherId = null)
        {
            await PopulateCourseLookups(selectedCourseId);
            await PopulateTeacherLookups(selectedTeacherId);
            PopulateDayOfWeekOptions();
            PopulateClassTypeOptions();
            PopulateStatusOptions();
            PopulateTimeSlotOptions();
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

        private void PopulateTimeSlotOptions()
        {
            var standardSlots = ScheduleSlotSingleton.Instance.GetStandardSlots();

            ViewBag.TimeSlotOptions = standardSlots
                .Select(slot => new SelectListItem(
                    $"{slot.Start:hh\\:mm} - {slot.End:hh\\:mm}",
                    $"{slot.Start:hh\\:mm}-{slot.End:hh\\:mm}"))
                .ToList();
        }

        // Time slots là gợi ý từ Singleton, không bắt buộc chính xác; chỉ cần EndTime > StartTime.
    }
}

