USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_SaveRefreshToken
    @UserId INT,
    @TokenHash NVARCHAR(255),
    @ExpiresAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId)
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'User not found.' AS Message,
            CAST(NULL AS INT) AS RefreshTokenId;
        RETURN;
    END

    INSERT INTO RefreshTokens
    (
        UserId,
        TokenHash,
        ExpiresAt,
        RevokedAt,
        CreatedAt
    )
    VALUES
    (
        @UserId,
        @TokenHash,
        @ExpiresAt,
        NULL,
        SYSUTCDATETIME()
    );

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Refresh token saved successfully.' AS Message,
        CAST(SCOPE_IDENTITY() AS INT) AS RefreshTokenId;
END
GO