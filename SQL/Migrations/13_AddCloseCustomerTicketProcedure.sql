USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_CloseCustomerTicket
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
            'Ticket is already closed.' AS Message;

        RETURN;
    END

    UPDATE Tickets
    SET
        Status = 'Closed',
        UpdatedAt = SYSUTCDATETIME(),
        ClosedAt = SYSUTCDATETIME()
    WHERE TicketId = @TicketId
      AND CustomerId = @CustomerId;

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Ticket closed successfully.' AS Message;
END
GO