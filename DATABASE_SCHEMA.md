# Cấu trúc Database - Student Information Management System

## Tổng quan

Hệ thống quản lý thông tin sinh viên cần **8 bảng chính** để đáp ứng các yêu cầu:

1. **Student Registration**: Đăng ký và quản lý thông tin sinh viên chi tiết
2. **Course Management**: Quản lý khóa học và phân công sinh viên vào khóa học
3. **User Authentication & Authorization**: Xác thực và phân quyền người dùng (Admin, Teacher, Student)
4. **Academic Records**: Lưu trữ kết quả học tập và bảng điểm

---

## 1. Bảng `Account` (Quản lý tài khoản người dùng)

Bảng này lưu trữ thông tin đăng nhập và xác thực cho tất cả người dùng trong hệ thống.

### Các cột:

| Tên cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|---------|--------------|-----------|-------|
| `Id` | INT | PRIMARY KEY, IDENTITY | ID duy nhất của tài khoản |
| `Username` | NVARCHAR(50) | NOT NULL, UNIQUE | Tên đăng nhập (duy nhất) |
| `PasswordHash` | NVARCHAR(255) | NOT NULL | Mật khẩu đã được hash (BCrypt/PBKDF2) |
| `Email` | NVARCHAR(100) | NOT NULL, UNIQUE | Địa chỉ email (duy nhất) |
| `Role` | NVARCHAR(20) | NOT NULL | Vai trò: "Admin", "Teacher", hoặc "Student" |
| `IsActive` | BIT | NOT NULL, DEFAULT 1 | Trạng thái tài khoản (1=Active, 0=Inactive) |
| `LastLoginAt` | DATETIME | NULL | Thời gian đăng nhập cuối cùng |
| `CreatedAt` | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo tài khoản |
| `UpdatedAt` | DATETIME | NULL | Thời gian cập nhật cuối cùng |

### Mối quan hệ:
- Một Account có thể có một Student (1-1)
- Một Account có thể có một Teacher (1-1)
- Một Account có thể có nhiều Tasks (1-N)

---

## 2. Bảng `Students` (Thông tin sinh viên)

Bảng này lưu trữ thông tin chi tiết của sinh viên, bao gồm personal details và academic records.

### Các cột:

| Tên cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|---------|--------------|-----------|-------|
| `Id` | INT | PRIMARY KEY, IDENTITY | ID duy nhất của sinh viên |
| `AccountId` | INT | NOT NULL, UNIQUE, FOREIGN KEY | ID tài khoản (tham chiếu `Account.Id`) |
| `StudentCode` | NVARCHAR(20) | NOT NULL, UNIQUE | Mã sinh viên (duy nhất) |
| `FullName` | NVARCHAR(100) | NOT NULL | Họ và tên đầy đủ |
| `DateOfBirth` | DATE | NOT NULL | Ngày sinh |
| `Gender` | NVARCHAR(10) | NULL | Giới tính: "Male", "Female", "Other" |
| `PhoneNumber` | NVARCHAR(20) | NULL | Số điện thoại |
| `Address` | NVARCHAR(255) | NULL | Địa chỉ thường trú |
| `AcademicProgramId` | INT | NOT NULL, FOREIGN KEY | ID chương trình học (tham chiếu `AcademicPrograms.Id`) |
| `EnrollmentDate` | DATE | NOT NULL | Ngày nhập học |
| `Status` | NVARCHAR(20) | NOT NULL, DEFAULT 'Active' | Trạng thái: "Active", "Graduated", "Suspended", "Withdrawn" |
| `GPA` | DECIMAL(3,2) | NULL | Điểm trung bình tích lũy (0.00 - 4.00) |
| `CreatedAt` | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo bản ghi |
| `UpdatedAt` | DATETIME | NULL | Thời gian cập nhật cuối cùng |

### Mối quan hệ:
- Một Student thuộc về một Account (1-1)
- Một Student thuộc về một AcademicProgram (N-1)
- Một Student có thể có nhiều Enrollments (1-N)
- Một Student có thể có nhiều AcademicRecords (1-N)

---

## 3. Bảng `Teachers` (Thông tin giáo viên)

Bảng này lưu trữ thông tin chi tiết của giáo viên.

### Các cột:

