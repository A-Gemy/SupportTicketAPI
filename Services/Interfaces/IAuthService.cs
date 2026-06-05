using SupportTicketAPI.Common;
using SupportTicketAPI.DTOs.Auth;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<int>> RegisterCustomerAsync(RegisterRequest request);

        Task<ServiceResult<object>> LoginAsync(LoginRequest request);

    }
}
