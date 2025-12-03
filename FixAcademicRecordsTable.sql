-- ============================================
-- SỬA/TẠO BẢNG ACADEMIC RECORDS ĐỂ LƯU GRADE
-- ============================================

PRINT '========================================';
PRINT 'Checking and fixing AcademicRecords table...';
PRINT '========================================';
GO

-- Kiểm tra và tạo bảng AcademicRecords nếu chưa có
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
            FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id),
        CONSTRAINT FK_AcademicRecords_Teacher 
            FOREIGN KEY (GradedBy) REFERENCES Teachers(Id) ON DELETE SET NULL
    );
    
    PRINT 'AcademicRecords table created successfully!';
END
ELSE
BEGIN
    PRINT 'AcademicRecords table already exists.';
    
    -- Kiểm tra và thêm các cột nếu thiếu
    IF COL_LENGTH('AcademicRecords', 'EnrollmentId') IS NULL
    BEGIN
        ALTER TABLE AcademicRecords ADD EnrollmentId INT NULL;
        PRINT 'Added EnrollmentId column.';
    END
    
    IF COL_LENGTH('AcademicRecords', 'Weight') IS NULL
    BEGIN
        ALTER TABLE AcademicRecords ADD Weight DECIMAL(5,2) NULL CHECK (Weight >= 0 AND Weight <= 100);
        PRINT 'Added Weight column.';
    END
    
    IF COL_LENGTH('AcademicRecords', 'Notes') IS NULL
    BEGIN
        ALTER TABLE AcademicRecords ADD Notes NVARCHAR(500) NULL;
        PRINT 'Added Notes column.';
    END
    
    -- Kiểm tra và thêm Foreign Key nếu thiếu
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AcademicRecords_Enrollment')
    BEGIN
        ALTER TABLE AcademicRecords
        ADD CONSTRAINT FK_AcademicRecords_Enrollment 
            FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id);
        PRINT 'Added FK_AcademicRecords_Enrollment constraint.';
    END
END
GO

-- Tạo các Indexes để tăng tốc độ truy vấn
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_StudentId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_StudentId ON AcademicRecords(StudentId);
    PRINT 'Created index IX_AcademicRecords_StudentId';
END
ELSE
BEGIN
    PRINT 'Index IX_AcademicRecords_StudentId already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_CourseId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_CourseId ON AcademicRecords(CourseId);
    PRINT 'Created index IX_AcademicRecords_CourseId';
END
ELSE
BEGIN
    PRINT 'Index IX_AcademicRecords_CourseId already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_EnrollmentId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_EnrollmentId ON AcademicRecords(EnrollmentId);
    PRINT 'Created index IX_AcademicRecords_EnrollmentId';
END
ELSE
BEGIN
    PRINT 'Index IX_AcademicRecords_EnrollmentId already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_GradedBy' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_GradedBy ON AcademicRecords(GradedBy);
    PRINT 'Created index IX_AcademicRecords_GradedBy';
END
ELSE
BEGIN
    PRINT 'Index IX_AcademicRecords_GradedBy already exists.';
END
GO

-- Kiểm tra lại cấu trúc bảng
PRINT '';
PRINT '========================================';
PRINT 'AcademicRecords table structure:';
PRINT '========================================';

SELECT 
    COLUMN_NAME as 'Column Name',
    DATA_TYPE as 'Data Type',
    IS_NULLABLE as 'Nullable',
    COLUMN_DEFAULT as 'Default Value'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AcademicRecords'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '========================================';
PRINT 'SUCCESS: AcademicRecords table is ready!';
PRINT '========================================';
PRINT '';
PRINT 'You can now save grades using the Grade Management page.';
PRINT 'Grades will be stored in this table with:';
PRINT '  - StudentId: ID của student';
PRINT '  - CourseId: ID của course';
PRINT '  - Score: Điểm số (0-100)';
PRINT '  - AssignmentType: Loại điểm (Final, Midterm, etc.)';
PRINT '  - GradedBy: ID của teacher chấm điểm';
PRINT '';
GO

