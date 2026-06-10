USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AddCustomerTicketComment
    @CustomerId INT,
    @TicketId INT,
    @CommentText NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM Users
        WHERE UserId = @CustomerId
          AND Role = 'Customer'
          AND IsActive = 1
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Customer not found or inactive.' AS Message,
            CAST(NULL AS INT) AS CommentId;

        RETURN;
    END

    IF NOT EXISTS
    (
        SELECT 1
        FROM Tickets
        WHERE TicketId = @TicketId
          AND CustomerId = @CustomerId
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
          AND CustomerId = @CustomerId
          AND Status = 'Closed'
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Cannot add comment to a closed ticket.' AS Message,
            CAST(NULL AS INT) AS CommentId;

        RETURN;
    END

    IF @CommentText IS NULL OR LTRIM(RTRIM(@CommentText)) = ''
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
        @CustomerId,
        LTRIM(RTRIM(@CommentText)),
        SYSUTCDATETIME()
    );

    UPDATE Tickets
    SET UpdatedAt = SYSUTCDATETIME()
    WHERE TicketId = @TicketId;

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Comment added successfully.' AS Message,
        CAST(SCOPE_IDENTITY() AS INT) AS CommentId;
END
GO