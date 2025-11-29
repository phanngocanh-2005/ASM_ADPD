using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthApp.Models.ViewModels
{
    public class TeacherDashboardViewModel
    {
        public Teacher Teacher { get; set; } = null!;
        public int TotalAssignments { get; set; }
        public int ActiveAssignments { get; set; }
        public int TotalGradedRecords { get; set; }
        public int UniqueStudents { get; set; }
        public IReadOnlyList<CourseAssignment> RecentAssignments { get; set; } = Array.Empty<CourseAssignment>();
        public IReadOnlyList<AcademicRecord> RecentGrades { get; set; } = Array.Empty<AcademicRecord>();
    }

    public class TeacherProfileEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(255)]
        public string? Specialization { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? AccountEmail { get; set; }
    }
}

