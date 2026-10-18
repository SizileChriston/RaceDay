USE RaceDayPart2;
GO

SELECT
    UserId,
    FirstName,
    LastName,
    Email,
    Role
FROM dbo.Users;
SELECT
    e.EventId,
    e.EventName,
    e.OrganiserId,
    u.Email,
    u.Role
FROM dbo.Events AS e
LEFT JOIN dbo.Users AS u
    ON e.OrganiserId = u.UserId;

    USE RaceDayPart2;
GO

SELECT
    UserId,
    FirstName,
    LastName,
    Email,
    Role,
    CreatedAt
FROM dbo.Users
ORDER BY UserId;

USE RaceDayPart2;
GO

SELECT UserId, FirstName, LastName, Email, Role
FROM dbo.Users
ORDER BY UserId;



USE RaceDayPart2;
GO

UPDATE dbo.Users
SET Role = 'Organiser'
WHERE UserId = 2
  AND Email = 'organiser.test@raceday.co.za';

SELECT UserId, FirstName, LastName, Email, Role
FROM dbo.Users
WHERE UserId = 2;

USE RaceDayPart2;
GO

UPDATE dbo.Users
SET Role = 'Organiser'
WHERE UserId = 3
  AND Email = 'raceday.organiser.test@raceday.co.za';

SELECT
    UserId,
    FirstName,
    LastName,
    Email,
    Role
FROM dbo.Users
WHERE UserId = 3;

SELECT DB_NAME() AS CurrentDatabase;
GO

SELECT
    TABLE_CATALOG AS DatabaseName,
    TABLE_SCHEMA AS SchemaName,
    TABLE_NAME AS TableName
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'Users';
GO

SELECT name
FROM sys.databases
ORDER BY name;

USE RaceDayPart2;
GO

SELECT DB_NAME() AS CurrentDatabase;

SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'Users';