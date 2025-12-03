using System;
using System.Linq;
using System.Threading.Tasks;
using AuthApp.Data;
using AuthApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthApp.Models.ViewModels;

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

        // Trang Grade Management: hiển thị Course cố định,
        // teacher chọn Student và nhập/sửa Grade trực tiếp
        public async Task<IActionResult> Index()
        {
            var teacherId = await GetCurrentTeacherId();
            if (teacherId == 0)
            {
                return RedirectToAction("Index", "TeacherHome");
            }

            // Lấy các course mà giáo viên đang dạy
            var assignments = await _context.CourseAssignments
                .Where(ca => ca.TeacherId == teacherId && ca.Status == "Active")
                .Include(ca => ca.Course)
                .ToListAsync();

            // Lấy điểm hiện có (Final) cho các course này do giáo viên hiện tại chấm
            const string assignmentType = "Final";
            var courseIds = assignments
                .Where(a => a.Course != null)
                .Select(a => a.CourseId)
                .Distinct()
                .ToList();

            var existingGrades = await _context.AcademicRecords
                .Where(ar => courseIds.Contains(ar.CourseId)
                             && ar.GradedBy == teacherId
                             && ar.AssignmentType == assignmentType)
                .ToListAsync();

            // Lấy toàn bộ sinh viên Active để teacher có thể chọn từ dropdown
            var allStudents = await _context.Students
                .Where(s => s.Status == "Active")
                .OrderBy(s => s.StudentCode)
                .ToListAsync();

            var viewModel = assignments
                .Where(a => a.Course != null)
                .Select(a =>
                {
                    var course = a.Course!;
                    var students = allStudents
                        .Select(s =>
                        {
                            var record = existingGrades.FirstOrDefault(r =>
                                r.CourseId == course.Id && r.StudentId == s.Id);

                            return new StudentInlineOptionViewModel
                            {
                                StudentId = s.Id,
                                StudentCode = s.StudentCode,
                                StudentName = s.FullName,
                                Grade = record?.Score
                            };
                        })
                        .OrderBy(s => s.StudentName)
                        .ToList();

                    return new CourseInlineGradeViewModel
                    {
                        CourseId = course.Id,
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        Students = students
                    };
                })
                .OrderBy(c => c.CourseCode)
                .ToList();

            return View(viewModel);
        }

        // Lưu điểm cho 1 dòng (course + student)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveInlineGrade(int courseId, int studentId, decimal? grade)
        {
            var teacherId = await GetCurrentTeacherId();
            if (teacherId == 0)
            {
                return RedirectToAction("Index", "TeacherHome");
            }

            // Kiểm tra giáo viên có được phân công dạy course này không
            var isAssigned = await _context.CourseAssignments
                .AnyAsync(ca => ca.TeacherId == teacherId && ca.CourseId == courseId && ca.Status == "Active");

            if (!isAssigned)
            {
                TempData["ErrorMessage"] = "You do not have permission to update grades for this course.";
                return RedirectToAction(nameof(Index));
            }

            const string assignmentType = "Final";

            // Tìm EnrollmentId nếu có (để đảm bảo student xem được điểm)
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId && e.Status == "Enrolled");

            var existingRecord = await _context.AcademicRecords
                .FirstOrDefaultAsync(ar =>
                    ar.StudentId == studentId &&
                    ar.CourseId == courseId &&
                    ar.GradedBy == teacherId &&
                    ar.AssignmentType == assignmentType);

            if (grade.HasValue)
            {
                if (grade.Value < 0 || grade.Value > 100)
                {
                    TempData["ErrorMessage"] = "Grade must be between 0 and 100.";
                    return RedirectToAction(nameof(Index));
                }

                if (existingRecord != null)
                {
                    // Cập nhật điểm hiện có
                    existingRecord.Score = grade.Value;
                    existingRecord.MaxScore = 100;
                    existingRecord.GradedDate = DateTime.Now;
                    existingRecord.UpdatedAt = DateTime.Now;
                    // Cập nhật EnrollmentId nếu có
                    if (enrollment != null && existingRecord.EnrollmentId == null)
                    {
                        existingRecord.EnrollmentId = enrollment.Id;
                    }
                    _context.Update(existingRecord);
                }
                else
                {
                    // Tạo mới AcademicRecord
                    var record = new AcademicRecord
                    {
                        StudentId = studentId,
                        CourseId = courseId,
                        EnrollmentId = enrollment?.Id, // Thêm EnrollmentId nếu có
                        GradedBy = teacherId,
                        Score = grade.Value,
                        MaxScore = 100,
                        AssignmentType = assignmentType,
                        GradedDate = DateTime.Now,
                        CreatedAt = DateTime.Now
                    };
                    _context.Add(record);
                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Grade {grade.Value:F2} has been saved successfully. Student can now view this grade.";
                    // Lưu thông tin để giữ lại selection sau khi reload
                    TempData["LastSavedCourseId"] = courseId;
                    TempData["LastSavedStudentId"] = studentId;
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = $"Error saving grade: {ex.InnerException?.Message ?? ex.Message}";
                }
            }
            else if (existingRecord != null)
            {
                // Nếu để trống thì xóa điểm
                _context.AcademicRecords.Remove(existingRecord);
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Grade has been deleted successfully.";
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = $"Error deleting grade: {ex.InnerException?.Message ?? ex.Message}";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Xóa điểm cho 1 dòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInlineGrade(int courseId, int studentId)
        {
            var teacherId = await GetCurrentTeacherId();
            if (teacherId == 0)
            {
                return RedirectToAction("Index", "TeacherHome");
            }

            var isAssigned = await _context.CourseAssignments
                .AnyAsync(ca => ca.TeacherId == teacherId && ca.CourseId == courseId && ca.Status == "Active");

            if (!isAssigned)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete grades for this course.";
                return RedirectToAction(nameof(Index));
            }

            const string assignmentType = "Final";

            var record = await _context.AcademicRecords
                .FirstOrDefaultAsync(ar =>
                    ar.StudentId == studentId &&
                    ar.CourseId == courseId &&
                    ar.GradedBy == teacherId &&
                    ar.AssignmentType == assignmentType);

            if (record != null)
            {
                _context.AcademicRecords.Remove(record);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Grade has been deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Display list of students in a course for grading
        public async Task<IActionResult> GradeStudents(int courseId, string? assignmentType = null)
        {
            var teacherId = await GetCurrentTeacherId();
            if (teacherId == 0)
            {
                return RedirectToAction("Index", "TeacherHome");
            }

            // Check if teacher is assigned to teach this course
            var isAssigned = await _context.CourseAssignments
                .AnyAsync(ca => ca.TeacherId == teacherId && ca.CourseId == courseId && ca.Status == "Active");

            if (!isAssigned)
            {
                TempData["ErrorMessage"] = "You do not have permission to access this course.";
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

            // Get all existing grades (all assignment types)
            var grades = await _context.AcademicRecords
                .Where(ar => ar.CourseId == courseId && ar.GradedBy == teacherId)
                .ToListAsync();

            // Get all assignment types for this course
            var assignmentTypes = grades.Select(g => g.AssignmentType).Distinct().ToList();
            // Add default assignment types if none exist
            var defaultTypes = new List<string> { "Midterm", "Final", "Assignment", "Quiz", "Project", "Lab", "Presentation" };
            foreach (var type in defaultTypes)
            {
                if (!assignmentTypes.Contains(type))
                {
                    assignmentTypes.Add(type);
                }
            }
            assignmentTypes = assignmentTypes.OrderBy(t => t).ToList();

            ViewBag.AssignmentTypes = assignmentTypes;
            ViewBag.SelectedAssignmentType = assignmentType ?? Request.Query["assignmentType"].ToString() ?? assignmentTypes.FirstOrDefault() ?? "Final";

            var viewModel = new GradeStudentsViewModel
            {
                CourseId = courseId,
                CourseName = course.CourseName,
                Students = course.Enrollments.Select(e => 
                {
                    var selectedType = ViewBag.SelectedAssignmentType as string;
                    var existingRecord = grades.FirstOrDefault(g => 
                        g.StudentId == e.StudentId && 
                        g.AssignmentType == selectedType);
                    
                    return new StudentGradeViewModel
                    {
                        StudentId = e.StudentId,
                        StudentName = e.Student.FullName,
                        StudentCode = e.Student.StudentCode,
                        Grade = existingRecord?.Score,
                        ExistingGrade = existingRecord?.Score,
                        MaxScore = existingRecord?.MaxScore ?? 100,
                        Notes = existingRecord?.Notes,
                        AssignmentType = selectedType
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // Handle saving grades
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrades(GradeStudentsViewModel model)
        {
            var teacherId = await GetCurrentTeacherId();
            if (teacherId == 0)
            {
                return RedirectToAction("Index", "TeacherHome");
            }

            // Get assignment type from form
            var assignmentType = Request.Form["AssignmentType"].ToString();
            if (string.IsNullOrEmpty(assignmentType))
            {
                assignmentType = "Final";
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(GradeStudents), new { courseId = model.CourseId, assignmentType = assignmentType });
            }

            // Check if teacher is assigned to teach this course
            var isAssigned = await _context.CourseAssignments
                .AnyAsync(ca => ca.TeacherId == teacherId && ca.CourseId == model.CourseId && ca.Status == "Active");

            if (!isAssigned)
            {
                TempData["ErrorMessage"] = "You do not have permission to update grades for this course.";
                return RedirectToAction(nameof(Index));
            }

            // Save grades for each student
            foreach (var studentGrade in model.Students)
            {
                var existingRecord = await _context.AcademicRecords
                    .FirstOrDefaultAsync(ar => ar.StudentId == studentGrade.StudentId && 
                                             ar.CourseId == model.CourseId &&
                                             ar.GradedBy == teacherId &&
                                             ar.AssignmentType == assignmentType);

                if (studentGrade.Grade.HasValue)
                {
                    if (existingRecord != null)
                    {
                        // Update existing grade
                        existingRecord.Score = studentGrade.Grade.Value;
                        existingRecord.MaxScore = studentGrade.MaxScore;
                        existingRecord.Notes = studentGrade.Notes;
                        existingRecord.GradedDate = DateTime.Now;
                        existingRecord.UpdatedAt = DateTime.Now;
                        _context.Update(existingRecord);
                    }
                    else
                    {
                        // Create new record
                        var record = new AcademicRecord
                        {
                            StudentId = studentGrade.StudentId,
                            CourseId = model.CourseId,
                            GradedBy = teacherId,
                            Score = studentGrade.Grade.Value,
                            MaxScore = studentGrade.MaxScore,
                            Notes = studentGrade.Notes,
                            AssignmentType = assignmentType,
                            GradedDate = DateTime.Now,
                            CreatedAt = DateTime.Now
                        };
                        _context.Add(record);
                    }
                }
                else if (existingRecord != null)
                {
                    // Delete record if grade is null
                    _context.AcademicRecords.Remove(existingRecord);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Grades for {assignmentType} have been saved successfully!";
            return RedirectToAction(nameof(GradeStudents), new { courseId = model.CourseId, assignmentType = assignmentType });
        }

        // Delete a specific grade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGrade(int studentId, string assignmentType, int courseId)
        {
            var teacherId = await GetCurrentTeacherId();
            if (teacherId == 0)
            {
                TempData["ErrorMessage"] = "Unable to identify teacher. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            // Check if teacher is assigned to teach this course
            var isAssigned = await _context.CourseAssignments
                .AnyAsync(ca => ca.TeacherId == teacherId && ca.CourseId == courseId && ca.Status == "Active");

            if (!isAssigned)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete grades for this course.";
                return RedirectToAction(nameof(Index));
            }

            // Find and delete the grade record
            var record = await _context.AcademicRecords
                .FirstOrDefaultAsync(ar => ar.StudentId == studentId && 
                                         ar.CourseId == courseId &&
                                         ar.GradedBy == teacherId &&
                                         ar.AssignmentType == assignmentType);

            if (record != null)
            {
                _context.AcademicRecords.Remove(record);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Grade for {assignmentType} has been deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Grade record not found.";
            }

            return RedirectToAction(nameof(GradeStudents), new { courseId = courseId, assignmentType = assignmentType });
        }

        // Get ID of the currently logged-in teacher
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
