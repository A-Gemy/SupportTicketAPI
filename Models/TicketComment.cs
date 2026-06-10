namespace SupportTicketAPI.Models
{
    public class TicketComment
    {
        public int CommentId { get; set; }

        public int TicketId { get; set; }

        public int UserId { get; set; }

        public string CommentText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

    }
}
