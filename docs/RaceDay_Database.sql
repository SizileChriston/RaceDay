-- ============================================================
-- RaceDay Event Management Platform
-- Database Creation, Schema and Seed Data Script
-- Target RDBMS: Microsoft SQL Server / LocalDB
-- Programming 2B - Part 1
-- ============================================================

USE master;
GO

-- ============================================================
-- 1. RESET THE DATABASE
-- Allows the script to be safely re-run during development.
-- ============================================================

IF DB_ID(N'RaceDay') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDay
        SET SINGLE_USER
        WITH ROLLBACK IMMEDIATE;

    DROP DATABASE RaceDay;
END;
GO

-- ============================================================
-- 2. CREATE DATABASE
-- ============================================================

CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

-- ============================================================
-- 3. USERS TABLE
-- Stores Organisers and Participants.
-- ============================================================

CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Users
        PRIMARY KEY (UserID),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);
GO

-- ============================================================
-- 4. EVENTS TABLE
-- Stores events created by Organisers.
-- ============================================================

CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) NOT NULL,
    OrganiserID INT NOT NULL,
    EventName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    EventDate DATETIME2 NOT NULL,
    Location NVARCHAR(150) NOT NULL,
    EventType NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Events
        PRIMARY KEY (EventID),

    CONSTRAINT FK_Events_Users
        FOREIGN KEY (OrganiserID)
        REFERENCES Users(UserID),

    CONSTRAINT CK_Events_EventType
        CHECK (EventType IN ('Running', 'Walking', 'Cycling'))
);
GO

-- ============================================================
-- 5. CATEGORIES TABLE
-- Stores race categories belonging to Events.
-- ============================================================

CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) NOT NULL,
    EventID INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    MaxParticipants INT NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,

    CONSTRAINT PK_Categories
        PRIMARY KEY (CategoryID),

    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID),

    CONSTRAINT CK_Categories_Distance
        CHECK (DistanceKm > 0),

    CONSTRAINT CK_Categories_MaxParticipants
        CHECK (MaxParticipants > 0),

    CONSTRAINT CK_Categories_EntryFee
        CHECK (EntryFee >= 0),

    CONSTRAINT UQ_Categories_Event_Category
        UNIQUE (EventID, CategoryName)
);
GO

-- ============================================================
-- 6. ROUTES TABLE
-- Stores routes associated with Events.
-- ============================================================

CREATE TABLE Routes
(
    RouteID INT IDENTITY(1,1) NOT NULL,
    EventID INT NOT NULL,
    RouteName NVARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    Description NVARCHAR(500) NULL,

    CONSTRAINT PK_Routes
        PRIMARY KEY (RouteID),

    CONSTRAINT FK_Routes_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID),

    CONSTRAINT CK_Routes_Distance
        CHECK (DistanceKm > 0)
);
GO

-- ============================================================
-- 7. ENROLMENTS TABLE
-- Connects Participants to Event Categories.
-- ============================================================

CREATE TABLE Enrolments
(
    EnrolmentID INT IDENTITY(1,1) NOT NULL,
    UserID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(30) NOT NULL,

    CONSTRAINT PK_Enrolments
        PRIMARY KEY (EnrolmentID),

    CONSTRAINT FK_Enrolments_Users
        FOREIGN KEY (UserID)
        REFERENCES Users(UserID),

    CONSTRAINT FK_Enrolments_Categories
        FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID),

    CONSTRAINT CK_Enrolments_Status
        CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),

    CONSTRAINT UQ_Enrolments_User_Category
        UNIQUE (UserID, CategoryID)
);
GO

-- ============================================================
-- 8. RESULTS TABLE
-- Stores participant race results.
-- ============================================================

CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) NOT NULL,
    EnrolmentID INT NOT NULL,
    FinishTime TIME NULL,
    Position INT NULL,
    ResultStatus NVARCHAR(30) NOT NULL,
    RecordedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Results
        PRIMARY KEY (ResultID),

    CONSTRAINT FK_Results_Enrolments
        FOREIGN KEY (EnrolmentID)
        REFERENCES Enrolments(EnrolmentID),

    CONSTRAINT CK_Results_Position
        CHECK (Position IS NULL OR Position > 0),

    CONSTRAINT CK_Results_Status
        CHECK (ResultStatus IN ('Official', 'Pending', 'Disqualified')),

    CONSTRAINT UQ_Results_Enrolment
        UNIQUE (EnrolmentID)
);
GO

-- ============================================================
-- 9. SEED USERS
-- 2 Organisers and 2 Participants
-- ============================================================

