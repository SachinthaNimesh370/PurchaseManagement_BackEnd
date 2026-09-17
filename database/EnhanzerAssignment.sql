-- Enhanzer Assignment SQL Server Script
-- Database: EnhanzerAssignmentDB

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'EnhanzerAssignmentDB')
BEGIN
    CREATE DATABASE EnhanzerAssignmentDB;
END
GO

USE EnhanzerAssignmentDB;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Location_Details')
BEGIN
    CREATE TABLE Location_Details
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Location_Code NVARCHAR(100) NOT NULL,
        Location_Name NVARCHAR(200) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO
