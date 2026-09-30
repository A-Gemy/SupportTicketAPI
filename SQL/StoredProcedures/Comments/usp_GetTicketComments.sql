USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_GetTicketComments
    @UserId INT,
    @TicketId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @UserRole NVARCHAR(20);
    DECLARE @CustomerId INT;
    DECLARE @AssignedAgentId INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @UserRole = Role
        FROM Users WITH (HOLDLOCK)
        WHERE UserId = @UserId
          AND IsActive = 1;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('USER_NOT_FOUND_OR_INACTIVE' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        SELECT
            @CustomerId = CustomerId,
            @AssignedAgentId = AssignedAgentId
        FROM Tickets WITH (HOLDLOCK)
        WHERE TicketId = @TicketId;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_NOT_FOUND' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        IF @UserRole = 'Customer'
           AND @CustomerId <> @UserId
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_COMMENT_READ_FORBIDDEN' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        IF @UserRole = 'Agent'
           AND ISNULL(@AssignedAgentId, -1) <> @UserId
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_COMMENT_READ_FORBIDDEN' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        IF @UserRole NOT IN ('Admin', 'Customer', 'Agent')
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('TICKET_COMMENT_READ_FORBIDDEN' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END

        SELECT
            CAST(1 AS BIT) AS IsSuccess,
            CAST('SUCCESS' AS VARCHAR(50)) AS ResultCode;

        SELECT
            tc.CommentId,
            tc.TicketId,
            tc.UserId,
            u.FullName AS UserFullName,
            u.Role AS UserRole,
            tc.CommentText,
            tc.CreatedAt
        FROM TicketComments tc
        INNER JOIN Users u
            ON tc.UserId = u.UserId
        WHERE tc.TicketId = @TicketId
        ORDER BY tc.CreatedAt ASC;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO
