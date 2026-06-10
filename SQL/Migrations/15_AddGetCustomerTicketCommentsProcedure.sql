USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_GetCustomerTicketComments
    @CustomerId INT,
    @TicketId INT
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
            'Customer not found or inactive.' AS Message;

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
            'Ticket not found.' AS Message;

        RETURN;
    END

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Ticket comments retrieved successfully.' AS Message;

    SELECT
        tc.CommentId,
        tc.TicketId,
        tc.UserId,
        u.FullName AS UserFullName,
        u.Role AS UserRole,
        tc.CommentText,
        tc.CreatedAt
    FROM TicketComments tc
    INNER JOIN Users u ON tc.UserId = u.UserId
    WHERE tc.TicketId = @TicketId
    ORDER BY tc.CreatedAt ASC;
END
GO