| Tên cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|---------|--------------|-----------|-------|
| `Id` | INT | PRIMARY KEY, IDENTITY | ID duy nhất của giáo viên |
| `AccountId` | INT | NOT NULL, UNIQUE, FOREIGN KEY | ID tài khoản (tham chiếu `Account.Id`) |
| `TeacherCode` | NVARCHAR(20) | NOT NULL, UNIQUE | Mã giáo viên (duy nhất) |
| `FullName` | NVARCHAR(100) | NOT NULL | Họ và tên đầy đủ |
| `DateOfBirth` | DATE | NULL | Ngày sinh |
| `Gender` | NVARCHAR(10) | NULL | Giới tính |
| `PhoneNumber` | NVARCHAR(20) | NULL | Số điện thoại |
| `Email` | NVARCHAR(100) | NULL | Email liên hệ |
| `Department` | NVARCHAR(100) | NULL | Khoa/Bộ môn |
| `Specialization` | NVARCHAR(255) | NULL | Chuyên ngành |
| `Status` | NVARCHAR(20) | NOT NULL, DEFAULT 'Active' | Trạng thái: "Active", "Inactive", "Retired" |
| `CreatedAt` | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo bản ghi |
| `UpdatedAt` | DATETIME | NULL | Thời gian cập nhật cuối cùng |

### Mối quan hệ:
- Một Teacher thuộc về một Account (1-1)
- Một Teacher có thể dạy nhiều Courses (1-N qua CourseAssignments)

---

## 4. Bảng `AcademicPrograms` (Chương trình học)

Bảng này lưu trữ thông tin các chương trình đào tạo của trường.

### Các cột:

| Tên cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|---------|--------------|-----------|-------|
| `Id` | INT | PRIMARY KEY, IDENTITY | ID duy nhất của chương trình |
| `ProgramCode` | NVARCHAR(20) | NOT NULL, UNIQUE | Mã chương trình (duy nhất) |
| `ProgramName` | NVARCHAR(200) | NOT NULL | Tên chương trình |
| `Description` | NVARCHAR(500) | NULL | Mô tả chương trình |
| `Duration` | INT | NULL | Thời gian đào tạo (số học kỳ) |
| `CreditsRequired` | INT | NULL | Số tín chỉ yêu cầu |
| `Status` | NVARCHAR(20) | NOT NULL, DEFAULT 'Active' | Trạng thái: "Active", "Inactive" |
| `CreatedAt` | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedAt` | DATETIME | NULL | Thời gian cập nhật |

### Mối quan hệ:
- Một AcademicProgram có thể có nhiều Students (1-N)
- Một AcademicProgram có thể có nhiều Courses (1-N)

---

## 5. Bảng `Courses` (Khóa học)

Bảng này lưu trữ thông tin các khóa học được cung cấp bởi trường.

### Các cột:

| Tên cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|---------|--------------|-----------|-------|
| `Id` | INT | PRIMARY KEY, IDENTITY | ID duy nhất của khóa học |
| `CourseCode` | NVARCHAR(20) | NOT NULL, UNIQUE | Mã khóa học (duy nhất) |
| `CourseName` | NVARCHAR(200) | NOT NULL | Tên khóa học |
| `Description` | NVARCHAR(500) | NULL | Mô tả khóa học |
| `AcademicProgramId` | INT | NOT NULL, FOREIGN KEY | ID chương trình học (tham chiếu `AcademicPrograms.Id`) |
| `Credits` | INT | NOT NULL | Số tín chỉ |
| `Semester` | INT | NULL | Học kỳ (1, 2, 3, ...) |
| `AcademicYear` | NVARCHAR(10) | NULL | Năm học (VD: "2024-2025") |
| `MaxStudents` | INT | NULL | Số lượng sinh viên tối đa |
| `Status` | NVARCHAR(20) | NOT NULL, DEFAULT 'Active' | Trạng thái: "Active", "Inactive", "Completed" |
| `CreatedAt` | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedAt` | DATETIME | NULL | Thời gian cập nhật |

### Mối quan hệ:
- Một Course thuộc về một AcademicProgram (N-1)
- Một Course có thể có nhiều Enrollments (1-N)
- Một Course có thể có nhiều CourseAssignments (1-N)
- Một Course có thể có nhiều AcademicRecords (1-N)

---

## 6. Bảng `Enrollments` (Đăng ký khóa học)

