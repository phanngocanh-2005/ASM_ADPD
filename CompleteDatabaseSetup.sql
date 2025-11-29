-- ============================================
-- COMPLETE DATABASE SETUP SCRIPT
-- Script hoàn chỉnh để tạo tất cả các bảng và cấu hình
-- Chạy script này để tạo database từ đầu
-- ============================================

-- ============================================
-- 1. BẢNG ACCOUNT
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Account')
BEGIN
    CREATE TABLE Account (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Username VARCHAR(50) NOT NULL UNIQUE,
        Fullname VARCHAR(100) NOT NULL,
        Email VARCHAR(100) NOT NULL UNIQUE,
        PhoneNumber VARCHAR(20) NULL,
        Role VARCHAR(20) NOT NULL,
        PasswordHash VARCHAR(255) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
    );
    PRINT 'Created Account table';
END
ELSE
BEGIN
    PRINT 'Account table already exists';
END
GO

-- ============================================
-- 2. BẢNG ACADEMIC PROGRAMS
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicPrograms')
BEGIN
    CREATE TABLE AcademicPrograms (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ProgramCode NVARCHAR(20) NOT NULL UNIQUE,
        ProgramName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        Duration INT NULL,
        CreditsRequired INT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Inactive')),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL
    );
    PRINT 'Created AcademicPrograms table';
END
ELSE
BEGIN
    PRINT 'AcademicPrograms table already exists';
END
GO

-- ============================================
-- 3. BẢNG STUDENTS
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
    CREATE TABLE Students (
        Id INT PRIMARY KEY IDENTITY(1,1),
        AccountId INT NULL,
        StudentCode NVARCHAR(20) NOT NULL UNIQUE,
        FullName NVARCHAR(100) NOT NULL,
        DateOfBirth DATE NOT NULL,
        Gender NVARCHAR(10) NULL CHECK (Gender IN ('Male', 'Female', 'Other')),
        PhoneNumber NVARCHAR(20) NULL,
        Address NVARCHAR(255) NULL,
        AcademicProgramId INT NULL,
        EnrollmentDate DATE NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Graduated', 'Suspended', 'Withdrawn')),
        GPA DECIMAL(5,2) NULL CHECK (GPA >= 0 AND GPA <= 100.00),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CONSTRAINT FK_Students_Account FOREIGN KEY (AccountId) REFERENCES Account(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Students_AcademicProgram FOREIGN KEY (AcademicProgramId) REFERENCES AcademicPrograms(Id)
    );
    PRINT 'Created Students table';
END
ELSE
BEGIN
    PRINT 'Students table already exists';
    -- Add AcademicProgramId if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND name = 'AcademicProgramId')
    BEGIN
        ALTER TABLE Students ADD AcademicProgramId INT NULL;
        ALTER TABLE Students ADD CONSTRAINT FK_Students_AcademicProgram 
            FOREIGN KEY (AcademicProgramId) REFERENCES AcademicPrograms(Id);
        PRINT 'Added AcademicProgramId to Students table';
    END
END
GO

-- ============================================
-- 4. BẢNG TEACHERS
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Teachers')
BEGIN
    CREATE TABLE Teachers (
        Id INT PRIMARY KEY IDENTITY(1,1),
        AccountId INT NULL,
        TeacherCode NVARCHAR(20) NOT NULL UNIQUE,
        FullName NVARCHAR(100) NOT NULL,
        DateOfBirth DATE NULL,
        Gender NVARCHAR(10) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        Department NVARCHAR(100) NULL,
        Specialization NVARCHAR(255) NULL,
        AcademicProgramId INT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Inactive', 'Retired')),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CONSTRAINT FK_Teachers_Account FOREIGN KEY (AccountId) REFERENCES Account(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Teachers_AcademicProgram FOREIGN KEY (AcademicProgramId) REFERENCES AcademicPrograms(Id)
    );
    PRINT 'Created Teachers table';
