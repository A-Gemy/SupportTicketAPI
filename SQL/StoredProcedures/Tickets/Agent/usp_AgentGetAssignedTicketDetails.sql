USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AgentGetAssignedTicketDetails
    @AgentId INT,
    @TicketId INT
AS
BEGIN
    SET NOCOUNT ON;

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
            CAST('AGENT_NOT_FOUND_OR_INACTIVE' AS VARCHAR(50)) AS ResultCode;

        RETURN;
    END

    IF NOT EXISTS
    (
        SELECT 1
        FROM Tickets
        WHERE TicketId = @TicketId
          AND AssignedAgentId = @AgentId
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
    WHERE t.TicketId = @TicketId
      AND t.AssignedAgentId = @AgentId;
END
GO
