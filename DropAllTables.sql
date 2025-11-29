-- ============================================
-- DROP ALL TABLES
-- Chạy script này để xóa tất cả các bảng
-- Sau đó chạy CompleteDatabaseSchema.sql để tạo lại
-- ============================================

-- Drop bảng con trước (có foreign key)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
    DROP TABLE AcademicRecords;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
    DROP TABLE Enrollments;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseAssignments')
    DROP TABLE CourseAssignments;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
    DROP TABLE Courses;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'tasks')
    DROP TABLE tasks;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'categories')
    DROP TABLE categories;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
    DROP TABLE Students;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Teachers')
    DROP TABLE Teachers;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicPrograms')
    DROP TABLE AcademicPrograms;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Account')
    DROP TABLE Account;

PRINT 'All tables dropped successfully!';
GO
