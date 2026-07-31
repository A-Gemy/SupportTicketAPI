using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SupportTicketAPI.Constants;
using SupportTicketAPI.DTOs.Auth;
using SupportTicketAPI.DTOs.Common;
using SupportTicketAPI.Services.Interfaces;
using System.Security.Claims;

namespace SupportTicketAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IAuditLogService auditLogService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _auditLogService = auditLogService;
            _logger = logger;
        }



        [EnableRateLimiting(RateLimitingPolicies.Auth)]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterCustomerAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    result.IsSuccess,
                    result.Message
                });
            }

            return Ok(new
            {
                result.IsSuccess,
                result.Message,
                UserId = result.Data
            });
        }



        [EnableRateLimiting(RateLimitingPolicies.Auth)]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.IsSuccess)
            {
                if (ShouldLogFailedLogin(result.Message))
                {
                    await AddSecurityAuditLogAsync(
                        userId: null,
                        action: "FailedLogin",
                        details: "Failed login attempt.");
                }

                return BadRequest(new
                {
                    result.IsSuccess,
                    result.Message
                });
            }

            await AddSecurityAuditLogAsync(
                userId: result.Data!.UserId,
                action: "UserLoggedIn",
                details: "User logged in successfully.");

            return Ok(new
            {
                result.IsSuccess,
                result.Message,
                Data = result.Data
            });
        }



        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("agents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateAgent(CreateAgentRequest request)
        {
            var result = await _authService.CreateAgentAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    result.IsSuccess,
                    result.Message
                });
            }

            return Ok(new
            {
                result.IsSuccess,
                result.Message,
                UserId = result.Data
            });
        }



        [EnableRateLimiting(RateLimitingPolicies.Auth)]
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    result.IsSuccess,
                    result.Message
                });
            }

            return Ok(new
            {
                result.IsSuccess,
                result.Message,
                Data = result.Data
            });
        }



        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout(RefreshTokenRequest request)
        {
            var result = await _authService.LogoutAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    result.IsSuccess,
                    result.Message
                });
            }

            await AddSecurityAuditLogAsync(
                userId: result.Data,
                action: "UserLoggedOut",
                details: "User logged out successfully.");

            return Ok(new
            {
                result.IsSuccess,
                result.Message
            });
        }



        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(
            typeof(ApiResponse<CurrentUserResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? fullName = User.FindFirstValue(ClaimTypes.Name);
            string? email = User.FindFirstValue(ClaimTypes.Email);
            string? role = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(userIdClaim, out var userId) ||
                string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(role)
                )
            {
                return Unauthorized(
                    ApiResponse<CurrentUserResponse>.Failure(
                        "Invalid user token."));
            }

            CurrentUserResponse currentUser = new()
            {
                UserId = userId,
                FullName = fullName,
                Email = email,
                Role = role
            };

            return Ok(
                ApiResponse<CurrentUserResponse>.Success(
                    currentUser,
                    "Current user retrieved successfully."));
        }



        private async Task AddSecurityAuditLogAsync(
            int? userId,
            string action,
            string details)
        {
            try
            {
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var result = await _auditLogService.AddAuditLogAsync(
                    userId,
                    action,
                    entityName: "Auth",
                    entityId: null,
                    details,
                    ipAddress);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to add security audit log for action {Action}. Message: {Message}",
                        action,
                        result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to add security audit log. Action: {Action}",
                    action);
            }
        }

        private static bool ShouldLogFailedLogin(string message)
        {
            return message == "Invalid email or password." ||
                   message == "This account is inactive.";
        }

    }
}
