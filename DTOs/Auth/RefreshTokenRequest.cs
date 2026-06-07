using System.ComponentModel.DataAnnotations;

namespace SupportTicketAPI.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;

    }
}
