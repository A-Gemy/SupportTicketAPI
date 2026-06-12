using System.ComponentModel.DataAnnotations;

namespace SupportTicketAPI.DTOs.Tickets
{
    public class UpdateTicketStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;

    }
}
