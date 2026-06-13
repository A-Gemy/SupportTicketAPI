USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AdminGetTicketsByAgent
    @AdminId INT,
    @AgentId INT
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
        FROM Users
        WHERE UserId = @AgentId
          AND Role = 'Agent'
          AND IsActive = 1
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Agent not found or inactive.' AS Message;

        RETURN;
    END

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Agent tickets retrieved successfully.' AS Message;

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
    INNER JOIN Users agent
        ON t.AssignedAgentId = agent.UserId
    WHERE t.AssignedAgentId = @AgentId
    ORDER BY t.CreatedAt DESC;
END
GO