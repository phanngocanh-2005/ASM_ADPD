-- ============================================
-- Script to Check Database Schema
-- Chạy script này để xem các bảng và cột hiện có trong database
-- ============================================

PRINT '========================================';
PRINT 'CHECKING DATABASE SCHEMA';
PRINT '========================================';
PRINT '';

-- ============================================
-- 1. List all tables
-- ============================================
PRINT '1. LIST OF ALL TABLES:';
PRINT '----------------------------------------';
SELECT 
    TABLE_NAME AS 'Table Name',
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS 'Column Count'
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
PRINT '';

-- ============================================
-- 2. Check Account table
-- ============================================
PRINT '2. ACCOUNT TABLE STRUCTURE:';
PRINT '----------------------------------------';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Account')
BEGIN
    SELECT 
        COLUMN_NAME AS 'Column Name',
        DATA_TYPE AS 'Data Type',
        IS_NULLABLE AS 'Nullable',
        COLUMN_DEFAULT AS 'Default Value'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Account'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT 'Account table does NOT exist!';
END
PRINT '';

-- ============================================
-- 3. Check Students table
-- ============================================
PRINT '3. STUDENTS TABLE STRUCTURE:';
PRINT '----------------------------------------';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
    SELECT 
        COLUMN_NAME AS 'Column Name',
        DATA_TYPE AS 'Data Type',
        IS_NULLABLE AS 'Nullable',
        COLUMN_DEFAULT AS 'Default Value'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Students'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT 'Students table does NOT exist!';
END
PRINT '';

-- ============================================
-- 4. Check Teachers table
-- ============================================
PRINT '4. TEACHERS TABLE STRUCTURE:';
PRINT '----------------------------------------';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Teachers')
BEGIN
    SELECT 
        COLUMN_NAME AS 'Column Name',
        DATA_TYPE AS 'Data Type',
        IS_NULLABLE AS 'Nullable',
        COLUMN_DEFAULT AS 'Default Value'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Teachers'
    ORDER BY ORDINAL_POSITION;
    
    -- Check specifically for AcademicProgramId
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Teachers]') AND name = 'AcademicProgramId')
    BEGIN
        PRINT '✓ AcademicProgramId column EXISTS in Teachers table';
    END
    ELSE
    BEGIN
        PRINT '✗ AcademicProgramId column does NOT exist in Teachers table';
    END
END
ELSE
BEGIN
    PRINT 'Teachers table does NOT exist!';
END
PRINT '';

-- ============================================
-- 5. Check AcademicPrograms table
-- ============================================
PRINT '5. ACADEMIC PROGRAMS TABLE STRUCTURE:';
PRINT '----------------------------------------';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicPrograms')
BEGIN
    SELECT 
        COLUMN_NAME AS 'Column Name',
        DATA_TYPE AS 'Data Type',
        IS_NULLABLE AS 'Nullable',
        COLUMN_DEFAULT AS 'Default Value'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AcademicPrograms'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT 'AcademicPrograms table does NOT exist!';
END
PRINT '';

-- ============================================
-- 6. Check Courses table
-- ============================================
PRINT '6. COURSES TABLE:';
PRINT '----------------------------------------';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
BEGIN
    PRINT '✓ Courses table EXISTS';
    SELECT COUNT(*) AS 'Row Count' FROM Courses;
END
ELSE
BEGIN
    PRINT '✗ Courses table does NOT exist!';
END
PRINT '';

-- ============================================
-- 7. Check Enrollments table
-- ============================================
PRINT '7. ENROLLMENTS TABLE:';
PRINT '----------------------------------------';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
BEGIN
    PRINT '✓ Enrollments table EXISTS';
    SELECT COUNT(*) AS 'Row Count' FROM Enrollments;
END
ELSE
BEGIN
    PRINT '✗ Enrollments table does NOT exist!';
END
PRINT '';

-- ============================================
-- 8. Check CourseAssignments table
-- ============================================
PRINT '8. COURSE ASSIGNMENTS TABLE:';
PRINT '----------------------------------------';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseAssignments')
BEGIN
    PRINT '✓ CourseAssignments table EXISTS';
    SELECT COUNT(*) AS 'Row Count' FROM CourseAssignments;
END
ELSE
BEGIN
    PRINT '✗ CourseAssignments table does NOT exist!';
END
PRINT '';

-- ============================================
-- 9. Check AcademicRecords table
-- ============================================
PRINT '9. ACADEMIC RECORDS TABLE:';
PRINT '----------------------------------------';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
BEGIN
    PRINT '✓ AcademicRecords table EXISTS';
    SELECT COUNT(*) AS 'Row Count' FROM AcademicRecords;
END
ELSE
BEGIN
    PRINT '✗ AcademicRecords table does NOT exist!';
END
PRINT '';

-- ============================================
-- 10. Check Foreign Keys
-- ============================================
PRINT '10. FOREIGN KEY CONSTRAINTS:';
PRINT '----------------------------------------';
SELECT 
    fk.name AS 'Foreign Key Name',
    OBJECT_NAME(fk.parent_object_id) AS 'Parent Table',
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS 'Parent Column',
    OBJECT_NAME(fk.referenced_object_id) AS 'Referenced Table',
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS 'Referenced Column'
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fc ON fk.object_id = fc.constraint_object_id
ORDER BY OBJECT_NAME(fk.parent_object_id), fk.name;
PRINT '';

-- ============================================
-- 11. Summary
-- ============================================
PRINT '========================================';
PRINT 'SUMMARY:';
PRINT '========================================';

DECLARE @MissingTables NVARCHAR(MAX) = '';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Account')
    SET @MissingTables = @MissingTables + 'Account, ';
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicPrograms')
    SET @MissingTables = @MissingTables + 'AcademicPrograms, ';
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
    SET @MissingTables = @MissingTables + 'Students, ';
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Teachers')
    SET @MissingTables = @MissingTables + 'Teachers, ';
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
    SET @MissingTables = @MissingTables + 'Courses, ';
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
    SET @MissingTables = @MissingTables + 'Enrollments, ';
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseAssignments')
    SET @MissingTables = @MissingTables + 'CourseAssignments, ';
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
    SET @MissingTables = @MissingTables + 'AcademicRecords, ';

IF LEN(@MissingTables) > 0
BEGIN
    SET @MissingTables = LEFT(@MissingTables, LEN(@MissingTables) - 1);
    PRINT 'Missing tables: ' + @MissingTables;
    PRINT 'Please run CreateMissingTables.sql to create them.';
END
ELSE
BEGIN
    PRINT 'All required tables exist!';
END

-- Check Teachers.AcademicProgramId
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Teachers')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Teachers]') AND name = 'AcademicProgramId')
    BEGIN
        PRINT 'Teachers table is missing AcademicProgramId column.';
        PRINT 'Please run AddAcademicProgramToTeachers.sql to add it.';
    END
    ELSE
    BEGIN
        PRINT 'Teachers table has AcademicProgramId column.';
    END
END

PRINT '';
PRINT '========================================';
PRINT 'Check completed!';
PRINT '========================================';
GO

