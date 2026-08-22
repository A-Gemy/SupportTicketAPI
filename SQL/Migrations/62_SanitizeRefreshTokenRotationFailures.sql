USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_RotateRefreshToken
    @OldTokenHash NVARCHAR(255),
    @NewTokenHash NVARCHAR(255),
    @NewExpiresAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @OldTokenHash IS NULL
       OR LTRIM(RTRIM(@OldTokenHash)) = ''
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Old refresh token hash is required.' AS Message,
            CAST(NULL AS INT) AS UserId,
            CAST(NULL AS NVARCHAR(100)) AS FullName,
            CAST(NULL AS NVARCHAR(150)) AS Email,
            CAST(NULL AS NVARCHAR(20)) AS Role,
            CAST(NULL AS INT) AS RefreshTokenId;

        RETURN;
    END

    IF @NewTokenHash IS NULL
       OR LTRIM(RTRIM(@NewTokenHash)) = ''
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'New refresh token hash is required.' AS Message,
            CAST(NULL AS INT) AS UserId,
            CAST(NULL AS NVARCHAR(100)) AS FullName,
            CAST(NULL AS NVARCHAR(150)) AS Email,
            CAST(NULL AS NVARCHAR(20)) AS Role,
            CAST(NULL AS INT) AS RefreshTokenId;

        RETURN;
    END

    IF @NewExpiresAt <= SYSUTCDATETIME()
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'New refresh token expiration must be in the future.' AS Message,
            CAST(NULL AS INT) AS UserId,
            CAST(NULL AS NVARCHAR(100)) AS FullName,
            CAST(NULL AS NVARCHAR(150)) AS Email,
            CAST(NULL AS NVARCHAR(20)) AS Role,
            CAST(NULL AS INT) AS RefreshTokenId;

        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @OldRefreshTokenId INT;
        DECLARE @UserId INT;
        DECLARE @FullName NVARCHAR(100);
        DECLARE @Email NVARCHAR(150);
        DECLARE @Role NVARCHAR(20);
        DECLARE @OldExpiresAt DATETIME2;
        DECLARE @OldRevokedAt DATETIME2;
        DECLARE @IsActive BIT;
        DECLARE @NewRefreshTokenId INT;

        SELECT
            @OldRefreshTokenId = RT.RefreshTokenId,
            @UserId = RT.UserId,
            @OldExpiresAt = RT.ExpiresAt,
            @OldRevokedAt = RT.RevokedAt,
            @FullName = U.FullName,
            @Email = U.Email,
            @Role = U.Role,
            @IsActive = U.IsActive
        FROM RefreshTokens RT WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN Users U
            ON U.UserId = RT.UserId
        WHERE RT.TokenHash = @OldTokenHash;

        IF @OldRefreshTokenId IS NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Invalid refresh token.' AS Message,
                CAST(NULL AS INT) AS UserId,
                CAST(NULL AS NVARCHAR(100)) AS FullName,
                CAST(NULL AS NVARCHAR(150)) AS Email,
                CAST(NULL AS NVARCHAR(20)) AS Role,
                CAST(NULL AS INT) AS RefreshTokenId;

            RETURN;
        END

        IF @OldRevokedAt IS NOT NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Refresh token has been revoked.' AS Message,
                CAST(NULL AS INT) AS UserId,
                CAST(NULL AS NVARCHAR(100)) AS FullName,
                CAST(NULL AS NVARCHAR(150)) AS Email,
                CAST(NULL AS NVARCHAR(20)) AS Role,
                CAST(NULL AS INT) AS RefreshTokenId;

            RETURN;
        END

        IF @OldExpiresAt <= SYSUTCDATETIME()
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Refresh token has expired.' AS Message,
                CAST(NULL AS INT) AS UserId,
                CAST(NULL AS NVARCHAR(100)) AS FullName,
                CAST(NULL AS NVARCHAR(150)) AS Email,
                CAST(NULL AS NVARCHAR(20)) AS Role,
                CAST(NULL AS INT) AS RefreshTokenId;

            RETURN;
        END

        IF @IsActive = 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'This account is inactive.' AS Message,
                CAST(NULL AS INT) AS UserId,
                CAST(NULL AS NVARCHAR(100)) AS FullName,
                CAST(NULL AS NVARCHAR(150)) AS Email,
                CAST(NULL AS NVARCHAR(20)) AS Role,
                CAST(NULL AS INT) AS RefreshTokenId;

            RETURN;
        END

        UPDATE RefreshTokens
        SET RevokedAt = SYSUTCDATETIME()
        WHERE RefreshTokenId = @OldRefreshTokenId;

        INSERT INTO RefreshTokens
        (
            UserId,
            TokenHash,
            ExpiresAt
        )
        VALUES
        (
            @UserId,
            @NewTokenHash,
            @NewExpiresAt
        );

        SET @NewRefreshTokenId =
            CAST(SCOPE_IDENTITY() AS INT);

        COMMIT TRANSACTION;

        SELECT
            CAST(1 AS BIT) AS IsSuccess,
            'Token refreshed successfully.' AS Message,
            @UserId AS UserId,
            @FullName AS FullName,
            @Email AS Email,
            @Role AS Role,
            @NewRefreshTokenId AS RefreshTokenId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Failed to rotate refresh token.' AS Message,
            CAST(NULL AS INT) AS UserId,
            CAST(NULL AS NVARCHAR(100)) AS FullName,
            CAST(NULL AS NVARCHAR(150)) AS Email,
            CAST(NULL AS NVARCHAR(20)) AS Role,
            CAST(NULL AS INT) AS RefreshTokenId;
    END CATCH
END
GO