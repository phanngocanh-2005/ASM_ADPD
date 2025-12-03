using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AuthApp.Models;

namespace AuthApp.Models.ViewModels
{
    public class GradeStudentsViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public List<StudentGradeViewModel> Students { get; set; } = new();
    }

    // Dùng cho trang Grade Management: mỗi hàng là 1 sinh viên trong 1 môn học
    public class InlineGradeRowViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
        public decimal? Grade { get; set; }
    }

    // Dùng cho trang Grade Management: mỗi hàng là 1 course,
    // teacher chọn student trong dropdown và nhập/sửa điểm.
    public class CourseInlineGradeViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        public List<StudentInlineOptionViewModel> Students { get; set; } = new();
    }

    public class StudentInlineOptionViewModel
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public decimal? Grade { get; set; }
    }

    public class StudentGradeViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        
        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
        public decimal? Grade { get; set; }
        
        public decimal MaxScore { get; set; } = 100;
        public string? Notes { get; set; }
        
        // Thuộc tính này dùng để lưu điểm hiện tại (nếu có)
        public decimal? ExistingGrade { get; set; }
        
        // Loại assignment (Midterm, Final, Assignment, Quiz, etc.)
        public string? AssignmentType { get; set; }
    }

    public class StudentGradesViewModel
    {
        public Student Student { get; set; } = null!;
        public List<CourseGradesViewModel> GradesByCourse { get; set; } = new();
    }

    public class CourseGradesViewModel
    {
        public Course Course { get; set; } = null!;
        public List<AcademicRecord> Records { get; set; } = new();
        public decimal AverageScore { get; set; }
        public int TotalRecords { get; set; }
    }
}
