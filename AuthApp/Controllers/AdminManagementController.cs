using System;
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
    public class AdminManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // Academic programs section
        // =========================
        public async Task<IActionResult> Programs()
        {
            var programs = await _context.AcademicPrograms
                .OrderBy(p => p.ProgramName)
                .ToListAsync();

            return View(programs);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedDefaultCourses()
        {
            try
            {
                // Get or create a default academic program
                var program = await _context.AcademicPrograms
                    .Where(p => p.Status == "Active")
                    .FirstOrDefaultAsync();

                if (program == null)
                {
                    // Create default program
                    program = new AcademicProgram
                    {
                        ProgramCode = "COMP",
                        ProgramName = "Computing Program",
                        Description = "Default Computing Program",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AcademicPrograms.Add(program);
                    await _context.SaveChangesAsync();
                }

                var courses = new List<Course>
                {
                    new Course { CourseCode = "PRO001", CourseName = "Professional Practice", Description = "Professional Practice course covering industry standards and professional ethics", AcademicProgramId = program.Id, Credits = 3, Status = "Active", CreatedAt = DateTime.UtcNow },
                    new Course { CourseCode = "PCP001", CourseName = "Planning a Computing Project", Description = "Course on project planning methodologies and tools for computing projects", AcademicProgramId = program.Id, Credits = 3, Status = "Active", CreatedAt = DateTime.UtcNow },
                    new Course { CourseCode = "DDD001", CourseName = "Database Design & Development", Description = "Comprehensive course on database design principles and development practices", AcademicProgramId = program.Id, Credits = 4, Status = "Active", CreatedAt = DateTime.UtcNow },
                    new Course { CourseCode = "DSA001", CourseName = "Data Structures & Algorithms", Description = "Study of fundamental data structures and algorithm design and analysis", AcademicProgramId = program.Id, Credits = 4, Status = "Active", CreatedAt = DateTime.UtcNow },
                    new Course { CourseCode = "IOT001", CourseName = "Internet of Things", Description = "Introduction to IoT concepts, devices, and applications", AcademicProgramId = program.Id, Credits = 3, Status = "Active", CreatedAt = DateTime.UtcNow },
                    new Course { CourseCode = "SEC001", CourseName = "Security", Description = "Cybersecurity fundamentals, threats, and protection mechanisms", AcademicProgramId = program.Id, Credits = 3, Status = "Active", CreatedAt = DateTime.UtcNow },
                    new Course { CourseCode = "SDLC001", CourseName = "Software Development Life Cycle", Description = "Comprehensive study of SDLC methodologies and best practices", AcademicProgramId = program.Id, Credits = 3, Status = "Active", CreatedAt = DateTime.UtcNow },
                    new Course { CourseCode = "WDD001", CourseName = "Website Design & Development", Description = "Web development technologies, design principles, and modern frameworks", AcademicProgramId = program.Id, Credits = 4, Status = "Active", CreatedAt = DateTime.UtcNow },
                    new Course { CourseCode = "BPS001", CourseName = "Business Process Support", Description = "Understanding and supporting business processes through technology", AcademicProgramId = program.Id, Credits = 3, Status = "Active", CreatedAt = DateTime.UtcNow },
                    new Course { CourseCode = "APD001", CourseName = "Applied Programming and Design Principles", Description = "Practical programming skills and software design principles", AcademicProgramId = program.Id, Credits = 4, Status = "Active", CreatedAt = DateTime.UtcNow },
                    new Course { CourseCode = "APP001", CourseName = "Application Development", Description = "Development of desktop and mobile applications using modern technologies", AcademicProgramId = program.Id, Credits = 4, Status = "Active", CreatedAt = DateTime.UtcNow },
                    new Course { CourseCode = "DM001", CourseName = "Discrete Maths", Description = "Mathematical foundations for computer science including logic, sets, and graph theory", AcademicProgramId = program.Id, Credits = 3, Status = "Active", CreatedAt = DateTime.UtcNow }
                };

                int addedCount = 0;
                foreach (var course in courses)
                {
                    var exists = await _context.Courses.AnyAsync(c => c.CourseCode == course.CourseCode);
                    if (!exists)
                    {
                        _context.Courses.Add(course);
                        addedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully added {addedCount} new courses. {courses.Count - addedCount} courses already existed.";
                return RedirectToAction("Programs");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Programs");
            }
        }

        [HttpGet]
        public IActionResult CreateProgram()
        {
            PopulateProgramStatuses();
            return View(new AcademicProgram
            {
                Status = "Active"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProgram(AcademicProgram model)
        {
            if (!ModelState.IsValid)
            {
                PopulateProgramStatuses();
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            _context.AcademicPrograms.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Program {model.ProgramName} was created successfully.";
            return RedirectToAction(nameof(Programs));
        }

        [HttpGet]
        public async Task<IActionResult> EditProgram(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var program = await _context.AcademicPrograms.FindAsync(id);
            if (program == null)
            {
                return NotFound();
            }

            PopulateProgramStatuses();
            return View(program);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProgram(int id, AcademicProgram model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                PopulateProgramStatuses();
                return View(model);
            }

            var existing = await _context.AcademicPrograms.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.ProgramCode = model.ProgramCode;
            existing.ProgramName = model.ProgramName;
            existing.Description = model.Description;
            existing.Duration = model.Duration;
            existing.CreditsRequired = model.CreditsRequired;
            existing.Status = model.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Update(existing);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Program {existing.ProgramName} was updated successfully.";
            return RedirectToAction(nameof(Programs));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProgram(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var program = await _context.AcademicPrograms
                .Include(p => p.Courses)
                .Include(p => p.Students)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        [HttpPost, ActionName("DeleteProgram")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProgramConfirmed(int id)
        {
            var program = await _context.AcademicPrograms.FindAsync(id);
            if (program == null)
            {
                return RedirectToAction(nameof(Programs));
            }

            if (await _context.Courses.AnyAsync(c => c.AcademicProgramId == id) ||
                await _context.Students.AnyAsync(s => s.AcademicProgramId == id))
            {
                TempData["ErrorMessage"] = "You cannot delete a program that still has related students or courses.";
                return RedirectToAction(nameof(Programs));
            }

            _context.AcademicPrograms.Remove(program);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Program {program.ProgramName} was deleted successfully.";
            return RedirectToAction(nameof(Programs));
        }

        private void PopulateProgramStatuses()
        {
            ViewBag.ProgramStatuses = new[]
            {
                new SelectListItem("Active", "Active"),
                new SelectListItem("Inactive", "Inactive")
            };
        }

        // =================
        // Student section
        // =================
        public async Task<IActionResult> Students()
        {
            var students = await _context.Students
                .Include(s => s.Account)
                .Include(s => s.AcademicProgram)
                .OrderBy(s => s.StudentCode)
                .ToListAsync();

            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> CreateStudent()
        {
            await PopulateStudentLookups();
            return View(new Student
            {
                EnrollmentDate = DateTime.UtcNow,
                Status = "Active",
                DateOfBirth = DateTime.UtcNow.AddYears(-18)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(Student model)
        {
            // Remove AccountId from validation since it's optional
            ModelState.Remove(nameof(Student.AccountId));
            
            // Remove AcademicProgramId from validation - make it optional
            ModelState.Remove(nameof(Student.AcademicProgramId));

            // Handle empty string from form - convert to null
            if (model.AcademicProgramId <= 0 || !model.AcademicProgramId.HasValue)
            {
                model.AcademicProgramId = null;
            }

            // Validate StudentCode is not empty
            if (string.IsNullOrWhiteSpace(model.StudentCode))
            {
                ModelState.AddModelError(nameof(Student.StudentCode), "Student Code is required.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateStudentLookups(model.AccountId);
                return View(model);
            }

            // Check for duplicate StudentCode
            var duplicateCode = await _context.Students.AnyAsync(s => s.StudentCode == model.StudentCode);
            if (duplicateCode)
            {
                ModelState.AddModelError(nameof(Student.StudentCode), "Student Code already exists. Please use a different code.");
                await PopulateStudentLookups(model.AccountId);
                return View(model);
            }

            if (model.AccountId.HasValue)
            {
                var accountExists = await _context.Accounts.AnyAsync(a => a.Id == model.AccountId && a.Role == "Student");
                if (!accountExists)
                {
                    ModelState.AddModelError(nameof(Student.AccountId), "The selected account is invalid or is not assigned the Student role.");
                    await PopulateStudentLookups(model.AccountId);
                    return View(model);
                }

                var assigned = await _context.Students.AnyAsync(s => s.AccountId == model.AccountId);
                if (assigned)
                {
                    ModelState.AddModelError(nameof(Student.AccountId), "This account is already linked to another student.");
                    await PopulateStudentLookups();
                    return View(model);
                }
            }

            try
            {
                // AcademicProgramId is already handled above (set to null if empty)
                model.CreatedAt = DateTime.UtcNow;
                _context.Students.Add(model);
                await _context.SaveChangesAsync();
                
                // Get the newly created student ID
                var newStudentId = model.Id;
                
                TempData["SuccessMessage"] = $"Student {model.FullName} was created successfully.";
                return RedirectToAction(nameof(StudentProfile), new { id = newStudentId });
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = "An error occurred while saving. Please check your data and try again.";
                if (ex.InnerException != null)
                {
                    var innerEx = ex.InnerException;
                    if (innerEx.Message.Contains("UNIQUE") || innerEx.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(nameof(Student.StudentCode), "Student Code already exists in the database.");
                    }
                    else if (innerEx.Message.Contains("FOREIGN KEY") || innerEx.Message.Contains("AcademicProgram"))
                    {
                        ModelState.AddModelError(nameof(Student.AcademicProgramId), "Invalid program selected. Please select a valid program or leave it empty.");
                    }
                    else
                    {
                        errorMessage = $"Database error: {innerEx.Message}";
                    }
                }
                ModelState.AddModelError("", errorMessage);
                await PopulateStudentLookups(model.AccountId);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                await PopulateStudentLookups(model.AccountId);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            await PopulateStudentLookups(student.AccountId);
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(int id, Student model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var studentToUpdate = await _context.Students
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (studentToUpdate == null)
            {
                return NotFound();
            }

            // Keep the original AccountId - don't allow changing it
            model.AccountId = studentToUpdate.AccountId;
            
            // Remove AccountId from validation since it's a hidden field
            ModelState.Remove(nameof(Student.AccountId));

            if (!ModelState.IsValid)
            {
                await PopulateStudentLookups(model.AccountId);
                return View(model);
            }

            studentToUpdate.StudentCode = model.StudentCode;
            studentToUpdate.FullName = model.FullName;
            studentToUpdate.DateOfBirth = model.DateOfBirth;
            studentToUpdate.Gender = model.Gender;
            studentToUpdate.PhoneNumber = model.PhoneNumber;
            studentToUpdate.Address = model.Address;
            studentToUpdate.AcademicProgramId = model.AcademicProgramId;
            studentToUpdate.EnrollmentDate = model.EnrollmentDate;
            studentToUpdate.Status = model.Status;
            studentToUpdate.GPA = model.GPA;
            studentToUpdate.UpdatedAt = DateTime.UtcNow;

            try
            {
                // Update linked account info (username & email) if this student has an account
                if (studentToUpdate.AccountId.HasValue)
                {
                    var account = await _context.Accounts.FindAsync(studentToUpdate.AccountId.Value);
                    if (account != null)
                    {
                        // Username (optional change)
                        if (Request.Form.ContainsKey("AccountUsername"))
                        {
                            var accountUsername = Request.Form["AccountUsername"].ToString();
                            if (!string.IsNullOrWhiteSpace(accountUsername) &&
                                !string.Equals(accountUsername, account.Username, StringComparison.Ordinal))
                            {
                                // Check username uniqueness
                                var usernameExists = await _context.Accounts
                                    .AnyAsync(a => a.Username == accountUsername && a.Id != account.Id);
                                if (usernameExists)
                                {
                                    ModelState.AddModelError("", "Username already exists. Please choose another one.");
                                    await PopulateStudentLookups(studentToUpdate.AccountId);
                                    return View(model);
                                }

                                account.Username = accountUsername;
                            }
                        }

                        // Email
                        if (Request.Form.ContainsKey("AccountEmail"))
                        {
                            var accountEmail = Request.Form["AccountEmail"].ToString();
                            if (!string.IsNullOrWhiteSpace(accountEmail))
                            {
                                account.Email = accountEmail;
                            }
                        }

                        _context.Update(account);
                    }
                }

                _context.Update(studentToUpdate);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Student {studentToUpdate.FullName} was updated successfully.";
                return RedirectToAction(nameof(StudentProfile), new { id = studentToUpdate.Id });
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving. Please check your data and try again.";
                await PopulateStudentLookups(studentToUpdate.AccountId);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
                await PopulateStudentLookups(studentToUpdate.AccountId);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteStudent(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Account)
                .Include(s => s.AcademicProgram)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return RedirectToAction(nameof(Students));
            }

            var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.StudentId == id);
            if (hasEnrollments)
            {
                TempData["ErrorMessage"] = "You cannot delete a student who already has enrollment history.";
                return RedirectToAction(nameof(Students));
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Student {student.FullName} was deleted successfully.";
            return RedirectToAction(nameof(Students));
        }

        [HttpGet]
        public async Task<IActionResult> StudentProfile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Account)
                .Include(s => s.AcademicProgram)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.AcademicRecords)
                    .ThenInclude(ar => ar.Course)
                .Include(s => s.AcademicRecords)
                    .ThenInclude(ar => ar.Teacher)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        private async Task PopulateStudentLookups(int? selectedAccountId = null)
        {
            var programs = await _context.AcademicPrograms
                .Where(p => p.Status == "Active")
                .OrderBy(p => p.ProgramName)
                .Select(p => new SelectListItem($"{p.ProgramCode} - {p.ProgramName}", p.Id.ToString()))
                .ToListAsync();

            ViewBag.ProgramOptions = programs;
            ViewBag.StudentStatuses = new[]
            {
                new SelectListItem("Active", "Active"),
                new SelectListItem("Graduated", "Graduated"),
                new SelectListItem("Suspended", "Suspended"),
                new SelectListItem("Withdrawn", "Withdrawn")
            };
            ViewBag.GenderOptions = new[]
            {
                new SelectListItem("Male", "Male"),
                new SelectListItem("Female", "Female"),
                new SelectListItem("Other", "Other")
            };
        }

        // =================
        // Teacher section
        // =================
        public async Task<IActionResult> Teachers()
        {
            var teachers = await _context.Teachers
                .Include(t => t.Account)
                .Include(t => t.AcademicProgram)
                .OrderBy(t => t.TeacherCode)
                .ToListAsync();

            return View(teachers);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTeacher()
        {
            await PopulateTeacherLookups();
            return View(new Teacher
            {
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(Teacher model)
        {
            // Remove AccountId and AcademicProgramId from validation since they're optional
            ModelState.Remove(nameof(Teacher.AccountId));
            ModelState.Remove(nameof(Teacher.AcademicProgramId));

            if (!ModelState.IsValid)
            {
                await PopulateTeacherLookups(model.AccountId);
                return View(model);
            }

            // Check for duplicate TeacherCode
            var duplicateCode = await _context.Teachers.AnyAsync(t => t.TeacherCode == model.TeacherCode);
            if (duplicateCode)
            {
                ModelState.AddModelError(nameof(Teacher.TeacherCode), "Teacher Code already exists. Please use a different code.");
                await PopulateTeacherLookups(model.AccountId);
                return View(model);
            }

            if (model.AccountId.HasValue && model.AccountId.Value > 0)
            {
                var accountIsTeacher = await _context.Accounts.AnyAsync(a => a.Id == model.AccountId.Value && a.Role == "Teacher");
                if (!accountIsTeacher)
                {
                    ModelState.AddModelError(nameof(Teacher.AccountId), "The selected account is invalid or not assigned the Teacher role.");
                    await PopulateTeacherLookups(model.AccountId);
                    return View(model);
                }

                var accountAlreadyLinked = await _context.Teachers.AnyAsync(t => t.AccountId == model.AccountId.Value);
                if (accountAlreadyLinked)
                {
                    ModelState.AddModelError(nameof(Teacher.AccountId), "This account is already linked to another teacher.");
                    await PopulateTeacherLookups(model.AccountId);
                    return View(model);
                }
            }

            try
            {
                // AccountId is now optional (nullable)
                // Set to null if not provided or <= 0
                if (!model.AccountId.HasValue || model.AccountId.Value <= 0)
                {
                    model.AccountId = null;
                }

                // AcademicProgramId is now optional (nullable)
                // Set to null if not selected
                if (model.AcademicProgramId <= 0 || !model.AcademicProgramId.HasValue)
                {
                    model.AcademicProgramId = null;
                }

                model.CreatedAt = DateTime.UtcNow;
                _context.Teachers.Add(model);
                await _context.SaveChangesAsync();
                
                // Get the newly created teacher ID
                var newTeacherId = model.Id;
                
                TempData["SuccessMessage"] = $"Teacher {model.FullName} was created successfully. " +
                    $"Next steps: 1) Assign courses to this teacher via Course Assignment, " +
                    $"2) Create schedules for the assigned courses. " +
                    $"The teacher will automatically see their schedule and be able to manage grades once courses are assigned.";
                return RedirectToAction(nameof(TeacherProfile), new { id = newTeacherId });
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = "An error occurred while saving. Please check your data and try again.";
                if (ex.InnerException != null)
                {
                    var innerEx = ex.InnerException;
                    if (innerEx.Message.Contains("UNIQUE") || innerEx.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(nameof(Teacher.TeacherCode), "Teacher Code already exists in the database.");
                    }
                    else if (innerEx.Message.Contains("FOREIGN KEY") || innerEx.Message.Contains("Account"))
                    {
                        ModelState.AddModelError(nameof(Teacher.AccountId), "Invalid account selected. Please select a valid account or leave it empty.");
                    }
                    else
                    {
                        errorMessage = $"Database error: {innerEx.Message}";
                    }
                }
                ModelState.AddModelError("", errorMessage);
                await PopulateTeacherLookups(model.AccountId);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                await PopulateTeacherLookups(model.AccountId);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditTeacher(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            await PopulateTeacherLookups(teacher.AccountId);
            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(int id, Teacher model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var teacherToUpdate = await _context.Teachers.FindAsync(id);
            if (teacherToUpdate == null)
            {
                return NotFound();
            }

            // Keep the original AccountId - don't allow changing it
            model.AccountId = teacherToUpdate.AccountId;
            
            // Remove AccountId and AcademicProgramId from validation since they're optional
            ModelState.Remove(nameof(Teacher.AccountId));
            ModelState.Remove(nameof(Teacher.AcademicProgramId));

            if (!ModelState.IsValid)
            {
                await PopulateTeacherLookups(teacherToUpdate.AccountId);
                return View(model);
            }

            teacherToUpdate.TeacherCode = model.TeacherCode;
            teacherToUpdate.FullName = model.FullName;
            teacherToUpdate.DateOfBirth = model.DateOfBirth;
            teacherToUpdate.Gender = model.Gender;
            teacherToUpdate.PhoneNumber = model.PhoneNumber;
            teacherToUpdate.Email = model.Email;
            teacherToUpdate.Department = model.Department;
            teacherToUpdate.Specialization = model.Specialization;
            teacherToUpdate.AcademicProgramId = model.AcademicProgramId;
            teacherToUpdate.Status = model.Status;
            teacherToUpdate.UpdatedAt = DateTime.UtcNow;

            try
            {
                // Update account email if provided
                if (teacherToUpdate.AccountId > 0)
                {
                    if (Request.Form.ContainsKey("AccountEmail"))
                    {
                        var accountEmail = Request.Form["AccountEmail"].ToString();
                        if (!string.IsNullOrWhiteSpace(accountEmail))
                        {
                            var account = await _context.Accounts.FindAsync(teacherToUpdate.AccountId);
                            if (account != null)
                            {
                                account.Email = accountEmail;
                                _context.Update(account);
                            }
                        }
                    }
                }

                _context.Update(teacherToUpdate);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Teacher {teacherToUpdate.FullName} was updated successfully.";
                return RedirectToAction(nameof(TeacherProfile), new { id = teacherToUpdate.Id });
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving. Please check your data and try again.";
                await PopulateTeacherLookups(teacherToUpdate.AccountId);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
                await PopulateTeacherLookups(teacherToUpdate.AccountId);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteTeacher(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        [HttpPost, ActionName("DeleteTeacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacherConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return RedirectToAction(nameof(Teachers));
            }

            var hasAssignments = await _context.CourseAssignments.AnyAsync(ca => ca.TeacherId == id);
            var hasGrades = await _context.AcademicRecords.AnyAsync(ar => ar.GradedBy == id);
            if (hasAssignments || hasGrades)
            {
                TempData["ErrorMessage"] = "You cannot delete a teacher that is still assigned to courses or grading records.";
                return RedirectToAction(nameof(Teachers));
            }

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Teacher {teacher.FullName} was deleted successfully.";
            return RedirectToAction(nameof(Teachers));
        }

        [HttpGet]
        public async Task<IActionResult> TeacherProfile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .Include(t => t.Account)
                .Include(t => t.AcademicProgram)
                .Include(t => t.CourseAssignments)
                    .ThenInclude(ca => ca.Course)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            // Load academic records where this teacher is the grader
            teacher.AcademicRecords = await _context.AcademicRecords
                .Where(ar => ar.GradedBy == id)
                .Include(ar => ar.Course)
                .Include(ar => ar.Student)
                .ToListAsync();

            // Add teacher ID to ViewData for use in the view if needed
            ViewData["TeacherId"] = id;

            return View(teacher);
        }

        private async Task PopulateTeacherLookups(int? selectedAccountId = null)
        {
            var availableAccounts = await _context.Accounts
                .Where(a => a.Role == "Teacher")
                .Where(a =>
                    (selectedAccountId.HasValue && a.Id == selectedAccountId) ||
                    !_context.Teachers.Any(t => t.AccountId == a.Id))
                .OrderBy(a => a.Username)
                .Select(a => new SelectListItem($"{a.Username} - {a.Fullname}", a.Id.ToString()))
                .ToListAsync();

            var programOptions = await _context.AcademicPrograms
                .Where(p => p.Status == "Active")
                .OrderBy(p => p.ProgramName)
                .Select(p => new SelectListItem($"{p.ProgramCode} - {p.ProgramName}", p.Id.ToString()))
                .ToListAsync();

            ViewBag.TeacherAccounts = availableAccounts;
            ViewBag.ProgramOptions = programOptions;
            ViewBag.TeacherStatuses = new[]
            {
                new SelectListItem("Active", "Active"),
                new SelectListItem("Inactive", "Inactive"),
                new SelectListItem("Retired", "Retired")
            };
            ViewBag.GenderOptions = new[]
            {
                new SelectListItem("Male", "Male"),
                new SelectListItem("Female", "Female"),
                new SelectListItem("Other", "Other")
            };
        }
    }
}

