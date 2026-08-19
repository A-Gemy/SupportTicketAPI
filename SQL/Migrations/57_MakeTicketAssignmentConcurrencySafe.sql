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
            'Admin not found or inactive.' AS Message;

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
                'Ticket not found.' AS Message;

            RETURN;
        END

        IF @CurrentStatus = 'Closed'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Closed tickets cannot be assigned.' AS Message;

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
                'Agent not found or inactive.' AS Message;

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

                WHEN @PreviousAgentId = @AgentId
                THEN CONCAT(
                    'Ticket assigned again to Agent ',
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
            Status =
                CASE
                    WHEN @CurrentStatus = 'Open'
                    THEN 'Assigned'
                    ELSE @CurrentStatus
                END,
            UpdatedAt = @UpdatedAt
        WHERE TicketId = @TicketId;

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
        'Ticket assigned to agent successfully.' AS Message;
END
GO