INSERT INTO Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    Role
)
VALUES
(
    'Sizile',
    'Christon',
    'organiser.sizile@raceday.co.za',
    'AQAAAAEAACcQAAAAEHashedPass123!',
    'Organiser'
),
(
    'Sipho',
    'Nkosi',
    'organiser.sipho@raceday.co.za',
    'AQAAAAEAACcQAAAAEHashedPass456!',
    'Organiser'
),
(
    'Thabo',
    'Mokoena',
    'thabo.mokoena@gmail.com',
    'AQAAAAEAACcQAAAAEHashedPass789!',
    'Participant'
),
(
    'Anika',
    'Van Zyl',
    'anika.vanzyl@yahoo.com',
    'AQAAAAEAACcQAAAAEHashedPass012!',
    'Participant'
);
GO

-- ============================================================
-- 10. SEED EVENTS
-- 3 South African road sport events
-- ============================================================

INSERT INTO Events
(
    OrganiserID,
    EventName,
    Description,
    EventDate,
    Location,
    EventType
)
VALUES
(
    1,
    'Soweto Marathon & Community Run',
    'Road running event passing through historical landmarks in Soweto.',
    '2026-11-01 06:00:00',
    'Soweto, Johannesburg',
    'Running'
),
(
    1,
    'Cape Town Cycle Challenge',
    'Scenic coastal cycling event around the Cape Peninsula.',
    '2026-10-15 07:00:00',
    'Cape Town, Western Cape',
    'Cycling'
),
(
    2,
    'Durban Promenade Walkathon',
    'Family-friendly walking event along the Durban beachfront promenade.',
    '2026-09-20 08:00:00',
    'Durban, KwaZulu-Natal',
    'Walking'
);
GO

-- ============================================================
-- 11. SEED CATEGORIES
-- Multiple categories for every event
-- ============================================================

INSERT INTO Categories
(
    EventID,
    CategoryName,
    DistanceKm,
    MaxParticipants,
    EntryFee
)
VALUES
(
    1,
    'Full Marathon',
    42.20,
    5000,
    350.00
),
(
    1,
    'Half Marathon',
    21.10,
    8000,
    250.00
),
(
    1,
    '10K Fun Run',
    10.00,
    10000,
    150.00
),
(
    2,
    'Main Cycle Tour',
    109.00,
    3000,
    550.00
),
(
    2,
    'Short Coastal Ride',
    42.00,
    1500,
    300.00
),
(
    3,
    '10K Coastal Walk',
    10.00,
    2000,
    100.00
),
(
    3,
    '5K Family Walk',
    5.00,
    3000,
    50.00
);
GO

-- ============================================================
-- 12. SEED ROUTES
-- One route for each event
-- ============================================================

INSERT INTO Routes
(
    EventID,
    RouteName,
    DistanceKm,
    Description
)
VALUES
(
    1,
    'Soweto Heritage Route',
    21.10,
    'Route passing through selected historical landmarks in Soweto.'
),
(
    2,
    'Cape Peninsula Coastal Route',
    109.00,
    'Scenic cycling route around the Cape Peninsula coastline.'
),
(
    3,
    'Durban Golden Mile Route',
    10.00,
    'Walking route following Durban beachfront and Golden Mile landmarks.'
);
GO

-- ============================================================
-- 13. SEED ENROLMENTS
-- 4 participant registrations
-- ============================================================

INSERT INTO Enrolments
(
    UserID,
    CategoryID,
    Status
)
VALUES
(
    3,
    2,
    'Confirmed'
),
(
    3,
    4,
    'Confirmed'
),
(
    4,
    3,
    'Confirmed'
),
(
    4,
    6,
    'Confirmed'
);
GO

-- ============================================================
-- 14. SEED RESULTS
-- 2 official participant results
-- ============================================================

INSERT INTO Results
(
    EnrolmentID,
    FinishTime,
    Position,
    ResultStatus
)
VALUES
(
    1,
    '01:45:30',
    142,
    'Official'
),
(
    3,
    '00:52:15',
    45,
    'Official'
);
GO

-- ============================================================
-- 15. RECORD COUNT VERIFICATION
-- ============================================================

SELECT
    'Users' AS TableName,
    COUNT(*) AS RecordCount
FROM Users

UNION ALL

SELECT
    'Events',
    COUNT(*)
FROM Events

UNION ALL

SELECT
    'Categories',
    COUNT(*)
FROM Categories

UNION ALL

SELECT
    'Routes',
    COUNT(*)
FROM Routes

UNION ALL

SELECT
    'Enrolments',
    COUNT(*)
FROM Enrolments

UNION ALL

SELECT
    'Results',
    COUNT(*)
FROM Results;
GO

-- ============================================================
-- 16. DISPLAY SEEDED DATA
-- ============================================================

SELECT * FROM Users;
SELECT * FROM Events;
SELECT * FROM Categories;
SELECT * FROM Routes;
SELECT * FROM Enrolments;
SELECT * FROM Results;
GO