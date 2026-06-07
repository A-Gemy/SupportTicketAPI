using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, DateTime expiresAt);

        DateTime GetAccessTokenExpiration();

        string GenerateRefreshToken();

        DateTime GetRefreshTokenExpiration();

        string HashRefreshToken(string refreshToken);

    }
}
