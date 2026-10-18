
USE RaceDayPart2;
GO

SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @UserId INT = 1;
    DECLARE @EventId INT;
    DECLARE @CategoryId INT;

    -- Confirm that the registered user exists.
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Users
        WHERE UserId = @UserId
    )
    BEGIN
        THROW 50001, 'The selected user does not exist.', 1;
    END;

    -- Reuse the test event if it already exists.
    SELECT @EventId = EventId
    FROM dbo.Events
    WHERE EventName = 'RaceDay Test Run'
      AND OrganiserId = @UserId;

    IF @EventId IS NULL
    BEGIN
        INSERT INTO dbo.Events
            (OrganiserId, EventName, Description, EventDate,
             Location, EventType, CreatedAt)
        VALUES
            (@UserId, 'RaceDay Test Run',
             'Test event for API enrolment testing.',
             DATEADD(DAY, 30, GETDATE()),
             'Johannesburg, South Africa',
             'Running', GETUTCDATE());

        SET @EventId = CONVERT(INT, SCOPE_IDENTITY());
    END;

    -- Reuse the test category if it already exists.
    SELECT @CategoryId = CategoryId
    FROM dbo.Categories
    WHERE EventId = @EventId
      AND CategoryName = '5 km Fun Run';

    IF @CategoryId IS NULL
    BEGIN
        INSERT INTO dbo.Categories
            (EventId, CategoryName, DistanceKm,
             MaxParticipants, EntryFee)
        VALUES
            (@EventId, '5 km Fun Run', 5.00, 100, 50.00);

        SET @CategoryId = CONVERT(INT, SCOPE_IDENTITY());
    END;

    -- Create the enrolment only if one does not already exist.
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Enrolments
        WHERE UserId = @UserId
          AND CategoryId = @CategoryId
    )
    BEGIN
        INSERT INTO dbo.Enrolments
            (UserId, CategoryId, EnrolmentDate, Status)
        VALUES
            (@UserId, @CategoryId, GETUTCDATE(), 'Confirmed');
    END;

    COMMIT TRANSACTION;

    -- Display the created test enrolment.
    SELECT
        en.EnrolmentId,
        en.UserId,
        u.FirstName,
        u.LastName,
        en.CategoryId,
        c.CategoryName,
        e.EventName,
        e.EventDate,
        en.Status
    FROM dbo.Enrolments AS en
    INNER JOIN dbo.Users AS u
        ON u.UserId = en.UserId
    INNER JOIN dbo.Categories AS c
        ON c.CategoryId = en.CategoryId
    INNER JOIN dbo.Events AS e
        ON e.EventId = c.EventId
    WHERE en.UserId = @UserId
      AND c.CategoryId = @CategoryId;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