Bảng này quản lý việc đăng ký khóa học của sinh viên (Many-to-Many giữa Students và Courses).

### Các cột:

| Tên cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|---------|--------------|-----------|-------|
| `Id` | INT | PRIMARY KEY, IDENTITY | ID duy nhất của bản ghi đăng ký |
| `StudentId` | INT | NOT NULL, FOREIGN KEY | ID sinh viên (tham chiếu `Students.Id`) |
| `CourseId` | INT | NOT NULL, FOREIGN KEY | ID khóa học (tham chiếu `Courses.Id`) |
| `EnrollmentDate` | DATETIME | NOT NULL, DEFAULT GETDATE() | Ngày đăng ký |
| `Status` | NVARCHAR(20) | NOT NULL, DEFAULT 'Enrolled' | Trạng thái: "Enrolled", "Completed", "Dropped", "Failed" |
| `FinalGrade` | NVARCHAR(5) | NULL | Điểm cuối khóa: "A", "B", "C", "D", "F" |
| `CreatedAt` | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedAt` | DATETIME | NULL | Thời gian cập nhật |

### Mối quan hệ:
- Một Enrollment thuộc về một Student (N-1)
- Một Enrollment thuộc về một Course (N-1)
- **Ràng buộc UNIQUE**: Một sinh viên không thể đăng ký cùng một khóa học nhiều lần (trừ khi đã dropped/failed)

---

## 7. Bảng `CourseAssignments` (Phân công giáo viên)

Bảng này quản lý việc phân công giáo viên dạy các khóa học.

### Các cột:

| Tên cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|---------|--------------|-----------|-------|
| `Id` | INT | PRIMARY KEY, IDENTITY | ID duy nhất |
| `TeacherId` | INT | NOT NULL, FOREIGN KEY | ID giáo viên (tham chiếu `Teachers.Id`) |
| `CourseId` | INT | NOT NULL, FOREIGN KEY | ID khóa học (tham chiếu `Courses.Id`) |
| `AssignmentDate` | DATETIME | NOT NULL, DEFAULT GETDATE() | Ngày phân công |
| `Status` | NVARCHAR(20) | NOT NULL, DEFAULT 'Active' | Trạng thái: "Active", "Completed", "Cancelled" |
| `CreatedAt` | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |

### Mối quan hệ:
- Một CourseAssignment thuộc về một Teacher (N-1)
- Một CourseAssignment thuộc về một Course (N-1)
- **Ràng buộc**: Một giáo viên có thể dạy nhiều khóa học, một khóa học có thể có nhiều giáo viên

---

## 8. Bảng `AcademicRecords` (Bảng điểm học tập)

Bảng này lưu trữ chi tiết điểm số và kết quả học tập của sinh viên.

### Các cột:

| Tên cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|---------|--------------|-----------|-------|
| `Id` | INT | PRIMARY KEY, IDENTITY | ID duy nhất |
| `StudentId` | INT | NOT NULL, FOREIGN KEY | ID sinh viên (tham chiếu `Students.Id`) |
| `CourseId` | INT | NOT NULL, FOREIGN KEY | ID khóa học (tham chiếu `Courses.Id`) |
| `EnrollmentId` | INT | NULL, FOREIGN KEY | ID đăng ký (tham chiếu `Enrollments.Id`) |
| `AssignmentType` | NVARCHAR(50) | NOT NULL | Loại điểm: "Quiz", "Midterm", "Final", "Assignment", "Project", "Participation" |
| `Score` | DECIMAL(5,2) | NOT NULL | Điểm số (0.00 - 100.00) |
| `MaxScore` | DECIMAL(5,2) | NOT NULL, DEFAULT 100.00 | Điểm tối đa |
| `Weight` | DECIMAL(5,2) | NULL | Trọng số (phần trăm) |
| `GradedDate` | DATETIME | NULL | Ngày chấm điểm |
| `GradedBy` | INT | NULL, FOREIGN KEY | ID giáo viên chấm (tham chiếu `Teachers.Id`) |
| `Notes` | NVARCHAR(500) | NULL | Ghi chú |
| `CreatedAt` | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedAt` | DATETIME | NULL | Thời gian cập nhật |

### Mối quan hệ:
- Một AcademicRecord thuộc về một Student (N-1)
- Một AcademicRecord thuộc về một Course (N-1)
- Một AcademicRecord có thể thuộc về một Enrollment (N-1)
- Một AcademicRecord có thể được chấm bởi một Teacher (N-1)

