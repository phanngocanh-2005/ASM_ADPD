using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApp.Models
{
    [Table("tasks")]
    public class TaskJob
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public int CategoryId { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Description { get; set; } = string.Empty;
        [Required]
        public int AccountId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public Category? Category { get; set; }
        public Account? Account { get; set; }
    }
}
