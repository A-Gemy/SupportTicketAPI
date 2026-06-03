USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_RegisterCustomer
    @FullName NVARCHAR(100),
    @Email NVARCHAR(150),
    @PasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

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
            NULL AS UserId;

        RETURN;
    END

    INSERT INTO Users
    (
        FullName,
        Email,
        PasswordHash,
        Role,
        IsActive
    )
    VALUES
    (
        @FullName,
        @Email,
        @PasswordHash,
        'Customer',
        1
    );

    SELECT 
        CAST(1 AS BIT) AS IsSuccess,
        'Customer registered successfully.' AS Message,
        SCOPE_IDENTITY() AS UserId;
END
GO