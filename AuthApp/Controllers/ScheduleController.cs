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

                // Load số lượng sinh viên cho mỗi course
                var courseIds = schedules.Select(s => s.CourseId).Distinct().ToList();
                Dictionary<int, int> studentCounts = new Dictionary<int, int>();
                
                if (courseIds.Any())
                {
                    studentCounts = await _context.Enrollments
                        .Where(e => courseIds.Contains(e.CourseId) && e.Status == "Enrolled")
                        .GroupBy(e => e.CourseId)
                        .Select(g => new { CourseId = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.CourseId, x => x.Count);
                }

                ViewBag.StudentCounts = studentCounts;

                return View(schedules);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("Invalid object name 'Schedules'"))
            {
                TempData["ErrorMessage"] = "The Schedules table does not exist in the database. Please run the database setup script: CreateSchedulesTable.sql";
                return View(new List<Schedule>());
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("Invalid column name 'StudentId'"))
            {
                // Try to automatically add the StudentId column
                try
                {
                    await EnsureStudentIdColumnExists();
                    // Retry the query after adding the column
                    var schedules = await _context.Schedules
                        .Include(s => s.Course)
                        .Include(s => s.Teacher)
                        .OrderBy(s => s.DayOfWeek)
                        .ThenBy(s => s.StartTime)
                        .ToListAsync();

                    // Load số lượng sinh viên cho mỗi course
                    var courseIds = schedules.Select(s => s.CourseId).Distinct().ToList();
                    Dictionary<int, int> studentCounts = new Dictionary<int, int>();
                    
                    if (courseIds.Any())
                    {
                        studentCounts = await _context.Enrollments
                            .Where(e => courseIds.Contains(e.CourseId) && e.Status == "Enrolled")
                            .GroupBy(e => e.CourseId)
                            .Select(g => new { CourseId = g.Key, Count = g.Count() })
                            .ToDictionaryAsync(x => x.CourseId, x => x.Count);
                    }

                    ViewBag.StudentCounts = studentCounts;

                    TempData["SuccessMessage"] = "Database schema updated successfully.";
                    return View(schedules);
                }
                catch (Exception innerEx)
                {
                    TempData["ErrorMessage"] = $"Failed to add StudentId column. Please run the SQL script AddStudentIdToSchedules.sql manually. Error: {innerEx.Message}";
                    return View(new List<Schedule>());
                }
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
                await PopulateScheduleLookups(model.CourseId, model.TeacherId, model.StudentId);
                return View(model);
            }

            // Kiểm tra logic: giờ kết thúc phải sau giờ bắt đầu
            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(nameof(Schedule.EndTime), "End time must be after start time.");
                await PopulateScheduleLookups(model.CourseId, model.TeacherId, model.StudentId);
                return View(model);
            }

            try
            {
                // Gán thời điểm tạo, thêm vào DbContext rồi lưu xuống DB
                model.CreatedAt = DateTime.UtcNow;
                _context.Schedules.Add(model);
                await _context.SaveChangesAsync();

                // Xử lý danh sách mã sinh viên từ form
                var studentCodes = Request.Form["StudentCodes"].ToString();
                if (!string.IsNullOrWhiteSpace(studentCodes))
                {
                    var codes = studentCodes.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .ToList();

                    int enrolledCount = 0;
                    foreach (var code in codes)
                    {
                        var student = await _context.Students
                            .FirstOrDefaultAsync(s => s.StudentCode == code && s.Status == "Active");

                        if (student != null)
                        {
                            // Kiểm tra xem enrollment đã tồn tại chưa
                            var existingEnrollment = await _context.Enrollments
                                .FirstOrDefaultAsync(e => e.StudentId == student.Id && 
                                                         e.CourseId == model.CourseId && 
                                                         e.Status == "Enrolled");

                            if (existingEnrollment == null)
                            {
                                var enrollment = new Enrollment
                                {
                                    StudentId = student.Id,
                                    CourseId = model.CourseId,
                                    EnrollmentDate = DateTime.UtcNow,
                                    Status = "Enrolled",
                                    CreatedAt = DateTime.UtcNow
                                };
                                _context.Enrollments.Add(enrollment);
                                enrolledCount++;
                            }
                        }
                    }

                    if (enrolledCount > 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }

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
                await PopulateScheduleLookups(model.CourseId, model.TeacherId, model.StudentId);
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

            var schedule = await _context.Schedules
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (schedule == null)
            {
                return NotFound();
            }

            // Load danh sách sinh viên đã enroll vào course này
            var enrolledStudents = await _context.Enrollments
                .Where(e => e.CourseId == schedule.CourseId && e.Status == "Enrolled")
                .Include(e => e.Student)
                .Select(e => new
                {
                    Id = e.Student.Id,
                    Code = e.Student.StudentCode,
                    Name = e.Student.FullName,
                    EnrollmentId = e.Id
                })
                .OrderBy(s => s.Code)
                .ToListAsync();

            ViewBag.EnrolledStudents = enrolledStudents;

            await PopulateScheduleLookups(schedule.CourseId, schedule.TeacherId, schedule.StudentId);

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
                await PopulateScheduleLookups(model.CourseId, model.TeacherId, model.StudentId);
                return View(model);
            }

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(nameof(Schedule.EndTime), "End time must be after start time.");
                await PopulateScheduleLookups(model.CourseId, model.TeacherId, model.StudentId);
                return View(model);
            }

            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            schedule.CourseId = model.CourseId;
            schedule.TeacherId = model.TeacherId;
            schedule.StudentId = model.StudentId;
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

            // Xử lý danh sách mã sinh viên từ form
            var studentCodes = Request.Form["StudentCodes"].ToString();
            if (!string.IsNullOrWhiteSpace(studentCodes))
            {
                var codes = studentCodes.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .ToList();

                int enrolledCount = 0;
                foreach (var code in codes)
                {
                    var student = await _context.Students
                        .FirstOrDefaultAsync(s => s.StudentCode == code && s.Status == "Active");

                    if (student != null)
                    {
                        // Kiểm tra xem enrollment đã tồn tại chưa
                        var existingEnrollment = await _context.Enrollments
                            .FirstOrDefaultAsync(e => e.StudentId == student.Id && 
                                                     e.CourseId == schedule.CourseId && 
                                                     e.Status == "Enrolled");

                        if (existingEnrollment == null)
                        {
                            var enrollment = new Enrollment
                            {
                                StudentId = student.Id,
                                CourseId = schedule.CourseId,
                                EnrollmentDate = DateTime.UtcNow,
                                Status = "Enrolled",
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Enrollments.Add(enrollment);
                            enrolledCount++;
                        }
                    }
                }

                if (enrolledCount > 0)
                {
                    await _context.SaveChangesAsync();
                }
            }

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

        private async Task PopulateScheduleLookups(int? selectedCourseId = null, int? selectedTeacherId = null, int? selectedStudentId = null)
        {
            await PopulateCourseLookups(selectedCourseId);
            await PopulateTeacherLookups(selectedTeacherId);
            await PopulateStudentLookups(selectedStudentId);
            PopulateDayOfWeekOptions();
            PopulateClassTypeOptions();
            PopulateStatusOptions();
            PopulateTimeSlotOptions();
        }

        private async Task PopulateStudentLookups(int? selectedStudentId = null)
        {
            ViewBag.Students = await _context.Students
                .Where(s => s.Status == "Active" || (selectedStudentId != null && s.Id == selectedStudentId))
                .OrderBy(s => s.StudentCode)
                .Select(s => new SelectListItem($"{s.StudentCode} - {s.FullName}", s.Id.ToString()))
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

        private async Task EnsureStudentIdColumnExists()
        {
            const string sql = @"
IF COL_LENGTH('Schedules', 'StudentId') IS NULL
BEGIN
    ALTER TABLE Schedules
    ADD StudentId INT NULL;

    ALTER TABLE Schedules
    ADD CONSTRAINT FK_Schedules_Students_StudentId
        FOREIGN KEY (StudentId) REFERENCES Students(Id)
        ON DELETE SET NULL;

    CREATE INDEX IX_Schedules_StudentId
        ON Schedules(StudentId);
END";

            await _context.Database.ExecuteSqlRawAsync(sql);
        }

        // API endpoint để tìm kiếm sinh viên theo mã
        [HttpGet]
        public async Task<IActionResult> SearchStudentByCode(string studentCode)
        {
            if (string.IsNullOrWhiteSpace(studentCode))
            {
                return Json(new { success = false, message = "Student code is required" });
            }

            var student = await _context.Students
                .Where(s => s.StudentCode == studentCode.Trim() && s.Status == "Active")
                .Select(s => new
                {
                    id = s.Id,
                    code = s.StudentCode,
                    name = s.FullName
                })
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return Json(new { success = false, message = "Student not found" });
            }

            return Json(new { success = true, student = student });
        }

        // Xóa sinh viên khỏi course (xóa enrollment)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudentFromCourse(int enrollmentId, int scheduleId)
        {
            try
            {
                var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
                if (enrollment == null)
                {
                    return Json(new { success = false, message = "Enrollment not found" });
                }

                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Student removed from course successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}

