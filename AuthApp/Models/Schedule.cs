using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApp.Models
{
    [Table("Schedules")]
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        // Optional: if set, this schedule is specific to a single student.
        // If null, the schedule applies to all enrolled students of the course.
        public int? StudentId { get; set; }

        [Required]
        [StringLength(20)]
        public string DayOfWeek { get; set; } = string.Empty; // Monday, Tuesday, etc.

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [StringLength(50)]
        public string? Room { get; set; }

        [StringLength(100)]
        public string? Building { get; set; }

        [StringLength(20)]
        public string? ClassType { get; set; } // Lecture, Lab, Tutorial, etc.

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Course? Course { get; set; }
        public Teacher? Teacher { get; set; }
        public Student? Student { get; set; }
    }
}

