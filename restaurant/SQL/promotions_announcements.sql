-- Promotions + Announcements tables (same schema as the Admin project's migrations).
-- Only run this on a database that does NOT already have these tables
-- (the shared Azure/Admin database already has them). Safe to re-run.

USE [restaurantDB];
GO

IF OBJECT_ID(N'dbo.Promotions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Promotions (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Promotions PRIMARY KEY,
        Name          NVARCHAR(100)  NOT NULL,
        Description   NVARCHAR(500)  NULL,
        DiscountType  NVARCHAR(20)   NOT NULL,
        DiscountValue DECIMAL(10,2)  NOT NULL,
        StartDate     DATETIME2      NOT NULL,
        EndDate       DATETIME2      NOT NULL,
        IsActive      BIT            NOT NULL,
        CreatedAt     DATETIME2      NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Announcements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Announcements (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Announcements PRIMARY KEY,
        Title       NVARCHAR(150)  NOT NULL,
        Message     NVARCHAR(2000) NOT NULL,
        StartDate   DATETIME2      NULL,
        EndDate     DATETIME2      NULL,
        IsPublished BIT            NOT NULL,
        IsActive    BIT            NOT NULL,
        CreatedAt   DATETIME2      NOT NULL,
        UpdatedAt   DATETIME2      NULL
    );
END
GO
