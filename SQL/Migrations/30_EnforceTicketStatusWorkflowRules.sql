USE SupportTicketDB;
GO

-- =====================================================
-- Enforce Admin ticket status rules
-- =====================================================

CREATE OR ALTER PROCEDURE usp_AdminUpdateTicketStatus
    @AdminId INT,
    @TicketId INT,
    @Status NVARCHAR(50)
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
        FROM Tickets
        WHERE TicketId = @TicketId
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Ticket not found.' AS Message;

        RETURN;
    END

    IF @Status NOT IN ('Open', 'InProgress', 'Resolved', 'Closed')
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Invalid ticket status.' AS Message;

        RETURN;
    END

    DECLARE @CurrentStatus NVARCHAR(50);
    DECLARE @AssignedAgentId INT;

    SELECT
        @CurrentStatus = Status,
        @AssignedAgentId = AssignedAgentId
    FROM Tickets
    WHERE TicketId = @TicketId;

    IF @CurrentStatus = 'Closed'
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Closed tickets cannot be updated.' AS Message;

        RETURN;
    END

    -- An assigned ticket must not return to Open.
    IF @Status = 'Open'
       AND @AssignedAgentId IS NOT NULL
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'An assigned ticket cannot be moved to Open.' AS Message;

        RETURN;
    END

    -- Working statuses require an assigned Agent.
    IF @Status IN ('InProgress', 'Resolved')
       AND @AssignedAgentId IS NULL
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'The ticket must be assigned before using this status.' AS Message;

        RETURN;
    END

    UPDATE Tickets
    SET
        Status = @Status,
        UpdatedAt = SYSUTCDATETIME(),
        ClosedAt =
            CASE
                WHEN @Status = 'Closed'
                    THEN SYSUTCDATETIME()
                ELSE ClosedAt
            END
    WHERE TicketId = @TicketId;

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Ticket status updated successfully.' AS Message;
END
GO

-- =====================================================
-- Enforce Agent ticket status transitions
-- =====================================================

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

    -- Agent starts working only from Assigned.
    IF @Status = 'InProgress'
       AND @CurrentStatus <> 'Assigned'
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Only an assigned ticket can be moved to InProgress.' AS Message;

        RETURN;
    END

    -- Agent resolves only after starting work.
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