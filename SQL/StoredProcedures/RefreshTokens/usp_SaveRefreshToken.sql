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
            CAST('USER_NOT_FOUND' AS VARCHAR(50)) AS ResultCode,
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
        CAST('SUCCESS' AS VARCHAR(50)) AS ResultCode,
        CAST(SCOPE_IDENTITY() AS INT) AS RefreshTokenId;
END
GO
