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
            CAST('ADMIN_NOT_FOUND_OR_INACTIVE' AS VARCHAR(50)) AS ResultCode;

        RETURN;
    END

    IF @Status NOT IN ('Open', 'InProgress', 'Resolved', 'Closed')
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            CAST('INVALID_TICKET_STATUS' AS VARCHAR(50)) AS ResultCode;

        RETURN;
    END

    DECLARE @CurrentStatus NVARCHAR(50);
    DECLARE @AssignedAgentId INT;
    DECLARE @UpdatedAt DATETIME2;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @CurrentStatus = Status,
            @AssignedAgentId = AssignedAgentId
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
                CAST('CLOSED_TICKET_CANNOT_BE_UPDATED' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        -- No update is needed when the requested status
        -- is already the current ticket status.
        IF @CurrentStatus = @Status
        BEGIN
            COMMIT TRANSACTION;

            SELECT
                CAST(1 AS BIT) AS IsSuccess,
                CAST('TICKET_ALREADY_HAS_REQUESTED_STATUS' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        -- An assigned ticket must not return to Open.
        IF @Status = 'Open'
           AND @AssignedAgentId IS NOT NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('ASSIGNED_TICKET_CANNOT_BE_MOVED_TO_OPEN' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        -- Working statuses require an assigned Agent.
        IF @Status IN ('InProgress', 'Resolved')
           AND @AssignedAgentId IS NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_MUST_BE_ASSIGNED_FOR_STATUS' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        SET @UpdatedAt = SYSUTCDATETIME();

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
