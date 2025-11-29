using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApp.Models
{
    [Table("Teachers")]
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        public int? AccountId { get; set; }

        [Required]
        [StringLength(20)]
        public string TeacherCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(255)]
        public string? Specialization { get; set; }

        public int? AcademicProgramId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Account? Account { get; set; }

        public AcademicProgram? AcademicProgram { get; set; }

        public ICollection<CourseAssignment> CourseAssignments { get; set; } = new List<CourseAssignment>();

        public ICollection<AcademicRecord> AcademicRecords { get; set; } = new List<AcademicRecord>();
    }
}

