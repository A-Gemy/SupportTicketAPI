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
        private readonly ITokenService _tokenService;

        public AuthService(
            IUserDataAccess userDataAccess,
            IPasswordHasher passwordHasher,
            ITokenService tokenService
            )
        {
            _userDataAccess = userDataAccess;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
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

        public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
        {
            if (request == null)
                return ServiceResult<LoginResponse>.Failure("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.Email))
                return ServiceResult<LoginResponse>.Failure("Email is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return ServiceResult<LoginResponse>.Failure("Password is required.");


            var user = await _userDataAccess.GetUserByEmailAsync(request.Email.Trim());


            if (user == null)
                return ServiceResult<LoginResponse>.Failure("Invalid email or password.");

            if (!user.IsActive)
                return ServiceResult<LoginResponse>.Failure("This account is inactive.");

            bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

            if (!isPasswordValid)
                return ServiceResult<LoginResponse>.Failure("Invalid email or password.");


            DateTime accessTokenExpiresAt = _tokenService.GetAccessTokenExpiration();
            string accessToken = _tokenService.GenerateAccessToken(user, accessTokenExpiresAt);

            DateTime refreshTokenExpiresAt = _tokenService.GetRefreshTokenExpiration();
            string refreshToken = _tokenService.GenerateRefreshToken();
            string refreshTokenHash = _tokenService.HashRefreshToken(refreshToken);

            var saveRefreshTokenResult = await _userDataAccess.SaveRefreshTokenAsync(
                user.UserId,
                refreshTokenHash,
                refreshTokenExpiresAt
            );

            if (!saveRefreshTokenResult.IsSuccess)
                return ServiceResult<LoginResponse>.Failure(saveRefreshTokenResult.Message);

            var loginResponse = new LoginResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
            };

            return ServiceResult<LoginResponse>.Success(loginResponse, "Login succeeded.");
        }

        public async Task<ServiceResult<int>> CreateAgentAsync(CreateAgentRequest request)
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

            return await _userDataAccess.CreateAgentAsync(
                request.FullName.Trim(),
                request.Email.Trim(),
                passwordHash
            );
        }

        public async Task<ServiceResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (request == null)
                return ServiceResult<LoginResponse>.Failure("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return ServiceResult<LoginResponse>.Failure("Refresh token is required.");

            string oldRefreshTokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

            var storedRefreshToken = await _userDataAccess.GetRefreshTokenAsync(oldRefreshTokenHash);

            if (storedRefreshToken == null)
                return ServiceResult<LoginResponse>.Failure("Invalid refresh token.");

            if (storedRefreshToken.RevokedAt != null)
                return ServiceResult<LoginResponse>.Failure("Refresh token has been revoked.");

            if (storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
                return ServiceResult<LoginResponse>.Failure("Refresh token has expired.");

            if (!storedRefreshToken.User.IsActive)
                return ServiceResult<LoginResponse>.Failure("This account is inactive.");

            var revokeOldTokenResult = await _userDataAccess.RevokeRefreshTokenAsync(oldRefreshTokenHash);

            if (!revokeOldTokenResult.IsSuccess)
                return ServiceResult<LoginResponse>.Failure(revokeOldTokenResult.Message);

            DateTime accessTokenExpiresAt = _tokenService.GetAccessTokenExpiration();
            string accessToken = _tokenService.GenerateAccessToken(storedRefreshToken.User, accessTokenExpiresAt);

            DateTime newRefreshTokenExpires = _tokenService.GetRefreshTokenExpiration();
            string newRefreshToken = _tokenService.GenerateRefreshToken();
            string newRefreshTokenHash = _tokenService.HashRefreshToken(newRefreshToken);

            var saveNewRefreshTokenResult = await _userDataAccess.SaveRefreshTokenAsync(
                storedRefreshToken.User.UserId,
                newRefreshTokenHash,
                newRefreshTokenExpires
            );

            if (!saveNewRefreshTokenResult.IsSuccess)
                return ServiceResult<LoginResponse>.Failure(saveNewRefreshTokenResult.Message);

            var response = new LoginResponse
            {
                UserId = storedRefreshToken.User.UserId,
                FullName = storedRefreshToken.User.FullName,
                Email = storedRefreshToken.User.Email,
                Role = storedRefreshToken.User.Role,

                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,

                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAt = newRefreshTokenExpires
            };

            return ServiceResult<LoginResponse>.Success(response, "Token refreshed successfully.");
        }

        public async Task<ServiceResult<int>> LogoutAsync(RefreshTokenRequest request)
        {
            if (request == null)
                return ServiceResult<int>.Failure("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return ServiceResult<int>.Failure("Refresh token is required.");

            string refreshTokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

            var storedRefreshToken = await _userDataAccess.GetRefreshTokenAsync(refreshTokenHash);

            if (storedRefreshToken == null)
                return ServiceResult<int>.Failure("Invalid refresh token.");

            var revokeResult = await _userDataAccess.RevokeRefreshTokenAsync(refreshTokenHash);

            if (!revokeResult.IsSuccess)
                return ServiceResult<int>.Failure(revokeResult.Message);

            return ServiceResult<int>.Success(storedRefreshToken.UserId, "Logged out successfully.");
        }

    }
}
