USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_CreateAgent
    @AdminId INT,
    @FullName NVARCHAR(100),
    @Email NVARCHAR(150),
    @PasswordHash NVARCHAR(500)
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
            CAST('ADMIN_NOT_FOUND_OR_INACTIVE' AS VARCHAR(50)) AS ResultCode,
            CAST(NULL AS INT) AS UserId;

        RETURN;
    END

    BEGIN TRY
        INSERT INTO Users
        (
            FullName,
            Email,
            PasswordHash,
            Role,
            IsActive,
            CreatedAt
        )
        VALUES
        (
            @FullName,
            @Email,
            @PasswordHash,
            'Agent',
            1,
            SYSUTCDATETIME()
        );

        SELECT
            CAST(1 AS BIT) AS IsSuccess,
            CAST('SUCCESS' AS VARCHAR(50)) AS ResultCode,
            CAST(SCOPE_IDENTITY() AS INT) AS UserId;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
        BEGIN
            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('EMAIL_ALREADY_EXISTS' AS VARCHAR(50)) AS ResultCode,
                CAST(NULL AS INT) AS UserId;

            RETURN;
        END;

        THROW;
    END CATCH
END
GO
