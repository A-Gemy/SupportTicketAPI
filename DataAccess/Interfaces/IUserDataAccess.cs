using SupportTicketAPI.Common;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.DataAccess.Interfaces
{

    public interface IUserDataAccess
    {
        Task<ServiceResult<int>> RegisterCustomerAsync(string fullName, string email, string passwordHash);

        Task<User?> GetUserByEmailAsync(string email);

        Task<ServiceResult<int>> CreateAgentAsync(string fullName, string email, string passwordHash);

        Task<ServiceResult<int>> SaveRefreshTokenAsync(int userId, string tokenHash, DateTime expiresAt);

        Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);

        Task<ServiceResult<RefreshTokenRotationResult>> RotateRefreshTokenAsync(string oldTokenHash, string newTokenHash, DateTime newExpiresAt);

        Task<ServiceResult<bool>> RevokeRefreshTokenAsync(string tokenHash);

    }

}
