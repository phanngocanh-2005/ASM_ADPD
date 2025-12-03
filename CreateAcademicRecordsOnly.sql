-- ============================================
-- TẠO/SỬA BẢNG ACADEMIC RECORDS - PHIÊN BẢN TỐI GIẢN
-- Chạy từng phần một nếu cần
-- ============================================

-- PHẦN 1: Tạo bảng nếu chưa có
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
END
GO

-- PHẦN 2: Thêm cột EnrollmentId nếu chưa có
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
    AND COL_LENGTH('AcademicRecords', 'EnrollmentId') IS NULL
BEGIN
    ALTER TABLE AcademicRecords ADD EnrollmentId INT NULL;
    PRINT 'Added EnrollmentId column.';
END
GO

-- PHẦN 3: Thêm cột Weight nếu chưa có
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
    AND COL_LENGTH('AcademicRecords', 'Weight') IS NULL
BEGIN
    ALTER TABLE AcademicRecords ADD Weight DECIMAL(5,2) NULL;
    PRINT 'Added Weight column.';
END
GO

-- PHẦN 4: Thêm cột Notes nếu chưa có
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
    AND COL_LENGTH('AcademicRecords', 'Notes') IS NULL
BEGIN
    ALTER TABLE AcademicRecords ADD Notes NVARCHAR(500) NULL;
    PRINT 'Added Notes column.';
END
GO

-- PHẦN 5: Tạo Indexes
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
    AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_StudentId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_StudentId ON AcademicRecords(StudentId);
    PRINT 'Created index IX_AcademicRecords_StudentId.';
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
    AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_CourseId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_CourseId ON AcademicRecords(CourseId);
    PRINT 'Created index IX_AcademicRecords_CourseId.';
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
    AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_EnrollmentId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_EnrollmentId ON AcademicRecords(EnrollmentId);
    PRINT 'Created index IX_AcademicRecords_EnrollmentId.';
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
    AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_GradedBy' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_GradedBy ON AcademicRecords(GradedBy);
    PRINT 'Created index IX_AcademicRecords_GradedBy.';
END
GO

PRINT '========================================';
PRINT 'AcademicRecords table setup completed!';
PRINT '========================================';
GO

