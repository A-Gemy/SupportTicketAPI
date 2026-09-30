USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AdminGetUnassignedTickets
    @AdminId INT,
    @PageNumber INT = 1,
    @PageSize INT = 10
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
            CAST('ADMIN_NOT_FOUND_OR_INACTIVE' AS VARCHAR(50)) AS ResultCode,
            CAST(0 AS INT) AS TotalCount,
            @PageNumber AS PageNumber,
            @PageSize AS PageSize;

        RETURN;
    END

    IF @PageNumber < 1
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            CAST('INVALID_PAGE_NUMBER' AS VARCHAR(50)) AS ResultCode,
            CAST(0 AS INT) AS TotalCount,
            @PageNumber AS PageNumber,
            @PageSize AS PageSize;

        RETURN;
    END

    IF @PageSize < 1 OR @PageSize > 100
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            CAST('INVALID_PAGE_SIZE' AS VARCHAR(50)) AS ResultCode,
            CAST(0 AS INT) AS TotalCount,
            @PageNumber AS PageNumber,
            @PageSize AS PageSize;

        RETURN;
    END

    DECLARE @Offset BIGINT =
        (CAST(@PageNumber AS BIGINT) - 1) *
        CAST(@PageSize AS BIGINT);
    DECLARE @TotalCount INT;

    SELECT
        @TotalCount = COUNT(*)
    FROM Tickets
    WHERE AssignedAgentId IS NULL
      AND Status <> 'Closed';

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        CAST('SUCCESS' AS VARCHAR(50)) AS ResultCode,
        @TotalCount AS TotalCount,
        @PageNumber AS PageNumber,
        @PageSize AS PageSize;

    SELECT
        t.TicketId,
        t.CustomerId,
        customer.FullName AS CustomerFullName,
        t.AssignedAgentId,
        CAST(NULL AS NVARCHAR(100)) AS AssignedAgentFullName,
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
    WHERE t.AssignedAgentId IS NULL
      AND t.Status <> 'Closed'
    ORDER BY
        t.CreatedAt ASC,
        t.TicketId ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO
