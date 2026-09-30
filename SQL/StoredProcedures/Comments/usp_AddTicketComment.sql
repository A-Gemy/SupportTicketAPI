USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_AddTicketComment
    @UserId INT,
    @TicketId INT,
    @CommentText NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NULLIF(LTRIM(RTRIM(@CommentText)), '') IS NULL
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            CAST('COMMENT_TEXT_REQUIRED' AS VARCHAR(50)) AS ResultCode,
            CAST(NULL AS INT) AS CommentId;

        RETURN;
    END

    DECLARE @UserRole NVARCHAR(20);
    DECLARE @CustomerId INT;
    DECLARE @AssignedAgentId INT;
    DECLARE @TicketStatus NVARCHAR(30);
    DECLARE @CommentId INT;
    DECLARE @CreatedAt DATETIME2;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @UserRole = Role
        FROM Users WITH (UPDLOCK, HOLDLOCK)
        WHERE UserId = @UserId
          AND IsActive = 1;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('USER_NOT_FOUND_OR_INACTIVE' AS VARCHAR(50)) AS ResultCode,
                CAST(NULL AS INT) AS CommentId;

            RETURN;
        END

        SELECT
            @CustomerId = CustomerId,
            @AssignedAgentId = AssignedAgentId,
            @TicketStatus = Status
        FROM Tickets WITH (UPDLOCK, HOLDLOCK)
        WHERE TicketId = @TicketId;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_NOT_FOUND' AS VARCHAR(50)) AS ResultCode,
                CAST(NULL AS INT) AS CommentId;

            RETURN;
        END

        IF @TicketStatus = 'Closed'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('CLOSED_TICKET_CANNOT_BE_COMMENTED' AS VARCHAR(50)) AS ResultCode,
                CAST(NULL AS INT) AS CommentId;

            RETURN;
        END

        IF @UserRole = 'Customer'
           AND @CustomerId <> @UserId
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_COMMENT_ADD_FORBIDDEN' AS VARCHAR(50)) AS ResultCode,
                CAST(NULL AS INT) AS CommentId;

            RETURN;
        END

        IF @UserRole = 'Agent'
           AND ISNULL(@AssignedAgentId, -1) <> @UserId
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_COMMENT_ADD_FORBIDDEN' AS VARCHAR(50)) AS ResultCode,
                CAST(NULL AS INT) AS CommentId;

            RETURN;
        END

        IF @UserRole NOT IN ('Admin', 'Customer', 'Agent')
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_COMMENT_ADD_FORBIDDEN' AS VARCHAR(50)) AS ResultCode,
                CAST(NULL AS INT) AS CommentId;

            RETURN;
        END

        SET @CreatedAt = SYSUTCDATETIME();

        INSERT INTO TicketComments
        (
            TicketId,
            UserId,
            CommentText,
            CreatedAt
        )
        VALUES
        (
            @TicketId,
            @UserId,
            LTRIM(RTRIM(@CommentText)),
            @CreatedAt
        );

        SET @CommentId = CAST(SCOPE_IDENTITY() AS INT);

        UPDATE Tickets
        SET UpdatedAt = @CreatedAt
        WHERE TicketId = @TicketId;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_NOT_FOUND' AS VARCHAR(50)) AS ResultCode,
                CAST(NULL AS INT) AS CommentId;

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
            @UserId,
            'TicketCommentAdded',
            'Ticket',
            @TicketId,
            CONCAT(
                'Comment ',
                @CommentId,
                ' added to ticket.'
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
        @CommentId AS CommentId;
END
GO