END
ELSE
BEGIN
    PRINT 'Teachers table already exists';
    -- Add AcademicProgramId if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Teachers]') AND name = 'AcademicProgramId')
    BEGIN
        ALTER TABLE Teachers ADD AcademicProgramId INT NULL;
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Teachers_AcademicProgram')
        BEGIN
            ALTER TABLE Teachers ADD CONSTRAINT FK_Teachers_AcademicProgram 
                FOREIGN KEY (AcademicProgramId) REFERENCES AcademicPrograms(Id);
        END
        PRINT 'Added AcademicProgramId to Teachers table';
    END
END
GO

-- ============================================
-- 5. BẢNG COURSES
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
-- 6. BẢNG ENROLLMENTS (Student đăng ký Course)
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
-- 7. BẢNG COURSE ASSIGNMENTS (Teacher được phân công dạy Course)
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
-- 8. BẢNG ACADEMIC RECORDS (Điểm số chi tiết)
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
-- 9. BẢNG CATEGORIES (Cho Tasks)
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'categories')
BEGIN
    CREATE TABLE categories (
        id INT PRIMARY KEY IDENTITY(1,1),
        name NVARCHAR(255) NOT NULL
    );
    PRINT 'Created categories table';
END
ELSE
BEGIN
    PRINT 'categories table already exists';
END
GO

-- ============================================
-- 10. BẢNG TASKS
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tasks')
BEGIN
    CREATE TABLE tasks (
        id INT PRIMARY KEY IDENTITY(1,1),
        name NVARCHAR(255) NOT NULL,
        description NVARCHAR(500) NOT NULL,
        category_id INT NULL,
        account_id INT NOT NULL,
        status NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'InProgress', 'Completed')),
        due_date DATETIME NULL,
        created_at DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Tasks_Category FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE SET NULL,
        CONSTRAINT FK_Tasks_Account FOREIGN KEY (account_id) REFERENCES Account(Id) ON DELETE CASCADE
    );
    PRINT 'Created tasks table';
END
ELSE
BEGIN
    PRINT 'tasks table already exists';
END
GO

-- ============================================
-- CREATE INDEXES
-- ============================================
PRINT '';
PRINT 'Creating indexes...';

-- Account indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Account_Username' AND object_id = OBJECT_ID(N'[dbo].[Account]'))
    CREATE INDEX IX_Account_Username ON Account(Username);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Account_Email' AND object_id = OBJECT_ID(N'[dbo].[Account]'))
    CREATE INDEX IX_Account_Email ON Account(Email);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Account_Role' AND object_id = OBJECT_ID(N'[dbo].[Account]'))
    CREATE INDEX IX_Account_Role ON Account(Role);

-- Students indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_AccountId' AND object_id = OBJECT_ID(N'[dbo].[Students]'))
    CREATE UNIQUE NONCLUSTERED INDEX IX_Students_AccountId ON Students(AccountId) WHERE AccountId IS NOT NULL;
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_StudentCode' AND object_id = OBJECT_ID(N'[dbo].[Students]'))
    CREATE INDEX IX_Students_StudentCode ON Students(StudentCode);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_AcademicProgramId' AND object_id = OBJECT_ID(N'[dbo].[Students]'))
    CREATE INDEX IX_Students_AcademicProgramId ON Students(AcademicProgramId);

-- Teachers indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teachers_AccountId' AND object_id = OBJECT_ID(N'[dbo].[Teachers]'))
    CREATE UNIQUE NONCLUSTERED INDEX IX_Teachers_AccountId ON Teachers(AccountId) WHERE AccountId IS NOT NULL;
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teachers_TeacherCode' AND object_id = OBJECT_ID(N'[dbo].[Teachers]'))
    CREATE INDEX IX_Teachers_TeacherCode ON Teachers(TeacherCode);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teachers_AcademicProgramId' AND object_id = OBJECT_ID(N'[dbo].[Teachers]'))
    CREATE INDEX IX_Teachers_AcademicProgramId ON Teachers(AcademicProgramId);

