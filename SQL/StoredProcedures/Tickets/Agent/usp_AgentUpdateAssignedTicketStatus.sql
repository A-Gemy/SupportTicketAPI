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
            CAST('AGENT_NOT_FOUND_OR_INACTIVE' AS VARCHAR(50)) AS ResultCode;

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
                CAST('TICKET_NOT_FOUND' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        IF @Status NOT IN ('InProgress', 'Resolved')
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('INVALID_TICKET_STATUS' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        IF @CurrentStatus = 'Closed'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('CLOSED_TICKET_CANNOT_BE_UPDATED' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        -- Repeating the current status is a successful no-op.
        -- No ticket update or audit log entry is created.
        IF @CurrentStatus = @Status
        BEGIN
            COMMIT TRANSACTION;

            SELECT
                CAST(1 AS BIT) AS IsSuccess,
                CAST('TICKET_ALREADY_HAS_REQUESTED_STATUS' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        IF @CurrentStatus = 'Resolved'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('RESOLVED_TICKET_CANNOT_BE_UPDATED_BY_AGENT' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        -- Agent starts working only from Assigned.
        IF @Status = 'InProgress'
           AND @CurrentStatus <> 'Assigned'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('ONLY_ASSIGNED_TICKET_CAN_MOVE_TO_IN_PROGRESS' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        -- Agent resolves only after starting work.
        IF @Status = 'Resolved'
           AND @CurrentStatus <> 'InProgress'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_MUST_BE_IN_PROGRESS_BEFORE_RESOLVED' AS VARCHAR(50)) AS ResultCode;

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
                CAST('TICKET_NOT_FOUND' AS VARCHAR(50)) AS ResultCode;

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
        CAST('SUCCESS' AS VARCHAR(50)) AS ResultCode;
END
GO
