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

    IF EXISTS
    (
        SELECT 1
        FROM Tickets
        WHERE TicketId = @TicketId
          AND Status = 'Closed'
    )
    BEGIN
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
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Agent not found or inactive.' AS Message;

        RETURN;
    END

    DECLARE @PreviousAgentId INT;
    DECLARE @Details NVARCHAR(1000);
    DECLARE @UpdatedAt DATETIME2 = SYSUTCDATETIME();

    SELECT @PreviousAgentId = AssignedAgentId
    FROM Tickets
    WHERE TicketId = @TicketId;

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

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE Tickets
        SET
            AssignedAgentId = @AgentId,
            Status = 'Assigned',
            UpdatedAt = @UpdatedAt
        WHERE TicketId = @TicketId;

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