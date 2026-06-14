namespace SupportTicketAPI.Models
{
    public class TicketAccessInfo
    {
        public int TicketId { get; set; }

        public int CustomerId { get; set; }

        public int? AssignedAgentId { get; set; }

        public string Status { get; set; } = string.Empty;

    }
}
