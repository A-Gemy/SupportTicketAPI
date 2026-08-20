using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.DTOs.Auth;
using SupportTicketAPI.Models;
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
            {
                return ServiceResult<int>.ValidationFailure("Invalid request.");
            }

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return ServiceResult<int>.ValidationFailure("Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return ServiceResult<int>.ValidationFailure("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return ServiceResult<int>.ValidationFailure("Password is required.");
            }

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
            {
                return ServiceResult<LoginResponse>.ValidationFailure("Invalid request.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return ServiceResult<LoginResponse>.ValidationFailure("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return ServiceResult<LoginResponse>.ValidationFailure("Password is required.");
            }


            User? user = await _userDataAccess.GetUserByEmailAsync(request.Email.Trim());


            if (user == null)
            {
                return ServiceResult<LoginResponse>.Unauthorized("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                return ServiceResult<LoginResponse>.Forbidden("This account is inactive.");
            }

            bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return ServiceResult<LoginResponse>.Unauthorized("Invalid email or password.");
            }


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
            {
                return ServiceResult<LoginResponse>.Failure(saveRefreshTokenResult.Message);
            }

            LoginResponse loginResponse = new()
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

        public async Task<ServiceResult<int>> CreateAgentAsync(int adminId, CreateAgentRequest request)
        {
            if (adminId <= 0)
            {
                return ServiceResult<int>.Unauthorized("Invalid admin id.");
            }

            if (request == null)
            {
                return ServiceResult<int>.ValidationFailure("Invalid request.");
            }

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return ServiceResult<int>.ValidationFailure("Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return ServiceResult<int>.ValidationFailure("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return ServiceResult<int>.ValidationFailure("Password is required.");
            }

            string passwordHash = _passwordHasher.HashPassword(request.Password);

            return await _userDataAccess.CreateAgentAsync(
                adminId,
                request.FullName.Trim(),
                request.Email.Trim(),
                passwordHash
            );
        }

        public async Task<ServiceResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (request == null)
            {
                return ServiceResult<LoginResponse>.ValidationFailure("Invalid request.");
            }

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return ServiceResult<LoginResponse>.ValidationFailure("Refresh token is required.");
            }

            string oldRefreshTokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

            DateTime newRefreshTokenExpires = _tokenService.GetRefreshTokenExpiration();
            string newRefreshToken = _tokenService.GenerateRefreshToken();
            string newRefreshTokenHash = _tokenService.HashRefreshToken(newRefreshToken);

            var rotateResult = await _userDataAccess.RotateRefreshTokenAsync(
                oldRefreshTokenHash,
                newRefreshTokenHash,
                newRefreshTokenExpires
            );

            if (!rotateResult.IsSuccess)
            {
                return ServiceResult<LoginResponse>.Failure(
                    rotateResult.Message,
                    rotateResult.ResultType);
            }

            RefreshTokenRotationResult rotationResult = rotateResult.Data!;

            User user = new User
            {
                UserId = rotationResult.UserId,
                FullName = rotationResult.FullName,
                Email = rotationResult.Email,
                Role = rotationResult.Role,
                IsActive = true
            };

            DateTime accessTokenExpiresAt = _tokenService.GetAccessTokenExpiration();
            string accessToken = _tokenService.GenerateAccessToken(user, accessTokenExpiresAt);

            LoginResponse response = new()
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,

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
            {
                return ServiceResult<int>.ValidationFailure("Invalid request.");
            }

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return ServiceResult<int>.ValidationFailure("Refresh token is required.");
            }

            string refreshTokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

            var storedRefreshToken = await _userDataAccess.GetRefreshTokenAsync(refreshTokenHash);

            if (storedRefreshToken == null)
            {
                return ServiceResult<int>.Unauthorized("Invalid refresh token.");
            }

            var revokeResult = await _userDataAccess.RevokeRefreshTokenAsync(refreshTokenHash);

            if (!revokeResult.IsSuccess)
            {
                return ServiceResult<int>.Failure(revokeResult.Message);
            }

            return ServiceResult<int>.Success(storedRefreshToken.UserId, "Logged out successfully.");
        }

    }
}
