using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.DTOs.Auth;
using SupportTicketAPI.Services.Interfaces;

namespace SupportTicketAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserDataAccess _userDataAccess;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUserDataAccess userDataAccess, IPasswordHasher passwordHasher)
        {
            _userDataAccess = userDataAccess;
            _passwordHasher = passwordHasher;
        }

        public async Task<ServiceResult<int>> RegisterCustomerAsync(RegisterRequest request)
        {
            if (request == null)
                return ServiceResult<int>.Failure("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.FullName))
                return ServiceResult<int>.Failure("Full name is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                return ServiceResult<int>.Failure("Email is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return ServiceResult<int>.Failure("Password is required.");

            string passwordHash = _passwordHasher.HashPassword(request.Password);

            return await _userDataAccess.RegisterCustomerAsync(
                request.FullName.Trim(),
                request.Email.Trim(),
                passwordHash
            );

        }
    }
}
