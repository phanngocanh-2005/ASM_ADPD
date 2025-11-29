using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApp.Models
{
    [Table("AcademicPrograms")]
    public class AcademicProgram
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string ProgramCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ProgramName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int? Duration { get; set; }

        public int? CreditsRequired { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public ICollection<Course> Courses { get; set; } = new List<Course>();

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}

