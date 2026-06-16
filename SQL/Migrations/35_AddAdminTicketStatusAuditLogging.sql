USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AdminUpdateTicketStatus
    @AdminId INT,
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
    DECLARE @UpdatedAt DATETIME2 = SYSUTCDATETIME();

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

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE Tickets
        SET
            Status = @Status,
            UpdatedAt = @UpdatedAt,
            ClosedAt =
                CASE
                    WHEN @Status = 'Closed'
                    THEN @UpdatedAt
                    ELSE ClosedAt
                END
        WHERE TicketId = @TicketId;

        -- Do not create an audit record if the status did not change.
        IF @CurrentStatus <> @Status
        BEGIN
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
        END

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