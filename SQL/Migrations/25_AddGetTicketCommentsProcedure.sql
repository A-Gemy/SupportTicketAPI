USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_GetTicketComments
    @TicketId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM Tickets
        WHERE TicketId = @TicketId
    )
    BEGIN
        SELECT
            CAST(0 AS BIT) AS IsSuccess,
            'Ticket not found.' AS Message;

        RETURN;
    END

    SELECT
        CAST(1 AS BIT) AS IsSuccess,
        'Ticket comments retrieved successfully.' AS Message;

    SELECT
        tc.CommentId,
        tc.TicketId,
        tc.UserId,
        u.FullName AS UserFullName,
        u.Role AS UserRole,
        tc.CommentText,
        tc.CreatedAt
    FROM TicketComments tc
    INNER JOIN Users u
        ON tc.UserId = u.UserId
    WHERE tc.TicketId = @TicketId
    ORDER BY tc.CreatedAt ASC;
END
GO