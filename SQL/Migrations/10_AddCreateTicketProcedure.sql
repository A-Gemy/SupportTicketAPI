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
            'Customer not found or inactive.' AS Message,
            CAST(NULL AS INT) AS TicketId;

        RETURN;
    END

    IF @Priority NOT IN ('Low', 'Medium', 'High')
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Invalid priority.' AS Message,
            CAST(NULL AS INT) AS TicketId;

        RETURN;
    END

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
        SYSUTCDATETIME(),
        NULL,
        NULL
    );

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Ticket created successfully.' AS Message,
        CAST(SCOPE_IDENTITY() AS INT) AS TicketId;
END
GO