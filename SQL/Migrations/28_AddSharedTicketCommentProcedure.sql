USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AddTicketComment
    @UserId INT,
    @TicketId INT,
    @CommentText NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM Users
        WHERE UserId = @UserId
          AND IsActive = 1
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'User not found or inactive.' AS Message,
            CAST(NULL AS INT) AS CommentId;

        RETURN;
    END

    IF NOT EXISTS
    (
        SELECT 1
        FROM Tickets
        WHERE TicketId = @TicketId
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Ticket not found.' AS Message,
            CAST(NULL AS INT) AS CommentId;

        RETURN;
    END

    IF EXISTS
    (
        SELECT 1
        FROM Tickets
        WHERE TicketId = @TicketId
          AND Status = 'Closed'
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Comments cannot be added to a closed ticket.' AS Message,
            CAST(NULL AS INT) AS CommentId;

        RETURN;
    END

    IF NULLIF(LTRIM(RTRIM(@CommentText)), '') IS NULL
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Comment text is required.' AS Message,
            CAST(NULL AS INT) AS CommentId;

        RETURN;
    END

    INSERT INTO TicketComments
    (
        TicketId,
        UserId,
        CommentText,
        CreatedAt
    )
    VALUES
    (
        @TicketId,
        @UserId,
        LTRIM(RTRIM(@CommentText)),
        SYSUTCDATETIME()
    );

    DECLARE @CommentId INT = SCOPE_IDENTITY();

    UPDATE Tickets
    SET UpdatedAt = SYSUTCDATETIME()
    WHERE TicketId = @TicketId;

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Comment added successfully.' AS Message,
        @CommentId AS CommentId;
END
GO