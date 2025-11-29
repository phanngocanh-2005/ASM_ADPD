-- ============================================
-- SQL Script to Update Database Schema
-- Make AccountId and AcademicProgramId nullable
-- ============================================

-- 1. Update Students table: Make AcademicProgramId nullable
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND name = 'AcademicProgramId' AND is_nullable = 0)
BEGIN
    ALTER TABLE Students ALTER COLUMN AcademicProgramId INT NULL;
    PRINT 'Updated Students.AcademicProgramId to allow NULL';
END
ELSE
BEGIN
    PRINT 'Students.AcademicProgramId is already nullable';
END
GO

-- 2. Update Students table: Make AccountId nullable (if not already)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND name = 'AccountId' AND is_nullable = 0)
BEGIN
    ALTER TABLE Students ALTER COLUMN AccountId INT NULL;
    PRINT 'Updated Students.AccountId to allow NULL';
END
ELSE
BEGIN
    PRINT 'Students.AccountId is already nullable';
END
GO

-- 3. Update Teachers table: Make AccountId nullable
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Teachers]') AND name = 'AccountId' AND is_nullable = 0)
BEGIN
    -- First, drop the unique constraint if it exists (because NULL values can cause issues with UNIQUE)
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teachers_AccountId' AND object_id = OBJECT_ID(N'[dbo].[Teachers]'))
    BEGIN
        DROP INDEX IX_Teachers_AccountId ON Teachers;
        PRINT 'Dropped unique index IX_Teachers_AccountId';
    END
    
    ALTER TABLE Teachers ALTER COLUMN AccountId INT NULL;
    PRINT 'Updated Teachers.AccountId to allow NULL';
    
    -- Recreate unique index but allow NULL (SQL Server allows multiple NULLs in unique index)
    CREATE UNIQUE NONCLUSTERED INDEX IX_Teachers_AccountId ON Teachers(AccountId) WHERE AccountId IS NOT NULL;
    PRINT 'Recreated unique index IX_Teachers_AccountId (allowing NULL)';
END
ELSE
BEGIN
    PRINT 'Teachers.AccountId is already nullable';
END
GO

-- 4. Update Students table: Make AccountId unique but allow NULL
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_AccountId' AND object_id = OBJECT_ID(N'[dbo].[Students]'))
BEGIN
    DROP INDEX IX_Students_AccountId ON Students;
    PRINT 'Dropped index IX_Students_AccountId';
END

CREATE UNIQUE NONCLUSTERED INDEX IX_Students_AccountId ON Students(AccountId) WHERE AccountId IS NOT NULL;
PRINT 'Created unique index IX_Students_AccountId (allowing NULL)';
GO

PRINT 'Database schema update completed successfully!';
GO

