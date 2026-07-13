using SupportTicketAPI.Common;
using SupportTicketAPI.DTOs.Auth;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<int>> RegisterCustomerAsync(RegisterRequest request);

        Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);

        Task<ServiceResult<int>> CreateAgentAsync(CreateAgentRequest request);

        Task<ServiceResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);

        Task<ServiceResult<int>> LogoutAsync(RefreshTokenRequest request);

    }
}
