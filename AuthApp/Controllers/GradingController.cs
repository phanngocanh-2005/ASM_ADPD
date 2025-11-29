using System;
using System.Linq;
using System.Threading.Tasks;
using AuthApp.Data;
using AuthApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class GradingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GradingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách các khóa học đang dạy
        public async Task<IActionResult> Index()
        {
            var teacherId = await GetCurrentTeacherId();
            if (teacherId == 0)
            {
                return RedirectToAction("Index", "TeacherHome");
            }

            var courses = await _context.CourseAssignments
                .Where(ca => ca.TeacherId == teacherId && ca.Status == "Active")
                .Include(ca => ca.Course)
                .Select(ca => ca.Course)
                .ToListAsync();

            return View(courses);
        }

        // Hiển thị danh sách sinh viên trong một khóa học để nhập điểm
        public async Task<IActionResult> GradeStudents(int courseId)
        {
            var teacherId = await GetCurrentTeacherId();
            if (teacherId == 0)
            {
                return RedirectToAction("Index", "TeacherHome");
            }

            // Kiểm tra xem giáo viên có được phân công dạy khóa học này không
            var isAssigned = await _context.CourseAssignments
                .AnyAsync(ca => ca.TeacherId == teacherId && ca.CourseId == courseId && ca.Status == "Active");

            if (!isAssigned)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào khóa học này.";
                return RedirectToAction(nameof(Index));
            }

            var course = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound();
            }

            // Lấy danh sách điểm hiện có
            var grades = await _context.AcademicRecords
                .Where(ar => ar.CourseId == courseId && ar.TeacherId == teacherId)
                .ToListAsync();

            var viewModel = new GradeStudentsViewModel
            {
                CourseId = courseId,
                CourseName = course.CourseName,
                Students = course.Enrollments.Select(e => new StudentGradeViewModel
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student.FullName,
                    StudentCode = e.Student.StudentCode,
                    ExistingGrade = grades.FirstOrDefault(g => g.StudentId == e.StudentId)?.Score,
                    MaxScore = 100,
                    Notes = grades.FirstOrDefault(g => g.StudentId == e.StudentId)?.Notes
                }).ToList()
            };

            return View(viewModel);
        }

        // Xử lý việc lưu điểm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrades(GradeStudentsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("GradeStudents", model);
            }

            var teacherId = await GetCurrentTeacherId();
            if (teacherId == 0)
            {
                return RedirectToAction("Index", "TeacherHome");
            }

            // Kiểm tra xem giáo viên có được phân công dạy khóa học này không
            var isAssigned = await _context.CourseAssignments
                .AnyAsync(ca => ca.TeacherId == teacherId && ca.CourseId == model.CourseId && ca.Status == "Active");

            if (!isAssigned)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền cập nhật điểm cho khóa học này.";
                return RedirectToAction(nameof(Index));
            }

            // Lưu điểm cho từng sinh viên
            foreach (var studentGrade in model.Students)
            {
                var existingRecord = await _context.AcademicRecords
                    .FirstOrDefaultAsync(ar => ar.StudentId == studentGrade.StudentId && 
                                             ar.CourseId == model.CourseId &&
                                             ar.TeacherId == teacherId);

                if (studentGrade.Grade.HasValue)
                {
                    if (existingRecord != null)
                    {
                        // Cập nhật điểm hiện có
                        existingRecord.Score = studentGrade.Grade.Value;
                        existingRecord.Notes = studentGrade.Notes;
                        existingRecord.UpdatedAt = DateTime.Now;
                        _context.Update(existingRecord);
                    }
                    else
                    {
                        // Tạo bản ghi mới
                        var record = new AcademicRecord
                        {
                            StudentId = studentGrade.StudentId,
                            CourseId = model.CourseId,
                            TeacherId = teacherId,
                            Score = studentGrade.Grade.Value,
                            MaxScore = studentGrade.MaxScore,
                            Notes = studentGrade.Notes,
                            AssignmentType = "Final",
                            GradedDate = DateTime.Now,
                            CreatedAt = DateTime.Now
                        };
                        _context.Add(record);
                    }
                }
                else if (existingRecord != null)
                {
                    // Xóa bản ghi nếu điểm là null
                    _context.AcademicRecords.Remove(existingRecord);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã lưu điểm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Lấy ID của giáo viên đang đăng nhập
        private async Task<int> GetCurrentTeacherId()
        {
            var accountId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (accountId == 0)
            {
                return 0;
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.AccountId == accountId);

            return teacher?.Id ?? 0;
        }
    }
}
