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
    [Authorize(Roles = "Admin")]
    public class CourseAssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseAssignmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display list of course assignments
        public async Task<IActionResult> Index()
        {
            var assignments = await _context.CourseAssignments
                .Include(ca => ca.Teacher)
                .Include(ca => ca.Course)
                .OrderByDescending(ca => ca.AssignmentDate)
                .ToListAsync();
                
            return View(assignments);
        }

        // Display form to create new assignment
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

        // Handle creating new assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeacherId,CourseId,Status")] CourseAssignment assignment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    assignment.AssignmentDate = DateTime.Now;
                    assignment.CreatedAt = DateTime.Now;

                    _context.Add(assignment);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Course assigned successfully! " +
                        "The teacher can now view this course in Grade Management. " +
                        "Remember to create a schedule for this course so the teacher can see it in their weekly schedule.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    var message = ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError(string.Empty, $"Could not save course assignment: {message}");
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