---

## 9. Bảng `categories` (Danh mục - Tùy chọn)

Bảng này lưu trữ các danh mục phân loại cho các công việc/nhiệm vụ (nếu cần).

### Các cột:

| Tên cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|---------|--------------|-----------|-------|
| `id` | INT | PRIMARY KEY, IDENTITY | ID duy nhất của danh mục |
| `name` | NVARCHAR(255) | NOT NULL | Tên danh mục |

---

## 10. Bảng `tasks` (Công việc/Nhiệm vụ - Tùy chọn)

Bảng này lưu trữ các công việc/nhiệm vụ được giao cho người dùng (nếu cần).

### Các cột:

| Tên cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|---------|--------------|-----------|-------|
| `id` | INT | PRIMARY KEY, IDENTITY | ID duy nhất của công việc |
| `name` | NVARCHAR(255) | NOT NULL | Tên công việc |
| `description` | NVARCHAR(500) | NOT NULL | Mô tả chi tiết công việc |
| `category_id` | INT | NULL, FOREIGN KEY | ID danh mục (tham chiếu `categories.id`) |
| `account_id` | INT | NOT NULL, FOREIGN KEY | ID tài khoản (tham chiếu `Account.Id`) |
| `status` | NVARCHAR(20) | NOT NULL, DEFAULT 'Pending' | Trạng thái: "Pending", "InProgress", "Completed" |
| `due_date` | DATETIME | NULL | Hạn chót |
| `created_at` | DATETIME | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |

---

## Sơ đồ quan hệ (ER Diagram)

```
Account (1) ──────< (1) Students
    │                  │
    │                  ├───< (N) Enrollments >─── (N) Courses
    │                  │                              │
    │                  └───< (N) AcademicRecords      │
    │                                                  │
    └───< (1) Teachers ────< (N) CourseAssignments ───┘
                              │
                              └───< (N) AcademicRecords

Students ────< (N) AcademicPrograms >─── (N) Courses
```

---

## Script SQL tạo database (SQL Server)

```sql
-- ============================================
-- TẠO DATABASE
-- ============================================
CREATE DATABASE StudentInfoManagement;
GO

USE StudentInfoManagement;
GO

-- ============================================
-- 1. BẢNG ACCOUNT
-- ============================================
CREATE TABLE Account (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'Teacher', 'Student')),
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
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
    AccountId INT NOT NULL UNIQUE,
    StudentCode NVARCHAR(20) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10) NULL CHECK (Gender IN ('Male', 'Female', 'Other')),
    PhoneNumber NVARCHAR(20) NULL,
    Address NVARCHAR(255) NULL,
    AcademicProgramId INT NOT NULL,
    EnrollmentDate DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Graduated', 'Suspended', 'Withdrawn')),
    GPA DECIMAL(3,2) NULL CHECK (GPA >= 0 AND GPA <= 4.00),
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
    AccountId INT NOT NULL UNIQUE,
    TeacherCode NVARCHAR(20) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NULL,
    Gender NVARCHAR(10) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    Department NVARCHAR(100) NULL,
    Specialization NVARCHAR(255) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Inactive', 'Retired')),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Teachers_Account FOREIGN KEY (AccountId) REFERENCES Account(Id) ON DELETE CASCADE
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
-- 6. BẢNG ENROLLMENTS
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
-- 7. BẢNG COURSE ASSIGNMENTS
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
-- 8. BẢNG ACADEMIC RECORDS
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
    CONSTRAINT FK_AcademicRecords_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id) ON DELETE SET NULL,
    CONSTRAINT FK_AcademicRecords_Teacher FOREIGN KEY (GradedBy) REFERENCES Teachers(Id) ON DELETE SET NULL
);
GO

-- ============================================
-- 9. BẢNG CATEGORIES (Tùy chọn)
-- ============================================
CREATE TABLE categories (
    id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(255) NOT NULL
);
GO

-- ============================================
-- 10. BẢNG TASKS (Tùy chọn)
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
-- TẠO INDEX ĐỂ TỐI ƯU HIỆU SUẤT
-- ============================================
CREATE INDEX IX_Account_Username ON Account(Username);
CREATE INDEX IX_Account_Email ON Account(Email);
CREATE INDEX IX_Account_Role ON Account(Role);
CREATE INDEX IX_Students_AccountId ON Students(AccountId);
CREATE INDEX IX_Students_StudentCode ON Students(StudentCode);
CREATE INDEX IX_Students_AcademicProgramId ON Students(AcademicProgramId);
CREATE INDEX IX_Teachers_AccountId ON Teachers(AccountId);
CREATE INDEX IX_Teachers_TeacherCode ON Teachers(TeacherCode);
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
-- CHÈN DỮ LIỆU MẪU
-- ============================================

-- Tạo tài khoản Admin mặc định
INSERT INTO Account (Username, PasswordHash, Email, Role, IsActive, CreatedAt)
VALUES ('admin', 'AQAAAAIAAYagAAAAE...', 'admin@btec.edu.vn', 'Admin', 1, GETDATE());
GO

-- Tạo chương trình học mẫu
INSERT INTO AcademicPrograms (ProgramCode, ProgramName, Description, Duration, CreditsRequired, Status)
VALUES 
('IT001', 'Information Technology', 'Chương trình Công nghệ Thông tin', 8, 120, 'Active'),
('BA001', 'Business Administration', 'Chương trình Quản trị Kinh doanh', 8, 120, 'Active'),
('ENG001', 'English Language', 'Chương trình Ngôn ngữ Anh', 8, 120, 'Active');
GO

-- Tạo danh mục mẫu (nếu cần)
INSERT INTO categories (name) VALUES 
('Assignment'),
('Project'),
('Exam'),
('Homework');
GO
```

