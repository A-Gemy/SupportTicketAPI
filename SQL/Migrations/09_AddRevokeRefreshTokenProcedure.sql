USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_RevokeRefreshToken
    @TokenHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM RefreshTokens WHERE TokenHash = @TokenHash)
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Refresh token not found.' AS Message;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM RefreshTokens
        WHERE TokenHash = @TokenHash
          AND RevokedAt IS NOT NULL
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Refresh token is already revoked.' AS Message;
        RETURN;
    END

    UPDATE RefreshTokens
    SET RevokedAt = SYSUTCDATETIME()
    WHERE TokenHash = @TokenHash;

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Refresh token revoked successfully.' AS Message;
END
GO