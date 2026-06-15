USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AgentUpdateAssignedTicketStatus
    @AgentId INT,
    @TicketId INT,
    @Status NVARCHAR(50)
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
            'Agent not found or inactive.' AS Message;

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
            'Ticket not found.' AS Message;

        RETURN;
    END

    IF @Status NOT IN ('InProgress', 'Resolved')
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Invalid ticket status.' AS Message;

        RETURN;
    END

    DECLARE @CurrentStatus NVARCHAR(50);

    SELECT @CurrentStatus = Status
    FROM Tickets
    WHERE TicketId = @TicketId
      AND AssignedAgentId = @AgentId;

    IF @CurrentStatus = 'Closed'
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Closed tickets cannot be updated.' AS Message;

        RETURN;
    END

    IF @CurrentStatus = 'Resolved'
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Resolved tickets cannot be updated by the agent.' AS Message;

        RETURN;
    END

    IF @Status = 'InProgress'
       AND @CurrentStatus NOT IN ('Open', 'Assigned')
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Ticket cannot be moved to InProgress from its current status.' AS Message;

        RETURN;
    END

    IF @Status = 'Resolved'
       AND @CurrentStatus <> 'InProgress'
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Ticket must be InProgress before it can be resolved.' AS Message;

        RETURN;
    END

    UPDATE Tickets
    SET
        Status = @Status,
        UpdatedAt = SYSUTCDATETIME()
    WHERE TicketId = @TicketId
      AND AssignedAgentId = @AgentId;

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Ticket status updated successfully.' AS Message;
END
GO