USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_RevokeRefreshToken
    @TokenHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @RefreshTokenId INT;
    DECLARE @RevokedAt DATETIME2;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @RefreshTokenId = RefreshTokenId,
            @RevokedAt = RevokedAt
        FROM RefreshTokens WITH (UPDLOCK, HOLDLOCK)
        WHERE TokenHash = @TokenHash;

        IF @RefreshTokenId IS NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('REFRESH_TOKEN_NOT_FOUND' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END;

        IF @RevokedAt IS NOT NULL
        BEGIN
            COMMIT TRANSACTION;

            SELECT
                CAST(1 AS BIT) AS IsSuccess,
                CAST('REFRESH_TOKEN_ALREADY_REVOKED' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END;

        UPDATE RefreshTokens
        SET RevokedAt = SYSUTCDATETIME()
        WHERE RefreshTokenId = @RefreshTokenId;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('REFRESH_TOKEN_REVOCATION_FAILED' AS VARCHAR(50)) AS ResultCode;

            RETURN;
        END;

        COMMIT TRANSACTION;

        SELECT
            CAST(1 AS BIT) AS IsSuccess,
            CAST('SUCCESS' AS VARCHAR(50)) AS ResultCode;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        THROW;
    END CATCH;
END;
GO
