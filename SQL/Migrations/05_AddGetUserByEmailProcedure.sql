USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_GetUserByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        FullName,
        Email,
        PasswordHash,
        Role,
        IsActive,
        CreatedAt
    FROM Users
    WHERE Email = @Email;
END
GO