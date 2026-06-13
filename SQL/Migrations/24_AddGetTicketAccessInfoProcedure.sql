USE SupportTicketDB;
GO

CREATE OR ALTER PROCEDURE usp_GetTicketAccessInfo
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
        'Ticket access information retrieved successfully.' AS Message;

    SELECT
        TicketId,
        CustomerId,
        AssignedAgentId,
        Status
    FROM Tickets
    WHERE TicketId = @TicketId;
END
GO