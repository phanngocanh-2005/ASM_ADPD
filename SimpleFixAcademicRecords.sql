-- ============================================
-- SỬA BẢNG ACADEMIC RECORDS - PHIÊN BẢN ĐƠN GIẢN
-- Chỉ tập trung vào việc đảm bảo bảng có đầy đủ cấu trúc
-- ============================================

-- Kiểm tra và tạo bảng nếu chưa có
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
        CONSTRAINT FK_AcademicRecords_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id),
        CONSTRAINT FK_AcademicRecords_Teacher FOREIGN KEY (GradedBy) REFERENCES Teachers(Id) ON DELETE SET NULL
    );
    PRINT 'Created AcademicRecords table.';
END
ELSE
BEGIN
    PRINT 'AcademicRecords table already exists.';
    
    -- Chỉ thêm các cột còn thiếu
    IF COL_LENGTH('AcademicRecords', 'EnrollmentId') IS NULL
    BEGIN
        ALTER TABLE AcademicRecords ADD EnrollmentId INT NULL;
        PRINT 'Added EnrollmentId column.';
    END;
    
    IF COL_LENGTH('AcademicRecords', 'Weight') IS NULL
    BEGIN
        ALTER TABLE AcademicRecords ADD Weight DECIMAL(5,2) NULL;
        PRINT 'Added Weight column.';
    END;
    
    IF COL_LENGTH('AcademicRecords', 'Notes') IS NULL
    BEGIN
        ALTER TABLE AcademicRecords ADD Notes NVARCHAR(500) NULL;
        PRINT 'Added Notes column.';
    END;
END;
GO

-- Tạo Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_StudentId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
    CREATE INDEX IX_AcademicRecords_StudentId ON AcademicRecords(StudentId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_CourseId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
    CREATE INDEX IX_AcademicRecords_CourseId ON AcademicRecords(CourseId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_EnrollmentId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
    CREATE INDEX IX_AcademicRecords_EnrollmentId ON AcademicRecords(EnrollmentId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_GradedBy' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
    CREATE INDEX IX_AcademicRecords_GradedBy ON AcademicRecords(GradedBy);
GO

PRINT 'AcademicRecords table is ready!';
GO

