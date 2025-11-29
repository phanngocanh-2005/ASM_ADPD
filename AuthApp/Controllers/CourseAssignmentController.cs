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

        // Hiển thị danh sách phân công
        public async Task<IActionResult> Index()
        {
            var assignments = await _context.CourseAssignments
                .Include(ca => ca.Teacher)
                .Include(ca => ca.Course)
                .ToListAsync();
                
            return View(assignments);
        }

        // Hiển thị form tạo mới phân công
        public async Task<IActionResult> Create()
        {
            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View();
        }

        // Xử lý tạo mới phân công
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeacherId,CourseId,Status")] CourseAssignment assignment)
        {
            if (ModelState.IsValid)
            {
                assignment.AssignmentDate = DateTime.Now;
                assignment.CreatedAt = DateTime.Now;
                
                _context.Add(assignment);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Đã phân công khóa học thành công!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View(assignment);
        }
    }
}
