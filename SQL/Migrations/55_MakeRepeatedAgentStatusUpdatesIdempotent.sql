USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AgentUpdateAssignedTicketStatus
    @AgentId INT,
    @TicketId INT,
    @Status NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

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

    DECLARE @CurrentStatus NVARCHAR(50);
    DECLARE @UpdatedAt DATETIME2;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @CurrentStatus = Status
        FROM Tickets WITH (UPDLOCK, HOLDLOCK)
        WHERE TicketId = @TicketId
          AND AssignedAgentId = @AgentId;

        IF @CurrentStatus IS NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Ticket not found.' AS Message;

            RETURN;
        END

        IF @Status NOT IN ('InProgress', 'Resolved')
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Invalid ticket status.' AS Message;

            RETURN;
        END

        IF @CurrentStatus = 'Closed'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Closed tickets cannot be updated.' AS Message;

            RETURN;
        END

        -- Repeating the current status is a successful no-op.
        -- No ticket update or audit log entry is created.
        IF @CurrentStatus = @Status
        BEGIN
            COMMIT TRANSACTION;

            SELECT
                CAST(1 AS BIT) AS IsSuccess,
                'Ticket already has the requested status.' AS Message;

            RETURN;
        END

        IF @CurrentStatus = 'Resolved'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Resolved tickets cannot be updated by the agent.' AS Message;

            RETURN;
        END

        -- Agent starts working only from Assigned.
        IF @Status = 'InProgress'
           AND @CurrentStatus <> 'Assigned'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Only an assigned ticket can be moved to InProgress.' AS Message;

            RETURN;
        END

        -- Agent resolves only after starting work.
        IF @Status = 'Resolved'
           AND @CurrentStatus <> 'InProgress'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Ticket must be InProgress before it can be resolved.' AS Message;

            RETURN;
        END

        SET @UpdatedAt = SYSUTCDATETIME();

        UPDATE Tickets
        SET
            Status = @Status,
            UpdatedAt = @UpdatedAt
        WHERE TicketId = @TicketId
          AND AssignedAgentId = @AgentId;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Ticket not found.' AS Message;

            RETURN;
        END

        INSERT INTO AuditLogs
        (
            UserId,
            Action,
            EntityName,
            EntityId,
            Details,
            IpAddress,
            CreatedAt
        )
        VALUES
        (
            @AgentId,
            'TicketStatusChanged',
            'Ticket',
            @TicketId,
            CONCAT(
                'Status changed from ',
                @CurrentStatus,
                ' to ',
                @Status,
                '.'
            ),
            NULL,
            @UpdatedAt
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Ticket status updated successfully.' AS Message;
END
GO