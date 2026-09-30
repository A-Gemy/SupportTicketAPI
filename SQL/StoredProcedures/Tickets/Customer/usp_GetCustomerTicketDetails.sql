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
            CAST('CUSTOMER_NOT_FOUND_OR_INACTIVE' AS VARCHAR(50)) AS ResultCode;

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
            CAST('TICKET_NOT_FOUND' AS VARCHAR(50)) AS ResultCode;

        RETURN;
    END

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        CAST('SUCCESS' AS VARCHAR(50)) AS ResultCode;

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
