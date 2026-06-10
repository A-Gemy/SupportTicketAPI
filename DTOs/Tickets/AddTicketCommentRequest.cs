using System.ComponentModel.DataAnnotations;

namespace SupportTicketAPI.DTOs.Tickets
{
    public class AddTicketCommentRequest
    {
        [Required]
        [MaxLength(1000)]
        public string CommentText { get; set; } = string.Empty;

    }
}
