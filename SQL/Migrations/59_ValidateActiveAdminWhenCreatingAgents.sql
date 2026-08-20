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
            'Admin not found or inactive.' AS Message,
            CAST(NULL AS INT) AS UserId;

        RETURN;
    END

    IF EXISTS
    (
        SELECT 1
        FROM Users
        WHERE Email = @Email
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Email already exists.' AS Message,
            CAST(NULL AS INT) AS UserId;

        RETURN;
    END

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
        'Agent created successfully.' AS Message,
        CAST(SCOPE_IDENTITY() AS INT) AS UserId;
END
GO