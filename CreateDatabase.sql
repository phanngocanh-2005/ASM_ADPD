-- ============================================
-- CREATE DATABASE - Script đơn giản
-- Chỉ dùng CREATE TABLE, không có IF EXISTS
-- Copy và chạy trong SQL Server Management Studio
-- ============================================

-- ============================================
-- 1. BẢNG ACCOUNT
-- ============================================
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
GO

-- ============================================
-- 2. BẢNG ACADEMIC PROGRAMS
-- ============================================
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
GO

-- ============================================
-- 3. BẢNG STUDENTS
-- ============================================
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
GO

-- ============================================
-- 4. BẢNG TEACHERS
-- ============================================
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
GO

-- ============================================
-- 5. BẢNG COURSES
-- ============================================
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
GO

-- ============================================
-- 6. BẢNG ENROLLMENTS (Student đăng ký Course)
-- ============================================
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
GO

-- ============================================
-- 7. BẢNG COURSE ASSIGNMENTS (Teacher được phân công dạy Course)
-- ============================================
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
GO

-- ============================================
-- 8. BẢNG ACADEMIC RECORDS (Điểm số chi tiết)
-- ============================================
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
    CONSTRAINT FK_AcademicRecords_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_AcademicRecords_Teacher FOREIGN KEY (GradedBy) REFERENCES Teachers(Id) ON DELETE SET NULL
);
GO

-- ============================================
-- 9. BẢNG CATEGORIES (Cho Tasks)
-- ============================================
CREATE TABLE categories (
    id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(255) NOT NULL
);
GO

-- ============================================
-- 10. BẢNG TASKS
-- ============================================
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
GO

-- ============================================
-- CREATE INDEXES
-- ============================================
CREATE INDEX IX_Account_Username ON Account(Username);
CREATE INDEX IX_Account_Email ON Account(Email);
CREATE INDEX IX_Account_Role ON Account(Role);

CREATE UNIQUE NONCLUSTERED INDEX IX_Students_AccountId ON Students(AccountId) WHERE AccountId IS NOT NULL;
CREATE INDEX IX_Students_StudentCode ON Students(StudentCode);
CREATE INDEX IX_Students_AcademicProgramId ON Students(AcademicProgramId);

CREATE UNIQUE NONCLUSTERED INDEX IX_Teachers_AccountId ON Teachers(AccountId) WHERE AccountId IS NOT NULL;
CREATE INDEX IX_Teachers_TeacherCode ON Teachers(TeacherCode);
CREATE INDEX IX_Teachers_AcademicProgramId ON Teachers(AcademicProgramId);

CREATE INDEX IX_Courses_CourseCode ON Courses(CourseCode);
CREATE INDEX IX_Courses_AcademicProgramId ON Courses(AcademicProgramId);

CREATE INDEX IX_Enrollments_StudentId ON Enrollments(StudentId);
CREATE INDEX IX_Enrollments_CourseId ON Enrollments(CourseId);

CREATE INDEX IX_CourseAssignments_TeacherId ON CourseAssignments(TeacherId);
CREATE INDEX IX_CourseAssignments_CourseId ON CourseAssignments(CourseId);

CREATE INDEX IX_AcademicRecords_StudentId ON AcademicRecords(StudentId);
CREATE INDEX IX_AcademicRecords_CourseId ON AcademicRecords(CourseId);
CREATE INDEX IX_AcademicRecords_EnrollmentId ON AcademicRecords(EnrollmentId);
GO

-- ============================================
-- INSERT SAMPLE DATA
-- ============================================
-- Tạo tài khoản Admin mặc định (password: admin123)
INSERT INTO Account (Username, Fullname, Email, Role, PasswordHash, IsActive, CreatedAt)
VALUES ('admin', 'Administrator', 'admin@btec.edu.vn', 'Admin', 'admin123', 1, GETDATE());
GO

-- Tạo chương trình học mẫu
INSERT INTO AcademicPrograms (ProgramCode, ProgramName, Description, Duration, CreditsRequired, Status)
VALUES 
    ('IT001', 'Information Technology', 'Information Technology Program', 8, 120, 'Active'),
    ('BA001', 'Business Administration', 'Business Administration Program', 8, 120, 'Active'),
    ('ENG001', 'English Language', 'English Language Program', 8, 120, 'Active');
GO

-- Tạo categories mẫu
INSERT INTO categories (name) VALUES 
    ('Assignment'),
    ('Project'),
    ('Exam'),
    ('Homework');
GO

PRINT 'Database created successfully!';
GO

