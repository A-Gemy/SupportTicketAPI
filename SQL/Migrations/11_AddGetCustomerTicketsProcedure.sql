USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_GetCustomerTickets
    @CustomerId INT
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

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Customer tickets retrieved successfully.' AS Message;

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
    WHERE CustomerId = @CustomerId
    ORDER BY CreatedAt DESC;
END
GO