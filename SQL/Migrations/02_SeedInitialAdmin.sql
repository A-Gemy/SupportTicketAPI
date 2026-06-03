USE SupportTicketDB;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM Users
    WHERE Email = 'admin@support.com'
)
BEGIN
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
        'Ahmed Gamal',
        'admin@support.com',
        '$2a$11$8i8gQ0yEdckTfm79aKR/Xe5ffs.XQsF5INMAINt.TgCLbbddSpGqi',
        'Admin',
        1
    );
END
GO