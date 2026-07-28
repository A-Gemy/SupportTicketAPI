USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AdminGetAuditLogs
    @AdminId INT,
    @Action NVARCHAR(100) = NULL,
    @ActorUserId INT = NULL,
    @EntityName NVARCHAR(100) = NULL,
    @EntityId INT = NULL,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

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
            'Admin not found or inactive.' AS Message,
            CAST(0 AS INT) AS TotalCount,
            @PageNumber AS PageNumber,
            @PageSize AS PageSize;

        RETURN;
    END

    IF @FromDate IS NOT NULL
       AND @ToDate IS NOT NULL
       AND @FromDate > @ToDate
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'FromDate cannot be later than ToDate.' AS Message,
            CAST(0 AS INT) AS TotalCount,
            @PageNumber AS PageNumber,
            @PageSize AS PageSize;

        RETURN;
    END

    IF @PageNumber < 1
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Page number must be greater than or equal to 1.' AS Message,
            CAST(0 AS INT) AS TotalCount,
            @PageNumber AS PageNumber,
            @PageSize AS PageSize;

        RETURN;
    END

    IF @PageSize < 1 OR @PageSize > 100
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Page size must be between 1 and 100.' AS Message,
            CAST(0 AS INT) AS TotalCount,
            @PageNumber AS PageNumber,
            @PageSize AS PageSize;

        RETURN;
    END

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @TotalCount INT;

    SELECT
        @TotalCount = COUNT(*)
    FROM AuditLogs auditLog
    WHERE
        (@Action IS NULL OR auditLog.Action = @Action)
        AND (@ActorUserId IS NULL OR auditLog.UserId = @ActorUserId)
        AND (@EntityName IS NULL OR auditLog.EntityName = @EntityName)
        AND (@EntityId IS NULL OR auditLog.EntityId = @EntityId)
        AND (@FromDate IS NULL OR auditLog.CreatedAt >= @FromDate)
        AND (@ToDate IS NULL OR auditLog.CreatedAt <= @ToDate);

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Audit logs retrieved successfully.' AS Message,
        @TotalCount AS TotalCount,
        @PageNumber AS PageNumber,
        @PageSize AS PageSize;

    SELECT
        auditLog.AuditLogId,
        auditLog.UserId,
        users.FullName AS ActorFullName,
        users.Role AS ActorRole,
        auditLog.Action,
        auditLog.EntityName,
        auditLog.EntityId,
        auditLog.Details,
        auditLog.IpAddress,
        auditLog.CreatedAt
    FROM AuditLogs auditLog
    LEFT JOIN Users users
        ON users.UserId = auditLog.UserId
    WHERE
        (@Action IS NULL OR auditLog.Action = @Action)
        AND (@ActorUserId IS NULL OR auditLog.UserId = @ActorUserId)
        AND (@EntityName IS NULL OR auditLog.EntityName = @EntityName)
        AND (@EntityId IS NULL OR auditLog.EntityId = @EntityId)
        AND (@FromDate IS NULL OR auditLog.CreatedAt >= @FromDate)
        AND (@ToDate IS NULL OR auditLog.CreatedAt <= @ToDate)
    ORDER BY
        auditLog.CreatedAt DESC,
        auditLog.AuditLogId DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO