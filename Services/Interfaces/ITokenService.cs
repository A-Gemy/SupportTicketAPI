using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, DateTime expiresAt);

        DateTime GetAccessTokenExpiration();

    }
}
