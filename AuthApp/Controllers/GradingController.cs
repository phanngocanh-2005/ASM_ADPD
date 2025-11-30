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

        // Display list of courses being taught
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