-- Courses indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Courses_CourseCode' AND object_id = OBJECT_ID(N'[dbo].[Courses]'))
    CREATE INDEX IX_Courses_CourseCode ON Courses(CourseCode);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Courses_AcademicProgramId' AND object_id = OBJECT_ID(N'[dbo].[Courses]'))
    CREATE INDEX IX_Courses_AcademicProgramId ON Courses(AcademicProgramId);

-- Enrollments indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_StudentId' AND object_id = OBJECT_ID(N'[dbo].[Enrollments]'))
    CREATE INDEX IX_Enrollments_StudentId ON Enrollments(StudentId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_CourseId' AND object_id = OBJECT_ID(N'[dbo].[Enrollments]'))
    CREATE INDEX IX_Enrollments_CourseId ON Enrollments(CourseId);

-- CourseAssignments indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CourseAssignments_TeacherId' AND object_id = OBJECT_ID(N'[dbo].[CourseAssignments]'))
    CREATE INDEX IX_CourseAssignments_TeacherId ON CourseAssignments(TeacherId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CourseAssignments_CourseId' AND object_id = OBJECT_ID(N'[dbo].[CourseAssignments]'))
    CREATE INDEX IX_CourseAssignments_CourseId ON CourseAssignments(CourseId);

-- AcademicRecords indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_StudentId' AND object_id = OBJECT_ID(N'[dbo].[AcademicRecords]'))
    CREATE INDEX IX_AcademicRecords_StudentId ON AcademicRecords(StudentId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_CourseId' AND object_id = OBJECT_ID(N'[dbo].[AcademicRecords]'))
    CREATE INDEX IX_AcademicRecords_CourseId ON AcademicRecords(CourseId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_EnrollmentId' AND object_id = OBJECT_ID(N'[dbo].[AcademicRecords]'))
    CREATE INDEX IX_AcademicRecords_EnrollmentId ON AcademicRecords(EnrollmentId);

PRINT 'All indexes created successfully';
GO

-- ============================================
-- INSERT SAMPLE DATA (Optional)
-- ============================================
PRINT '';
PRINT 'Inserting sample data...';

-- Tạo tài khoản Admin mặc định (chỉ nếu chưa có)
IF NOT EXISTS (SELECT * FROM Account WHERE Username = 'admin')
BEGIN
    INSERT INTO Account (Username, Fullname, Email, Role, PasswordHash, IsActive, CreatedAt)
    VALUES ('admin', 'Administrator', 'admin@btec.edu.vn', 'Admin', 'admin123', 1, GETDATE());
    PRINT 'Created default admin account';
END
ELSE
BEGIN
    PRINT 'Admin account already exists';
END
GO

-- Tạo chương trình học mẫu (chỉ nếu chưa có)
IF NOT EXISTS (SELECT * FROM AcademicPrograms WHERE ProgramCode = 'IT001')
BEGIN
    INSERT INTO AcademicPrograms (ProgramCode, ProgramName, Description, Duration, CreditsRequired, Status)
    VALUES 
        ('IT001', 'Information Technology', 'Information Technology Program', 8, 120, 'Active'),
        ('BA001', 'Business Administration', 'Business Administration Program', 8, 120, 'Active'),
        ('ENG001', 'English Language', 'English Language Program', 8, 120, 'Active');
    PRINT 'Created sample academic programs';
END
ELSE
BEGIN
    PRINT 'Sample academic programs already exist';
END
GO

-- Tạo categories mẫu (chỉ nếu chưa có)
IF NOT EXISTS (SELECT * FROM categories WHERE name = 'Assignment')
BEGIN
    INSERT INTO categories (name) VALUES 
        ('Assignment'),
        ('Project'),
        ('Exam'),
        ('Homework');
    PRINT 'Created sample categories';
END
ELSE
BEGIN
    PRINT 'Sample categories already exist';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Database setup completed successfully!';
PRINT '========================================';
GO