---

## Lưu ý quan trọng

### 1. **Bảo mật**
- **Mật khẩu**: Luôn sử dụng hashing (BCrypt, PBKDF2, Argon2) - KHÔNG lưu plain text
- **SQL Injection**: Sử dụng parameterized queries (Entity Framework đã xử lý)
- **XSS Protection**: Validate và sanitize input từ người dùng
- **HTTPS**: Sử dụng HTTPS trong production
- **Role-based Access Control**: Đã được implement qua `[Authorize(Roles = "...")]`

### 2. **Tối ưu hiệu suất**
- Đã tạo index cho các cột thường xuyên được query
- Sử dụng pagination cho danh sách dài
- Cache dữ liệu thường xuyên truy cập
- Connection pooling (Entity Framework tự động)

### 3. **Tính toàn vẹn dữ liệu**
- Foreign Key Constraints đảm bảo tính toàn vẹn tham chiếu
- CHECK Constraints đảm bảo dữ liệu hợp lệ
- UNIQUE Constraints đảm bảo không trùng lặp
- CASCADE DELETE cho các quan hệ phụ thuộc

### 4. **Scalability**
- Database có thể scale theo chiều ngang (sharding) hoặc chiều dọc (upgrade hardware)
- Index giúp query nhanh hơn khi dữ liệu tăng
- Partitioning cho các bảng lớn (nếu cần)

### 5. **Backup và Recovery**
- Thiết lập backup tự động hàng ngày
- Test restore procedure định kỳ
- Transaction log backup cho point-in-time recovery

### 6. **Connection String**
Cấu hình trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentInfoManagement;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Migration với Entity Framework Core

Sau khi cập nhật Models, chạy các lệnh sau:

```bash
# Tạo migration
dotnet ef migrations add InitialCreate

# Cập nhật database
dotnet ef database update
```

---

## Tóm tắt các bảng

| STT | Tên bảng | Mục đích | Số cột chính |
|-----|----------|----------|--------------|
| 1 | Account | Quản lý tài khoản đăng nhập | 8 |
| 2 | Students | Thông tin chi tiết sinh viên | 13 |
| 3 | Teachers | Thông tin chi tiết giáo viên | 12 |
| 4 | AcademicPrograms | Chương trình đào tạo | 9 |
| 5 | Courses | Khóa học | 11 |
| 6 | Enrollments | Đăng ký khóa học | 7 |
| 7 | CourseAssignments | Phân công giáo viên | 6 |
| 8 | AcademicRecords | Bảng điểm học tập | 12 |
| 9 | categories | Danh mục (tùy chọn) | 2 |
| 10 | tasks | Công việc (tùy chọn) | 7 |

**Tổng cộng: 10 bảng** (8 bảng chính + 2 bảng tùy chọn)



