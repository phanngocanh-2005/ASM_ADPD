-- ============================================
-- KIỂM TRA VÀ TẠO BẢNG ACADEMIC RECORDS
-- Bảng này dùng để lưu điểm của student
-- ============================================

-- Kiểm tra xem bảng AcademicRecords đã tồn tại chưa
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
BEGIN
    PRINT 'Creating AcademicRecords table...';
    
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
        
        -- Foreign Keys
        CONSTRAINT FK_AcademicRecords_Student 
            FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AcademicRecords_Course 
            FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AcademicRecords_Enrollment 
            FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id) ON DELETE SET NULL,
        CONSTRAINT FK_AcademicRecords_Teacher 
            FOREIGN KEY (GradedBy) REFERENCES Teachers(Id) ON DELETE SET NULL
    );
    
    PRINT 'AcademicRecords table created successfully!';
END
ELSE
BEGIN
    PRINT 'AcademicRecords table already exists.';
END
GO

-- Tạo Indexes để tăng tốc độ truy vấn
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_StudentId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_StudentId ON AcademicRecords(StudentId);
    PRINT 'Created index IX_AcademicRecords_StudentId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_CourseId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_CourseId ON AcademicRecords(CourseId);
    PRINT 'Created index IX_AcademicRecords_CourseId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_EnrollmentId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_EnrollmentId ON AcademicRecords(EnrollmentId);
    PRINT 'Created index IX_AcademicRecords_EnrollmentId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_GradedBy' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_GradedBy ON AcademicRecords(GradedBy);
    PRINT 'Created index IX_AcademicRecords_GradedBy';
END
GO

-- Kiểm tra lại bảng đã được tạo chưa
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
BEGIN
    PRINT '';
    PRINT '========================================';
    PRINT 'SUCCESS: AcademicRecords table is ready!';
    PRINT '========================================';
    PRINT '';
    PRINT 'You can now save grades using the Grade Management page.';
    PRINT 'Grades will be stored in this table.';
END
ELSE
BEGIN
    PRINT 'ERROR: Failed to create AcademicRecords table.';
END
GO

