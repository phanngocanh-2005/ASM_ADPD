-- ============================================
-- Add AcademicProgramId to Teachers table
-- Run this script if your database already exists
-- ============================================

-- Check if column already exists before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Teachers]') AND name = 'AcademicProgramId')
BEGIN
    -- Add AcademicProgramId column
    ALTER TABLE Teachers ADD AcademicProgramId INT NULL;
    PRINT 'Added AcademicProgramId column to Teachers table';
END
ELSE
BEGIN
    PRINT 'AcademicProgramId column already exists in Teachers table';
END
GO

-- Add foreign key constraint if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Teachers_AcademicProgram')
BEGIN
    ALTER TABLE Teachers 
    ADD CONSTRAINT FK_Teachers_AcademicProgram 
    FOREIGN KEY (AcademicProgramId) REFERENCES AcademicPrograms(Id);
    PRINT 'Added foreign key constraint FK_Teachers_AcademicProgram';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_Teachers_AcademicProgram already exists';
END
GO

-- Add index if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teachers_AcademicProgramId' AND object_id = OBJECT_ID(N'[dbo].[Teachers]'))
BEGIN
    CREATE INDEX IX_Teachers_AcademicProgramId ON Teachers(AcademicProgramId);
    PRINT 'Added index IX_Teachers_AcademicProgramId';
END
ELSE
BEGIN
    PRINT 'Index IX_Teachers_AcademicProgramId already exists';
END
GO

PRINT 'Script completed successfully!';
GO

