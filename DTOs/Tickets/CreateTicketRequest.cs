using System.ComponentModel.DataAnnotations;

namespace SupportTicketAPI.DTOs.Tickets
{
    public class CreateTicketRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = string.Empty;

    }
}
