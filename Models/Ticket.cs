namespace SupportTicketAPI.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerFullName { get; set; } = string.Empty;

        public int? AssignedAgentId { get; set; }

        public string? AssignedAgentFullName { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

    }
}
