USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AdminUpdateTicketStatus
    @AdminId INT,
    @TicketId INT,
    @Status NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM Users
        WHERE UserId = @AdminId
          AND Role = 'Admin'
          AND IsActive = 1
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Admin not found or inactive.' AS Message;

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
            'Ticket not found.' AS Message;

        RETURN;
    END

    IF @Status NOT IN ('Open', 'InProgress', 'Resolved', 'Closed')
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Invalid ticket status.' AS Message;

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
            'Closed tickets cannot be updated.' AS Message;

        RETURN;
    END

    UPDATE Tickets
    SET
        Status = @Status,
        UpdatedAt = SYSUTCDATETIME(),
        ClosedAt =
            CASE
                WHEN @Status = 'Closed' THEN SYSUTCDATETIME()
                ELSE ClosedAt
            END
    WHERE TicketId = @TicketId;

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Ticket status updated successfully.' AS Message;
END
GO