using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApp.Models
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int AcademicProgramId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Credits { get; set; }

        public int? Semester { get; set; }

        [StringLength(10)]
        public string? AcademicYear { get; set; }

        public int? MaxStudents { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public AcademicProgram AcademicProgram { get; set; } = null!;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public ICollection<CourseAssignment> CourseAssignments { get; set; } = new List<CourseAssignment>();

        public ICollection<AcademicRecord> AcademicRecords { get; set; } = new List<AcademicRecord>();
    }
}

