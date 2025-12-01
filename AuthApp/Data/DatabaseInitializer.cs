using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthApp.Data
{
    public static class DatabaseInitializer
    {
        public static void EnsureScheduleSchema(ApplicationDbContext context, ILogger logger)
        {
            const string sql = @"
IF OBJECT_ID('dbo.Schedules', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Schedules (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CourseId INT NOT NULL,
        TeacherId INT NOT NULL,
        DayOfWeek NVARCHAR(20) NOT NULL CHECK (DayOfWeek IN ('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday')),
        StartTime TIME NOT NULL,
        EndTime TIME NOT NULL,
        Room NVARCHAR(50) NULL,
        Building NVARCHAR(100) NULL,
        ClassType NVARCHAR(20) NULL CHECK (ClassType IN ('Lecture','Lab','Tutorial','Seminar','Workshop')),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active','Inactive')),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CONSTRAINT CK_Schedules_Time CHECK (EndTime > StartTime),
        CONSTRAINT FK_Schedules_Course FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Schedules_Teacher FOREIGN KEY (TeacherId) REFERENCES Teachers(Id) ON DELETE CASCADE
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Schedules', 'TeacherId') IS NULL
    BEGIN
        ALTER TABLE dbo.Schedules ADD TeacherId INT NULL;

        UPDATE dbo.Schedules
        SET TeacherId = ISNULL(
            (SELECT TOP 1 TeacherId FROM CourseAssignments WHERE CourseAssignments.CourseId = dbo.Schedules.CourseId ORDER BY AssignmentDate DESC),
            (SELECT TOP 1 Id FROM Teachers ORDER BY Id)
        );

        ALTER TABLE dbo.Schedules ALTER COLUMN TeacherId INT NOT NULL;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Schedules_Course')
    BEGIN
        ALTER TABLE dbo.Schedules
        WITH CHECK ADD CONSTRAINT FK_Schedules_Course FOREIGN KEY (CourseId)
        REFERENCES Courses(Id) ON DELETE CASCADE;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Schedules_Teacher')
    BEGIN
        ALTER TABLE dbo.Schedules
        WITH CHECK ADD CONSTRAINT FK_Schedules_Teacher FOREIGN KEY (TeacherId)
        REFERENCES Teachers(Id) ON DELETE CASCADE;
    END
END

IF OBJECT_ID('dbo.Schedules', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Schedules_CourseId' AND object_id = OBJECT_ID('dbo.Schedules'))
        CREATE INDEX IX_Schedules_CourseId ON dbo.Schedules(CourseId);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Schedules_TeacherId' AND object_id = OBJECT_ID('dbo.Schedules'))
        CREATE INDEX IX_Schedules_TeacherId ON dbo.Schedules(TeacherId);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Schedules_DayOfWeek_StartTime' AND object_id = OBJECT_ID('dbo.Schedules'))
        CREATE INDEX IX_Schedules_DayOfWeek_StartTime ON dbo.Schedules(DayOfWeek, StartTime);
END";

            try
            {
                context.Database.ExecuteSqlRaw(sql);
                logger.LogInformation("Verified Schedules table schema.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to ensure Schedules table schema. Please run the SQL script manually.");
                throw;
            }
        }
    }
}

