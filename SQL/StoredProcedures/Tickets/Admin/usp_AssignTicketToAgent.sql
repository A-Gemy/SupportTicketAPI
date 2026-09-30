USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AssignTicketToAgent
    @AdminId INT,
    @TicketId INT,
    @AgentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

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
            CAST('ADMIN_NOT_FOUND_OR_INACTIVE' AS VARCHAR(50)) AS ResultCode;

        RETURN;
    END

    DECLARE @CurrentStatus NVARCHAR(50);
    DECLARE @PreviousAgentId INT;
    DECLARE @Details NVARCHAR(1000);
    DECLARE @UpdatedAt DATETIME2;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @CurrentStatus = Status,
            @PreviousAgentId = AssignedAgentId
        FROM Tickets WITH (UPDLOCK, HOLDLOCK)
        WHERE TicketId = @TicketId;

        IF @CurrentStatus IS NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_NOT_FOUND' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        IF @CurrentStatus = 'Closed'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('CLOSED_TICKET_CANNOT_BE_ASSIGNED' AS VARCHAR(50)) AS ResultCode;

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
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('AGENT_NOT_FOUND_OR_INACTIVE' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        -- Repeating the current assignment is a successful no-op.
        -- No ticket update or audit log entry is created.
        IF @PreviousAgentId = @AgentId
        BEGIN
            COMMIT TRANSACTION;

            SELECT
                CAST(1 AS BIT) AS IsSuccess,
                CAST('TICKET_ALREADY_ASSIGNED_TO_AGENT' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        SET @Details =
            CASE
                WHEN @PreviousAgentId IS NULL
                THEN CONCAT(
                    'Ticket assigned to Agent ',
                    @AgentId,
                    '.'
                )

                ELSE CONCAT(
                    'Ticket reassigned from Agent ',
                    @PreviousAgentId,
                    ' to Agent ',
                    @AgentId,
                    '.'
                )
            END;

        SET @UpdatedAt = SYSUTCDATETIME();

        UPDATE Tickets
        SET
            AssignedAgentId = @AgentId,
            Status = CASE
                        WHEN @CurrentStatus = 'Open' THEN 'Assigned'
                        ELSE @CurrentStatus
                     END,
            UpdatedAt = @UpdatedAt
        WHERE TicketId = @TicketId;

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
            @AdminId,
            'TicketAssigned',
            'Ticket',
            @TicketId,
            @Details,
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
