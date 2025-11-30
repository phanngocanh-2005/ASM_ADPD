-- ============================================
-- Script to Create Schedules Table (if missing)
-- This script will create the Schedules table if it doesn't exist
-- ============================================

-- Check if Courses table exists first (required for foreign key)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
BEGIN
    PRINT 'ERROR: Courses table does not exist. Please create it first.';
    RETURN;
END

-- Create Schedules table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Schedules')
BEGIN
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
    
    PRINT 'Successfully created table Schedules';
END
ELSE
BEGIN
    PRINT 'Table Schedules already exists';
END
GO

-- Create index for better query performance (CourseId)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Schedules_CourseId' AND object_id = OBJECT_ID('Schedules'))
BEGIN
    CREATE INDEX IX_Schedules_CourseId ON Schedules(CourseId);
    PRINT 'Created index IX_Schedules_CourseId';
END
ELSE
BEGIN
    PRINT 'Index IX_Schedules_CourseId already exists';
END
GO

-- Create index for DayOfWeek
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Schedules_DayOfWeek' AND object_id = OBJECT_ID('Schedules'))
BEGIN
    CREATE INDEX IX_Schedules_DayOfWeek ON Schedules(DayOfWeek);
    PRINT 'Created index IX_Schedules_DayOfWeek';
END
ELSE
BEGIN
    PRINT 'Index IX_Schedules_DayOfWeek already exists';
END
GO

PRINT '========================================';
PRINT 'Schedules table setup completed!';
PRINT '========================================';
GO

