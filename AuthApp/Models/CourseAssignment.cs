using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApp.Models
{
    [Table("CourseAssignments")]
    public class CourseAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public DateTime AssignmentDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; }

        public Teacher Teacher { get; set; } = null!;

        public Course Course { get; set; } = null!;
    }
}

