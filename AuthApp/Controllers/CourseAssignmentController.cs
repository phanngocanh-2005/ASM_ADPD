using System;
using System.Linq;
using System.Threading.Tasks;
using AuthApp.Data;
using AuthApp.Models;
using AuthApp.Patterns.Factory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Controllers
{
    // Controller quản lý việc phân công môn học cho giáo viên (CourseAssignment)
    [Authorize(Roles = "Admin")]
    public class CourseAssignmentController : Controller
    {
        // DbContext làm việc với database
        private readonly ApplicationDbContext _context;

        public CourseAssignmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách các phân công môn học
        public async Task<IActionResult> Index()
        {
            var assignments = await _context.CourseAssignments
                .Include(ca => ca.Teacher)
                .Include(ca => ca.Course)
                .OrderByDescending(ca => ca.AssignmentDate)
                .ToListAsync();
                
            return View(assignments);
        }

        // Hiển thị form tạo mới phân công môn học
        public async Task<IActionResult> Create()
        {
            ViewBag.Teachers = await _context.Teachers
                .Where(t => t.Status == "Active")
                .OrderBy(t => t.FullName)
                .ToListAsync();
            ViewBag.Courses = await _context.Courses
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.CourseCode)
                .ToListAsync();
            return View();
        }

        // Xử lý submit tạo mới phân công môn học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeacherId,CourseId,Status")] CourseAssignment assignment)
        {
            // Chỉ xử lý khi ModelState hợp lệ (đã chọn Teacher, Course, Status)
            if (ModelState.IsValid)
            {
                try
                {
                    // Gán ngày phân công + ngày tạo
                    assignment.AssignmentDate = DateTime.Now;
                    assignment.CreatedAt = DateTime.Now;

                    // Thêm bản ghi vào DbContext và lưu xuống DB
                    _context.Add(assignment);
                    await _context.SaveChangesAsync();

                    // Tạo thông báo thành công bằng Factory Method
                    var notification = NotificationFactory.CreateSuccess(
                        "Course assigned successfully! The teacher can now view this course in Grade Management. " +
                        "Remember to create a schedule for this course so the teacher can see it in their weekly schedule.");
                    TempData["SuccessMessage"] = notification.Message;
                    return RedirectToAction(nameof(Index));
                }
                // Bắt lỗi khi lưu DB (ví dụ trùng phân công, vi phạm FK, ...)
                catch (DbUpdateException ex)
                {
                    var error = NotificationFactory.CreateError(ex.InnerException?.Message ?? ex.Message);
                    ModelState.AddModelError(string.Empty, $"Could not save course assignment: {error.Message}");
                }
            }

            ViewBag.Teachers = await _context.Teachers
                .Where(t => t.Status == "Active")
                .OrderBy(t => t.FullName)
                .ToListAsync();
            ViewBag.Courses = await _context.Courses
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.CourseCode)
                .ToListAsync();
            return View(assignment);
        }
    }
}
