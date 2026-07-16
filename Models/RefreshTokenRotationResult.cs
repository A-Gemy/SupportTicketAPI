namespace SupportTicketAPI.Models
{
    public class RefreshTokenRotationResult
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public int RefreshTokenId { get; set; }

    }
}
