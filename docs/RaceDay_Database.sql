-- =============================================
-- RaceDay Database
-- Part 1 - Database Script
-- =============================================

CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

-- =============================================
-- USERS
-- =============================================

CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- =============================================
-- EVENTS
-- =============================================

CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    EventDate DATETIME2 NOT NULL,
    Location NVARCHAR(150) NOT NULL,
    EventType NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Events_Users
        FOREIGN KEY (OrganiserID)
        REFERENCES Users(UserID)
);
GO

-- =============================================
-- CATEGORIES
-- =============================================

CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    MaxParticipants INT NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID)
);
GO

-- =============================================
-- ROUTES
-- =============================================

CREATE TABLE Routes
(
    RouteID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    RouteName NVARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    Description NVARCHAR(500) NULL,

    CONSTRAINT FK_Routes_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID)
);
GO

-- =============================================
-- ENROLMENTS
-- =============================================

CREATE TABLE Enrolments
(
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(30) NOT NULL,

    CONSTRAINT FK_Enrolments_Users
        FOREIGN KEY (UserID)
        REFERENCES Users(UserID),

    CONSTRAINT FK_Enrolments_Categories
        FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID)
);
GO

-- =============================================
-- RESULTS
-- =============================================

CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME NULL,
    Position INT NULL,
    ResultStatus NVARCHAR(30) NOT NULL,
    RecordedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Results_Enrolments
        FOREIGN KEY (EnrolmentID)
        REFERENCES Enrolments(EnrolmentID)
);
GO

-- =============================================
-- SEED DATA: USERS
-- Minimum required:
-- 2 Organisers
-- 2 Participants
-- =============================================

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
    'Thabo',
    'Mokoena',
    'thabo.mokoena@raceday.co.za',
    'TEMP_HASH_001',
    'Organiser'
),
(
    'Nomsa',
    'Dlamini',
    'nomsa.dlamini@raceday.co.za',
    'TEMP_HASH_002',
    'Organiser'
),
(
    'Sipho',
    'Nkosi',
    'sipho.nkosi@example.com',
    'TEMP_HASH_003',
    'Participant'
),
(
    'Lerato',
    'Maseko',
    'lerato.maseko@example.com',
    'TEMP_HASH_004',
    'Participant'
);
GO

-- =============================================
-- SEED DATA: EVENTS
-- 3 events
-- =============================================

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
    'Johannesburg City Run',
    'A road running event through Johannesburg.',
    '2026-10-10 07:00:00',
    'Johannesburg',
    'Running'
),
(
    1,
    'Soweto Community Walk',
    'A community walking event in Soweto.',
    '2026-10-24 08:00:00',
    'Soweto',
    'Walking'
),
(
    2,
    'Pretoria Cycle Challenge',
    'A road cycling challenge around Pretoria.',
    '2026-11-07 06:30:00',
    'Pretoria',
    'Cycling'
);
GO

-- =============================================
-- SEED DATA: CATEGORIES
-- Categories for each event
-- =============================================

INSERT INTO Categories
(
    EventID,
    CategoryName,
    DistanceKm,
    MaxParticipants,
    EntryFee
)
VALUES
-- Johannesburg City Run
(1, '5 KM Run', 5.00, 500, 80.00),
(1, '10 KM Run', 10.00, 500, 120.00),

-- Soweto Community Walk
(2, '5 KM Walk', 5.00, 400, 50.00),
(2, '10 KM Walk', 10.00, 400, 75.00),

-- Pretoria Cycle Challenge
(3, '20 KM Cycle', 20.00, 300, 150.00),
(3, '50 KM Cycle', 50.00, 300, 250.00);
GO

-- =============================================
-- SEED DATA: ROUTES
-- =============================================

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
    'Johannesburg Central Route',
    10.00,
    'Road route through central Johannesburg.'
),
(
    2,
    'Soweto Heritage Route',
    10.00,
    'Walking route through selected Soweto landmarks.'
),
(
    3,
    'Pretoria City Cycle Route',
    50.00,
    'Road cycling route around Pretoria.'
);
GO

-- =============================================
-- SEED DATA: ENROLMENTS
-- Participants:
-- Sipho = UserID 3
-- Lerato = UserID 4
-- =============================================

INSERT INTO Enrolments
(
    UserID,
    CategoryID,
    Status
)
VALUES
(
    3,
    1,
    'Confirmed'
),
(
    3,
    3,
    'Confirmed'
),
(
    4,
    2,
    'Confirmed'
),
(
    4,
    5,
    'Confirmed'
);
GO

-- =============================================
-- SEED DATA: RESULTS
-- =============================================

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
    '00:28:35',
    15,
    'Official'
),
(
    2,
    '00:42:10',
    22,
    'Official'
);
GO

-- =============================================
-- VERIFICATION
-- =============================================

SELECT * FROM Users;
SELECT * FROM Events;
SELECT * FROM Categories;
SELECT * FROM Routes;
SELECT * FROM Enrolments;
SELECT * FROM Results;
GO