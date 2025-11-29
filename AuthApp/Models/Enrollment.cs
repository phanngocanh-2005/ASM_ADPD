using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApp.Models
{
    [Table("Enrollments")]
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public DateTime EnrollmentDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Enrolled";

        [StringLength(5)]
        public string? FinalGrade { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Student Student { get; set; } = null!;

        public Course Course { get; set; } = null!;

        public ICollection<AcademicRecord> AcademicRecords { get; set; } = new List<AcademicRecord>();
    }
}

