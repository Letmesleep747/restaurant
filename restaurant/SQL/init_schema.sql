-- Initialization script for RROMS (Dine & Dash)
-- Creates Identity tables (minimal schema) and domain tables for reservations, menu, orders
-- WARNING: Run in a development environment. Review before running in production.

-- Optional: create database if it does not exist. Adjust the database name as needed.
IF DB_ID(N'restaurantDB') IS NULL
BEGIN
	PRINT 'Creating database restaurantDB...';
	CREATE DATABASE [restaurantDB];
END
GO

USE [restaurantDB];
GO

-- ==========================
-- ASP.NET Identity baseline
-- (simplified copy of default Identity schema)
-- ==========================

IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.AspNetRoles (
		Id NVARCHAR(450) NOT NULL PRIMARY KEY,
		Name NVARCHAR(256) NULL,
		NormalizedName NVARCHAR(256) NULL,
		ConcurrencyStamp NVARCHAR(MAX) NULL
	);
END
GO

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.AspNetUsers (
		Id NVARCHAR(450) NOT NULL PRIMARY KEY,
		UserName NVARCHAR(256) NULL,
		NormalizedUserName NVARCHAR(256) NULL,
		Email NVARCHAR(256) NULL,
		NormalizedEmail NVARCHAR(256) NULL,
		EmailConfirmed BIT NOT NULL DEFAULT 0,
		PasswordHash NVARCHAR(MAX) NULL,
		SecurityStamp NVARCHAR(MAX) NULL,
		ConcurrencyStamp NVARCHAR(MAX) NULL,
		PhoneNumber NVARCHAR(MAX) NULL,
		PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
		TwoFactorEnabled BIT NOT NULL DEFAULT 0,
		LockoutEnd DATETIMEOFFSET NULL,
		LockoutEnabled BIT NOT NULL DEFAULT 0,
		AccessFailedCount INT NOT NULL DEFAULT 0,
		-- Application-specific fields
		FullName NVARCHAR(200) NULL
	);
END
GO

IF OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.AspNetRoleClaims (
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		RoleId NVARCHAR(450) NOT NULL,
		ClaimType NVARCHAR(MAX) NULL,
		ClaimValue NVARCHAR(MAX) NULL,
		CONSTRAINT FK_AspNetRoleClaims_Role FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
	);
END
GO

IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.AspNetUserClaims (
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		UserId NVARCHAR(450) NOT NULL,
		ClaimType NVARCHAR(MAX) NULL,
		ClaimValue NVARCHAR(MAX) NULL,
		CONSTRAINT FK_AspNetUserClaims_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
	);
END
GO

IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.AspNetUserLogins (
		LoginProvider NVARCHAR(128) NOT NULL,
		ProviderKey NVARCHAR(128) NOT NULL,
		ProviderDisplayName NVARCHAR(128) NULL,
		UserId NVARCHAR(450) NOT NULL,
		CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
		CONSTRAINT FK_AspNetUserLogins_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
	);
END
GO

IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.AspNetUserTokens (
		UserId NVARCHAR(450) NOT NULL,
		LoginProvider NVARCHAR(128) NOT NULL,
		Name NVARCHAR(128) NOT NULL,
		Value NVARCHAR(MAX) NULL,
		CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
		CONSTRAINT FK_AspNetUserTokens_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
	);
END
GO

IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.AspNetUserRoles (
		UserId NVARCHAR(450) NOT NULL,
		RoleId NVARCHAR(450) NOT NULL,
		CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
		CONSTRAINT FK_AspNetUserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
		CONSTRAINT FK_AspNetUserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
	);
END
GO

-- ==========================
-- Domain tables for RROMS
-- ==========================

