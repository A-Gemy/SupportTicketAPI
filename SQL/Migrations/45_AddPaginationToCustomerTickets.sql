USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_GetCustomerTickets
    @CustomerId INT,
    @PageNumber INT = 1,
    @PageSize INT = 10
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
            CAST(0 AS INT) AS TotalCount,
            @PageNumber AS PageNumber,
            @PageSize AS PageSize;

        RETURN;
    END

    IF @PageNumber < 1
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Page number must be greater than or equal to 1.' AS Message,
            CAST(0 AS INT) AS TotalCount,
            @PageNumber AS PageNumber,
            @PageSize AS PageSize;

        RETURN;
    END

    IF @PageSize < 1 OR @PageSize > 100
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Page size must be between 1 and 100.' AS Message,
            CAST(0 AS INT) AS TotalCount,
            @PageNumber AS PageNumber,
            @PageSize AS PageSize;

        RETURN;
    END

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @TotalCount INT;

    SELECT
        @TotalCount = COUNT(*)
    FROM Tickets
    WHERE CustomerId = @CustomerId;

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Customer tickets retrieved successfully.' AS Message,
        @TotalCount AS TotalCount,
        @PageNumber AS PageNumber,
        @PageSize AS PageSize;

    SELECT
        t.TicketId,
        t.CustomerId,
        customer.FullName AS CustomerFullName,
        t.AssignedAgentId,
        agent.FullName AS AssignedAgentFullName,
        t.Title,
        t.Description,
        t.Status,
        t.Priority,
        t.CreatedAt,
        t.UpdatedAt,
        t.ClosedAt
    FROM Tickets t
    INNER JOIN Users customer
        ON t.CustomerId = customer.UserId
    LEFT JOIN Users agent
        ON t.AssignedAgentId = agent.UserId
    WHERE t.CustomerId = @CustomerId
    ORDER BY
        t.CreatedAt DESC,
        t.TicketId DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO