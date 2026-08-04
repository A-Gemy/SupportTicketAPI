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
                'Refresh token not found.' AS Message;

            RETURN;
        END;

        IF @RevokedAt IS NOT NULL
        BEGIN
            COMMIT TRANSACTION;

            SELECT
                CAST(1 AS BIT) AS IsSuccess,
                'Refresh token is already revoked.' AS Message;

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
                'Failed to revoke refresh token.' AS Message;

            RETURN;
        END;

        COMMIT TRANSACTION;

        SELECT
            CAST(1 AS BIT) AS IsSuccess,
            'Refresh token revoked successfully.' AS Message;
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