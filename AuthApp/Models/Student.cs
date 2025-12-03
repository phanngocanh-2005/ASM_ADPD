using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApp.Models
{
    [Table("Students")]
    public class Student
    {
        [Key]
        public int Id { get; set; }

        public int? AccountId { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public int? AcademicProgramId { get; set; }

        [Required]
        public DateTime EnrollmentDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
        public decimal? GPA { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Account? Account { get; set; }

        public AcademicProgram? AcademicProgram { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public ICollection<AcademicRecord> AcademicRecords { get; set; } = new List<AcademicRecord>();

        // Schedules that are assigned specifically to this student (optional).
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}

