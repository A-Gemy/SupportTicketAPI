USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_GetRefreshToken
    @TokenHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        rt.RefreshTokenId,
        rt.UserId,
        rt.TokenHash,
        rt.ExpiresAt,
        rt.RevokedAt,
        rt.CreatedAt,
        u.FullName,
        u.Email,
        u.PasswordHash,
        u.Role,
        u.IsActive,
        u.CreatedAt AS UserCreatedAt
    FROM RefreshTokens rt
    INNER JOIN Users u ON rt.UserId = u.UserId
    WHERE rt.TokenHash = @TokenHash;
END
GO