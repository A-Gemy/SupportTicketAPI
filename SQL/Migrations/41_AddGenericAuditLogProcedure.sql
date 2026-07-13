USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AddAuditLog
    @UserId INT = NULL,
    @Action NVARCHAR(100),
    @EntityName NVARCHAR(100) = NULL,
    @EntityId INT = NULL,
    @Details NVARCHAR(1000) = NULL,
    @IpAddress NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action IS NULL OR LTRIM(RTRIM(@Action)) = ''
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Action is required.' AS Message,
            CAST(NULL AS INT) AS AuditLogId;

        RETURN;
    END

    IF @UserId IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM Users
           WHERE UserId = @UserId
       )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'User not found.' AS Message,
            CAST(NULL AS INT) AS AuditLogId;

        RETURN;
    END

    INSERT INTO AuditLogs
    (
        UserId,
        Action,
        EntityName,
        EntityId,
        Details,
        IpAddress
    )
    VALUES
    (
        @UserId,
        LTRIM(RTRIM(@Action)),
        NULLIF(LTRIM(RTRIM(@EntityName)), ''),
        @EntityId,
        NULLIF(LTRIM(RTRIM(@Details)), ''),
        NULLIF(LTRIM(RTRIM(@IpAddress)), '')
    );

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Audit log added successfully.' AS Message,
        CAST(SCOPE_IDENTITY() AS INT) AS AuditLogId;
END
GO