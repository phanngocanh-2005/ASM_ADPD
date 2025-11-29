using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthApp.Models.ViewModels
{
    public class GradeStudentsViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public List<StudentGradeViewModel> Students { get; set; } = new();
    }

    public class StudentGradeViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        
        [Range(0, 100, ErrorMessage = "Điểm phải từ 0 đến 100")]
        public decimal? Grade { get; set; }
        
        public decimal MaxScore { get; set; } = 100;
        public string? Notes { get; set; }
        
        // Thuộc tính này dùng để lưu điểm hiện tại (nếu có)
        public decimal? ExistingGrade { get; set; }
    }
}
