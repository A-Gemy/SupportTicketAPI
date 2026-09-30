USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_RegisterCustomer
    @FullName NVARCHAR(100),
    @Email NVARCHAR(150),
    @PasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
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
            CAST('SUCCESS' AS VARCHAR(50)) AS ResultCode,
            CAST(SCOPE_IDENTITY() AS INT) AS UserId;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
        BEGIN
            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                CAST('EMAIL_ALREADY_EXISTS' AS VARCHAR(50)) AS ResultCode,
                CAST(NULL AS INT) AS UserId;

            RETURN;
        END;

        THROW;
    END CATCH
END
GO
