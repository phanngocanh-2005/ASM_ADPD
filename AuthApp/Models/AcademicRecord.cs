using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApp.Models
{
    [Table("AcademicRecords")]
    public class AcademicRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int? EnrollmentId { get; set; }

        [Required]
        [StringLength(50)]
        public string AssignmentType { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Score { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal MaxScore { get; set; } = 100m;

        [Range(0, 100)]
        public decimal? Weight { get; set; }

        public DateTime? GradedDate { get; set; }

        public int? GradedBy { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties with ForeignKey attributes
        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;

        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;

        [ForeignKey("EnrollmentId")]
        public Enrollment? Enrollment { get; set; }

        [ForeignKey("GradedBy")]
        public Teacher? Teacher { get; set; }
    }
}

