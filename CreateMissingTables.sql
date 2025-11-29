-- ============================================
-- Create Missing Tables Script
-- This script checks and creates missing tables
-- Run this if you get "Invalid object name" errors
-- ============================================

-- ============================================
-- 1. Check and Create Courses table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
BEGIN
    CREATE TABLE Courses (
        Id INT PRIMARY KEY IDENTITY(1,1),
        CourseCode NVARCHAR(20) NOT NULL UNIQUE,
        CourseName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        AcademicProgramId INT NOT NULL,
        Credits INT NOT NULL CHECK (Credits > 0),
        Semester INT NULL CHECK (Semester > 0),
        AcademicYear NVARCHAR(10) NULL,
        MaxStudents INT NULL CHECK (MaxStudents > 0),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Inactive', 'Completed')),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CONSTRAINT FK_Courses_AcademicProgram FOREIGN KEY (AcademicProgramId) REFERENCES AcademicPrograms(Id)
    );
    PRINT 'Created Courses table';
END
ELSE
BEGIN
    PRINT 'Courses table already exists';
END
GO

-- ============================================
-- 2. Check and Create Enrollments table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
BEGIN
    CREATE TABLE Enrollments (
        Id INT PRIMARY KEY IDENTITY(1,1),
        StudentId INT NOT NULL,
        CourseId INT NOT NULL,
        EnrollmentDate DATETIME NOT NULL DEFAULT GETDATE(),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Enrolled' CHECK (Status IN ('Enrolled', 'Completed', 'Dropped', 'Failed')),
        FinalGrade NVARCHAR(5) NULL CHECK (FinalGrade IN ('A', 'B', 'C', 'D', 'F', 'A+', 'A-', 'B+', 'B-', 'C+', 'C-', 'D+', 'D-')),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CONSTRAINT FK_Enrollments_Student FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Enrollments_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_Enrollment_Student_Course UNIQUE (StudentId, CourseId, Status)
    );
    PRINT 'Created Enrollments table';
END
ELSE
BEGIN
    PRINT 'Enrollments table already exists';
END
GO

-- ============================================
-- 3. Check and Create CourseAssignments table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseAssignments')
BEGIN
    CREATE TABLE CourseAssignments (
        Id INT PRIMARY KEY IDENTITY(1,1),
        TeacherId INT NOT NULL,
        CourseId INT NOT NULL,
        AssignmentDate DATETIME NOT NULL DEFAULT GETDATE(),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Completed', 'Cancelled')),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_CourseAssignments_Teacher FOREIGN KEY (TeacherId) REFERENCES Teachers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CourseAssignments_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE
    );
    PRINT 'Created CourseAssignments table';
END
ELSE
BEGIN
    PRINT 'CourseAssignments table already exists';
END
GO

-- ============================================
-- 4. Check and Create AcademicRecords table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
BEGIN
    CREATE TABLE AcademicRecords (
        Id INT PRIMARY KEY IDENTITY(1,1),
        StudentId INT NOT NULL,
        CourseId INT NOT NULL,
        EnrollmentId INT NULL,
        AssignmentType NVARCHAR(50) NOT NULL,
        Score DECIMAL(5,2) NOT NULL CHECK (Score >= 0),
        MaxScore DECIMAL(5,2) NOT NULL DEFAULT 100.00 CHECK (MaxScore > 0),
        Weight DECIMAL(5,2) NULL CHECK (Weight >= 0 AND Weight <= 100),
        GradedDate DATETIME NULL,
        GradedBy INT NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CONSTRAINT FK_AcademicRecords_Student FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AcademicRecords_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AcademicRecords_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id) ON DELETE SET NULL,
        CONSTRAINT FK_AcademicRecords_Teacher FOREIGN KEY (GradedBy) REFERENCES Teachers(Id) ON DELETE SET NULL
    );
    PRINT 'Created AcademicRecords table';
END
ELSE
BEGIN
    PRINT 'AcademicRecords table already exists';
END
GO

-- ============================================
-- Create Indexes for AcademicRecords
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_StudentId' AND object_id = OBJECT_ID(N'[dbo].[AcademicRecords]'))
BEGIN
    CREATE INDEX IX_AcademicRecords_StudentId ON AcademicRecords(StudentId);
    PRINT 'Created index IX_AcademicRecords_StudentId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_CourseId' AND object_id = OBJECT_ID(N'[dbo].[AcademicRecords]'))
BEGIN
    CREATE INDEX IX_AcademicRecords_CourseId ON AcademicRecords(CourseId);
    PRINT 'Created index IX_AcademicRecords_CourseId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_EnrollmentId' AND object_id = OBJECT_ID(N'[dbo].[AcademicRecords]'))
BEGIN
    CREATE INDEX IX_AcademicRecords_EnrollmentId ON AcademicRecords(EnrollmentId);
    PRINT 'Created index IX_AcademicRecords_EnrollmentId';
END
GO

-- ============================================
-- Create Indexes for Courses
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Courses_CourseCode' AND object_id = OBJECT_ID(N'[dbo].[Courses]'))
BEGIN
    CREATE INDEX IX_Courses_CourseCode ON Courses(CourseCode);
    PRINT 'Created index IX_Courses_CourseCode';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Courses_AcademicProgramId' AND object_id = OBJECT_ID(N'[dbo].[Courses]'))
BEGIN
    CREATE INDEX IX_Courses_AcademicProgramId ON Courses(AcademicProgramId);
    PRINT 'Created index IX_Courses_AcademicProgramId';
END
GO

-- ============================================
-- Create Indexes for Enrollments
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_StudentId' AND object_id = OBJECT_ID(N'[dbo].[Enrollments]'))
BEGIN
    CREATE INDEX IX_Enrollments_StudentId ON Enrollments(StudentId);
    PRINT 'Created index IX_Enrollments_StudentId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_CourseId' AND object_id = OBJECT_ID(N'[dbo].[Enrollments]'))
BEGIN
    CREATE INDEX IX_Enrollments_CourseId ON Enrollments(CourseId);
    PRINT 'Created index IX_Enrollments_CourseId';
END
GO

-- ============================================
-- Create Indexes for CourseAssignments
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CourseAssignments_TeacherId' AND object_id = OBJECT_ID(N'[dbo].[CourseAssignments]'))
BEGIN
    CREATE INDEX IX_CourseAssignments_TeacherId ON CourseAssignments(TeacherId);
    PRINT 'Created index IX_CourseAssignments_TeacherId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CourseAssignments_CourseId' AND object_id = OBJECT_ID(N'[dbo].[CourseAssignments]'))
BEGIN
    CREATE INDEX IX_CourseAssignments_CourseId ON CourseAssignments(CourseId);
    PRINT 'Created index IX_CourseAssignments_CourseId';
END
GO

PRINT 'Script completed successfully!';
PRINT 'All missing tables have been created.';
GO

