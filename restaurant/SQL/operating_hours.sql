-- OperatingHours table (same schema as the Admin project's AddOperatingHours migration).
-- The Admin site OWNS this table and edits it; the customer site only reads it.
--
-- Only run this on a database that does NOT already have the table (the shared
-- Admin database already has it). Safe to re-run.
-- If this database will later have the Admin migrations applied
-- (dotnet ef database update), apply the migrations first instead of this script.

USE [restaurantDB];
GO

IF OBJECT_ID(N'dbo.OperatingHours', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OperatingHours (
        OperatingHourId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OperatingHours PRIMARY KEY,
        Day             INT          NOT NULL,   -- 0 = Sunday ... 6 = Saturday (System.DayOfWeek)
        OpeningTime     TIME         NULL,
        ClosingTime     TIME         NULL,
        IsClosed        BIT          NOT NULL,
        UpdatedAt       DATETIME2    NOT NULL
    );

    CREATE UNIQUE INDEX IX_OperatingHours_Day ON dbo.OperatingHours (Day);
END
GO

-- OPTIONAL, development only: sample hours (10:00-22:00, every day) so you can
-- try the customer site without running the Admin app. Un-comment to use.
-- In a real setup the Admin > Operating Hours page is the source of truth.
--
-- INSERT INTO dbo.OperatingHours (Day, OpeningTime, ClosingTime, IsClosed, UpdatedAt)
-- SELECT d.Day, '10:00', '22:00', 0, SYSUTCDATETIME()
-- FROM (VALUES (0),(1),(2),(3),(4),(5),(6)) AS d(Day)
-- WHERE NOT EXISTS (SELECT 1 FROM dbo.OperatingHours o WHERE o.Day = d.Day);
-- GO
