-- ============================================
-- QUICK FIX: Create Schedules Table
-- ============================================
-- Run this script in SQL Server Management Studio
-- to fix the "Invalid object name 'Schedules'" error
-- ============================================

-- Step 1: Check if Courses table exists (required for foreign key)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
BEGIN
    PRINT 'ERROR: Courses table does not exist. Please create it first.';
    PRINT 'You may need to run CreateDatabase.sql or similar script first.';
    RETURN;
END

-- Step 2: Create Schedules table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Schedules')
BEGIN
    PRINT 'Creating Schedules table...';
    
    CREATE TABLE Schedules (
        Id INT PRIMARY KEY IDENTITY(1,1),
        CourseId INT NOT NULL,
        DayOfWeek NVARCHAR(20) NOT NULL CHECK (DayOfWeek IN ('Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday')),
        StartTime TIME NOT NULL,
        EndTime TIME NOT NULL,
        Room NVARCHAR(50) NULL,
        Building NVARCHAR(100) NULL,
        ClassType NVARCHAR(20) NULL CHECK (ClassType IN ('Lecture', 'Lab', 'Tutorial', 'Seminar', 'Workshop')),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Inactive')),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CONSTRAINT FK_Schedules_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
        CONSTRAINT CK_Schedules_Time CHECK (EndTime > StartTime)
    );
    
    PRINT '✓ Schedules table created successfully!';
END
ELSE
BEGIN
    PRINT 'Schedules table already exists.';
END
GO

-- Step 3: Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Schedules_CourseId' AND object_id = OBJECT_ID('Schedules'))
BEGIN
    CREATE INDEX IX_Schedules_CourseId ON Schedules(CourseId);
    PRINT '✓ Index IX_Schedules_CourseId created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Schedules_DayOfWeek' AND object_id = OBJECT_ID('Schedules'))
BEGIN
    CREATE INDEX IX_Schedules_DayOfWeek ON Schedules(DayOfWeek);
    PRINT '✓ Index IX_Schedules_DayOfWeek created.';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Setup completed successfully!';
PRINT 'You can now use the Schedule features.';
PRINT '========================================';
GO

