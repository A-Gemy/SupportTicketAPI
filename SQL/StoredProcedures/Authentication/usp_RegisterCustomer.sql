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
            'Customer registered successfully.' AS Message,
            CAST(SCOPE_IDENTITY() AS INT) AS UserId;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
        BEGIN
            SELECT
                CAST(0 AS BIT) AS IsSuccess,
                'Email already exists.' AS Message,
                CAST(NULL AS INT) AS UserId;

            RETURN;
        END;

        THROW;
    END CATCH
END
GO
