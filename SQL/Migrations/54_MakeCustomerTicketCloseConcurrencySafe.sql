USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_CloseCustomerTicket
    @CustomerId INT,
    @TicketId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM Users
        WHERE UserId = @CustomerId
          AND Role = 'Customer'
          AND IsActive = 1
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Customer not found or inactive.' AS Message;

        RETURN;
    END

    DECLARE @CurrentStatus NVARCHAR(50);
    DECLARE @ClosedAt DATETIME2;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @CurrentStatus = Status
        FROM Tickets WITH (UPDLOCK, HOLDLOCK)
        WHERE TicketId = @TicketId
          AND CustomerId = @CustomerId;

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
                'Ticket is already closed.' AS Message;

            RETURN;
        END

        SET @ClosedAt = SYSUTCDATETIME();

        UPDATE Tickets
        SET
            Status = 'Closed',
            UpdatedAt = @ClosedAt,
            ClosedAt = @ClosedAt
        WHERE TicketId = @TicketId
          AND CustomerId = @CustomerId;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Failed to close ticket.' AS Message;

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
            @CustomerId,
            'TicketStatusChanged',
            'Ticket',
            @TicketId,
            CONCAT(
                'Status changed from ',
                @CurrentStatus,
                ' to Closed.'
            ),
            NULL,
            @ClosedAt
        );

        COMMIT TRANSACTION;

        SELECT
            CAST(1 AS BIT) AS IsSuccess,
            'Ticket closed successfully.' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        THROW;
    END CATCH
END
GO