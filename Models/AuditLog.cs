namespace SupportTicketAPI.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        public int? UserId { get; set; }

        public string? ActorFullName { get; set; }

        public string? ActorRole { get; set; }

        public string Action { get; set; } = string.Empty;

        public string? EntityName { get; set; }

        public int? EntityId { get; set; }

        public string? Details { get; set; }

        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
