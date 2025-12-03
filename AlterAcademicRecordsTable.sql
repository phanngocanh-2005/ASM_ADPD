-- ============================================
-- SỬA BẢNG ACADEMIC RECORDS - CHỈ THÊM CÁC PHẦN CÒN THIẾU
-- Script này chỉ ALTER TABLE, không thay đổi code hiện có
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
    -- Nếu bảng đã tồn tại, chỉ thêm các cột/constraint còn thiếu
    
    -- Thêm cột EnrollmentId nếu chưa có
    IF COL_LENGTH('AcademicRecords', 'EnrollmentId') IS NULL
    BEGIN
        ALTER TABLE AcademicRecords ADD EnrollmentId INT NULL;
        PRINT 'Added EnrollmentId column.';
    END
    
    -- Thêm cột Weight nếu chưa có
    IF COL_LENGTH('AcademicRecords', 'Weight') IS NULL
    BEGIN
        ALTER TABLE AcademicRecords ADD Weight DECIMAL(5,2) NULL;
        -- Thêm CHECK constraint cho Weight
        ALTER TABLE AcademicRecords ADD CONSTRAINT CK_AcademicRecords_Weight 
            CHECK (Weight IS NULL OR (Weight >= 0 AND Weight <= 100));
        PRINT 'Added Weight column.';
    END
    
    -- Thêm cột Notes nếu chưa có
    IF COL_LENGTH('AcademicRecords', 'Notes') IS NULL
    BEGIN
        ALTER TABLE AcademicRecords ADD Notes NVARCHAR(500) NULL;
        PRINT 'Added Notes column.';
    END
    
    -- Thêm Foreign Key cho EnrollmentId nếu chưa có
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AcademicRecords_Enrollment')
    BEGIN
        ALTER TABLE AcademicRecords
        ADD CONSTRAINT FK_AcademicRecords_Enrollment 
            FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(Id);
        PRINT 'Added FK_AcademicRecords_Enrollment constraint.';
    END
    
    -- Thêm Foreign Key cho GradedBy (Teacher) nếu chưa có
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AcademicRecords_Teacher')
    BEGIN
        ALTER TABLE AcademicRecords
        ADD CONSTRAINT FK_AcademicRecords_Teacher 
            FOREIGN KEY (GradedBy) REFERENCES Teachers(Id) ON DELETE SET NULL;
        PRINT 'Added FK_AcademicRecords_Teacher constraint.';
    END
    
    -- Thêm CHECK constraint cho Score nếu chưa có
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_AcademicRecords_Score')
    BEGIN
        ALTER TABLE AcademicRecords
        ADD CONSTRAINT CK_AcademicRecords_Score CHECK (Score >= 0);
        PRINT 'Added CK_AcademicRecords_Score constraint.';
    END
    
    -- Thêm CHECK constraint cho MaxScore nếu chưa có
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_AcademicRecords_MaxScore')
    BEGIN
        ALTER TABLE AcademicRecords
        ADD CONSTRAINT CK_AcademicRecords_MaxScore CHECK (MaxScore > 0);
        PRINT 'Added CK_AcademicRecords_MaxScore constraint.';
    END
    
    PRINT 'AcademicRecords table already exists. Added missing columns/constraints if any.';
END
GO

-- Tạo Indexes nếu chưa có (để tăng tốc độ truy vấn)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_StudentId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_StudentId ON AcademicRecords(StudentId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_CourseId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_CourseId ON AcademicRecords(CourseId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_EnrollmentId' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_EnrollmentId ON AcademicRecords(EnrollmentId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_GradedBy' AND object_id = OBJECT_ID('dbo.AcademicRecords'))
BEGIN
    CREATE INDEX IX_AcademicRecords_GradedBy ON AcademicRecords(GradedBy);
END
GO

PRINT '';
PRINT '========================================';
PRINT 'AcademicRecords table is ready!';
PRINT '========================================';
GO

