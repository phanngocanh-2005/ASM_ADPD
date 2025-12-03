-- Add optional StudentId column to Schedules table so that a schedule
-- can be assigned directly to an individual student.
-- Run this script once on your AuthApp database.

IF COL_LENGTH('Schedules', 'StudentId') IS NULL
BEGIN
    ALTER TABLE Schedules
    ADD StudentId INT NULL;

    ALTER TABLE Schedules
    ADD CONSTRAINT FK_Schedules_Students_StudentId
        FOREIGN KEY (StudentId) REFERENCES Students(Id)
        ON DELETE SET NULL;

    CREATE INDEX IX_Schedules_StudentId
        ON Schedules(StudentId);
END
GO


