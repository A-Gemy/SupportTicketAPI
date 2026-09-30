USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_CreateTicket
    @CustomerId INT,
    @Title NVARCHAR(200),
    @Description NVARCHAR(1000),
    @Priority NVARCHAR(20)
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
            CAST('CUSTOMER_NOT_FOUND_OR_INACTIVE' AS VARCHAR(50)) AS ResultCode,
            CAST(NULL AS INT) AS TicketId;

        RETURN;
    END

    IF @Priority NOT IN ('Low', 'Medium', 'High')
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            CAST('INVALID_PRIORITY' AS VARCHAR(50)) AS ResultCode,
            CAST(NULL AS INT) AS TicketId;

        RETURN;
    END

    DECLARE @TicketId INT;
    DECLARE @CreatedAt DATETIME2 = SYSUTCDATETIME();

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Tickets
        (
            CustomerId,
            AssignedAgentId,
            Title,
            Description,
            Status,
            Priority,
            CreatedAt,
            UpdatedAt,
            ClosedAt
        )
        VALUES
        (
            @CustomerId,
            NULL,
            @Title,
            @Description,
            'Open',
            @Priority,
            @CreatedAt,
            NULL,
            NULL
        );

        SET @TicketId = CAST(SCOPE_IDENTITY() AS INT);

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
            'TicketCreated',
            'Ticket',
            @TicketId,
            CONCAT(
                'Ticket created with priority ',
                @Priority,
                '.'
            ),
            NULL,
            @CreatedAt
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
        CAST('SUCCESS' AS VARCHAR(50)) AS ResultCode,
        @TicketId AS TicketId;
END
GO
