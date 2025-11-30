-- ============================================
-- SQL Script to Insert Default Courses
-- ============================================
-- This script will insert 12 default courses into the system
-- Make sure you have at least one Academic Program in the database

-- First, ensure there's at least one Academic Program
IF NOT EXISTS (SELECT 1 FROM AcademicPrograms WHERE Status = 'Active')
BEGIN
    INSERT INTO AcademicPrograms (ProgramCode, ProgramName, Description, Status, CreatedAt)
    VALUES ('COMP', 'Computing Program', 'Default Computing Program', 'Active', GETDATE());
    PRINT 'Created default Academic Program: COMP';
END
ELSE
BEGIN
    PRINT 'Academic Program already exists';
END

DECLARE @ProgramId INT;
SELECT TOP 1 @ProgramId = Id FROM AcademicPrograms WHERE Status = 'Active';

IF @ProgramId IS NULL
BEGIN
    PRINT 'ERROR: Could not find or create an Academic Program. Please create one manually.';
    RETURN;
END

PRINT 'Using Academic Program ID: ' + CAST(@ProgramId AS VARCHAR(10));

-- Insert all 12 courses
-- 1. Professional Practice
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'PRO001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('PRO001', 'Professional Practice', 'Professional Practice course covering industry standards and professional ethics', @ProgramId, 3, 'Active', GETDATE());
    PRINT 'Inserted: PRO001 - Professional Practice';
END
ELSE
BEGIN
    PRINT 'Course PRO001 already exists';
END

-- 2. Planning a Computing Project
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'PCP001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('PCP001', 'Planning a Computing Project', 'Course on project planning methodologies and tools for computing projects', @ProgramId, 3, 'Active', GETDATE());
    PRINT 'Inserted: PCP001 - Planning a Computing Project';
END
ELSE
BEGIN
    PRINT 'Course PCP001 already exists';
END

-- 3. Database Design & Development
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'DDD001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('DDD001', 'Database Design & Development', 'Comprehensive course on database design principles and development practices', @ProgramId, 4, 'Active', GETDATE());
    PRINT 'Inserted: DDD001 - Database Design & Development';
END
ELSE
BEGIN
    PRINT 'Course DDD001 already exists';
END

-- 4. Data Structures & Algorithms
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'DSA001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('DSA001', 'Data Structures & Algorithms', 'Study of fundamental data structures and algorithm design and analysis', @ProgramId, 4, 'Active', GETDATE());
    PRINT 'Inserted: DSA001 - Data Structures & Algorithms';
END
ELSE
BEGIN
    PRINT 'Course DSA001 already exists';
END

-- 5. Internet of Things
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'IOT001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('IOT001', 'Internet of Things', 'Introduction to IoT concepts, devices, and applications', @ProgramId, 3, 'Active', GETDATE());
    PRINT 'Inserted: IOT001 - Internet of Things';
END
ELSE
BEGIN
    PRINT 'Course IOT001 already exists';
END

-- 6. Security
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'SEC001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('SEC001', 'Security', 'Cybersecurity fundamentals, threats, and protection mechanisms', @ProgramId, 3, 'Active', GETDATE());
    PRINT 'Inserted: SEC001 - Security';
END
ELSE
BEGIN
    PRINT 'Course SEC001 already exists';
END

-- 7. Software Development Life Cycle
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'SDLC001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('SDLC001', 'Software Development Life Cycle', 'Comprehensive study of SDLC methodologies and best practices', @ProgramId, 3, 'Active', GETDATE());
    PRINT 'Inserted: SDLC001 - Software Development Life Cycle';
END
ELSE
BEGIN
    PRINT 'Course SDLC001 already exists';
END

-- 8. Website Design & Development
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'WDD001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('WDD001', 'Website Design & Development', 'Web development technologies, design principles, and modern frameworks', @ProgramId, 4, 'Active', GETDATE());
    PRINT 'Inserted: WDD001 - Website Design & Development';
END
ELSE
BEGIN
    PRINT 'Course WDD001 already exists';
END

-- 9. Business Process Support
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'BPS001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('BPS001', 'Business Process Support', 'Understanding and supporting business processes through technology', @ProgramId, 3, 'Active', GETDATE());
    PRINT 'Inserted: BPS001 - Business Process Support';
END
ELSE
BEGIN
    PRINT 'Course BPS001 already exists';
END

-- 10. Applied Programming and Design Principles
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'APD001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('APD001', 'Applied Programming and Design Principles', 'Practical programming skills and software design principles', @ProgramId, 4, 'Active', GETDATE());
    PRINT 'Inserted: APD001 - Applied Programming and Design Principles';
END
ELSE
BEGIN
    PRINT 'Course APD001 already exists';
END

-- 11. Application Development
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'APP001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('APP001', 'Application Development', 'Development of desktop and mobile applications using modern technologies', @ProgramId, 4, 'Active', GETDATE());
    PRINT 'Inserted: APP001 - Application Development';
END
ELSE
BEGIN
    PRINT 'Course APP001 already exists';
END

-- 12. Discrete Maths
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'DM001')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, AcademicProgramId, Credits, Status, CreatedAt)
    VALUES ('DM001', 'Discrete Maths', 'Mathematical foundations for computer science including logic, sets, and graph theory', @ProgramId, 3, 'Active', GETDATE());
    PRINT 'Inserted: DM001 - Discrete Maths';
END
ELSE
BEGIN
    PRINT 'Course DM001 already exists';
END

PRINT '============================================';
PRINT 'Course insertion completed!';
PRINT '============================================';

