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
            await PopulateAssignmentLookups();
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

            await PopulateAssignmentLookups(assignment.TeacherId, assignment.CourseId);
            return View(assignment);
        }

        // Hiển thị form chỉnh sửa một phân công môn học
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.CourseAssignments
                .Include(ca => ca.Teacher)
                .Include(ca => ca.Course)
                .FirstOrDefaultAsync(ca => ca.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            await PopulateAssignmentLookups(assignment.TeacherId, assignment.CourseId);
            return View(assignment);
        }

        // Xử lý submit chỉnh sửa phân công môn học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeacherId,CourseId,Status")] CourseAssignment model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateAssignmentLookups(model.TeacherId, model.CourseId);
                return View(model);
            }

            var existing = await _context.CourseAssignments.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.TeacherId = model.TeacherId;
            existing.CourseId = model.CourseId;
            existing.Status = model.Status;

            try
            {
                _context.Update(existing);
                await _context.SaveChangesAsync();

                var notification = NotificationFactory.CreateSuccess("Course assignment updated successfully.");
                TempData["SuccessMessage"] = notification.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                var error = NotificationFactory.CreateError(ex.InnerException?.Message ?? ex.Message);
                ModelState.AddModelError(string.Empty, $"Could not update course assignment: {error.Message}");
                await PopulateAssignmentLookups(model.TeacherId, model.CourseId);
                return View(model);
            }
        }

        // Hiển thị xác nhận xóa phân công môn học
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.CourseAssignments
                .Include(ca => ca.Teacher)
                .Include(ca => ca.Course)
                .FirstOrDefaultAsync(ca => ca.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // Xử lý xóa phân công môn học
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.CourseAssignments.FindAsync(id);
            if (assignment == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.CourseAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            var notification = NotificationFactory.CreateSuccess("Course assignment deleted successfully.");
            TempData["SuccessMessage"] = notification.Message;
            return RedirectToAction(nameof(Index));
        }

        // Load danh sách Teacher / Course cho các dropdown của CourseAssignment
        private async Task PopulateAssignmentLookups(int? selectedTeacherId = null, int? selectedCourseId = null)
        {
            ViewBag.Teachers = await _context.Teachers
                .Where(t => t.Status == "Active" || (selectedTeacherId != null && t.Id == selectedTeacherId))
                .OrderBy(t => t.FullName)
                .ToListAsync();

            ViewBag.Courses = await _context.Courses
                .Where(c => c.Status == "Active" || (selectedCourseId != null && c.Id == selectedCourseId))
                .OrderBy(c => c.CourseCode)
                .ToListAsync();
        }
    }
}