IF OBJECT_ID(N'dbo.DiningAreas', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.DiningAreas (
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		Name NVARCHAR(100) NOT NULL,
		Description NVARCHAR(500) NULL
	);
END
GO

IF OBJECT_ID(N'dbo.RestaurantTables', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.RestaurantTables (
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		DiningAreaId INT NULL,
		Code NVARCHAR(50) NULL,
		Seats INT NOT NULL DEFAULT 2,
		IsAvailable BIT NOT NULL DEFAULT 1,
		CONSTRAINT FK_RestaurantTables_DiningArea FOREIGN KEY (DiningAreaId) REFERENCES dbo.DiningAreas(Id) ON DELETE SET NULL
	);
END
GO

IF OBJECT_ID(N'dbo.MenuCategories', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.MenuCategories (
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		Name NVARCHAR(100) NOT NULL,
		Description NVARCHAR(500) NULL
	);
END
GO

IF OBJECT_ID(N'dbo.MenuItems', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.MenuItems (
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		CategoryId INT NULL,
		Name NVARCHAR(200) NOT NULL,
		Description NVARCHAR(1000) NULL,
		Price DECIMAL(10,2) NOT NULL,
		IsAvailable BIT NOT NULL DEFAULT 1,
		ImageUrl NVARCHAR(1000) NULL,
		CONSTRAINT FK_MenuItems_Category FOREIGN KEY (CategoryId) REFERENCES dbo.MenuCategories(Id) ON DELETE SET NULL
	);
END
GO

IF OBJECT_ID(N'dbo.Reservations', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.Reservations (
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		ApplicationUserId NVARCHAR(450) NULL,
		TableId INT NULL,
		StartTime DATETIME2 NOT NULL,
		EndTime DATETIME2 NULL,
		Guests INT NOT NULL DEFAULT 1,
		Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
		Notes NVARCHAR(1000) NULL,
		CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
		CONSTRAINT FK_Reservations_User FOREIGN KEY (ApplicationUserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE SET NULL,
		CONSTRAINT FK_Reservations_Table FOREIGN KEY (TableId) REFERENCES dbo.RestaurantTables(Id) ON DELETE SET NULL
	);
END
GO

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.Orders (
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		ApplicationUserId NVARCHAR(450) NULL,
		ReservationId INT NULL,
		CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
		Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
		Total DECIMAL(10,2) NOT NULL DEFAULT 0,
		CONSTRAINT FK_Orders_User FOREIGN KEY (ApplicationUserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE SET NULL,
		CONSTRAINT FK_Orders_Reservation FOREIGN KEY (ReservationId) REFERENCES dbo.Reservations(Id) ON DELETE SET NULL
	);
END
GO

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.OrderItems (
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		OrderId INT NOT NULL,
		MenuItemId INT NOT NULL,
		Quantity INT NOT NULL DEFAULT 1,
		UnitPrice DECIMAL(10,2) NOT NULL,
		CONSTRAINT FK_OrderItems_Order FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
		CONSTRAINT FK_OrderItems_MenuItem FOREIGN KEY (MenuItemId) REFERENCES dbo.MenuItems(Id) ON DELETE NO ACTION
	);
END
GO

IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.Payments (
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		OrderId INT NULL,
		Amount DECIMAL(10,2) NOT NULL,
		PaymentMethod NVARCHAR(50) NULL,
		Status NVARCHAR(50) NULL,
		TransactionReference NVARCHAR(200) NULL,
		CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
		CONSTRAINT FK_Payments_Order FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE SET NULL
	);
END
GO

IF OBJECT_ID(N'dbo.Reviews', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.Reviews (
		Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		ApplicationUserId NVARCHAR(450) NULL,
		OrderId INT NULL,
		Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
		Comments NVARCHAR(2000) NULL,
		CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
		CONSTRAINT FK_Reviews_User FOREIGN KEY (ApplicationUserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE SET NULL,
		CONSTRAINT FK_Reviews_Order FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE SET NULL
	);
END
GO

-- Indexes for common lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_AspNetUsers_NormalizedUserName' AND object_id = OBJECT_ID(N'dbo.AspNetUsers'))
	CREATE UNIQUE INDEX IX_AspNetUsers_NormalizedUserName ON dbo.AspNetUsers(NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_AspNetRoles_NormalizedName' AND object_id = OBJECT_ID(N'dbo.AspNetRoles'))
	CREATE UNIQUE INDEX IX_AspNetRoles_NormalizedName ON dbo.AspNetRoles(NormalizedName) WHERE NormalizedName IS NOT NULL;

PRINT 'Schema creation script completed.';
GO
