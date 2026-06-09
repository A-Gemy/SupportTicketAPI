USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_GetCustomerTicketDetails
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
        'Ticket details retrieved successfully.' AS Message;

    SELECT
        TicketId,
        CustomerId,
        AssignedAgentId,
        Title,
        Description,
        Status,
        Priority,
        CreatedAt,
        UpdatedAt,
        ClosedAt
    FROM Tickets
    WHERE TicketId = @TicketId
      AND CustomerId = @CustomerId;
END
GO