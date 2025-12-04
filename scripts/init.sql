-- Initialize eservice_new database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'eservice_new')
BEGIN
    CREATE DATABASE eservice_new;
END
GO

USE eservice_new;
GO

-- Database is now ready for the application
PRINT 'Database eservice_new created successfully';
GO
