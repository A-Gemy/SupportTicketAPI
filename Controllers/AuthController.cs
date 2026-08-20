using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SupportTicketAPI.Common;
using SupportTicketAPI.Constants;
using SupportTicketAPI.DTOs.Auth;
using SupportTicketAPI.DTOs.Common;
using SupportTicketAPI.Extensions;
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
        [ProducesResponseType(
            typeof(ApiResponse<UserCreatedResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ApiResponse<UserCreatedResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<UserCreatedResponse>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterCustomerAsync(request);

            if (!result.IsSuccess)
            {
                return this.ToErrorResponse<UserCreatedResponse>(
                        result.ResultType,
                        result.Message);
            }

            UserCreatedResponse userCreated = new()
            {
                UserId = result.Data
            };

            return StatusCode(
                        StatusCodes.Status201Created,
                        ApiResponse<UserCreatedResponse>.Success(
                            userCreated, result.Message));
        }



        [EnableRateLimiting(RateLimitingPolicies.Auth)]
        [HttpPost("login")]
        [ProducesResponseType(
            typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<LoginResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<LoginResponse>), StatusCodes.Status500InternalServerError)]
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

                return this.ToErrorResponse<LoginResponse>(
                            result.ResultType,
                            result.Message);
            }

            await AddSecurityAuditLogAsync(
                userId: result.Data!.UserId,
                action: "UserLoggedIn",
                details: "User logged in successfully.");

            return Ok(
                ApiResponse<LoginResponse>.Success(
                    result.Data,
                    result.Message));
        }



        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("agents")]
        [ProducesResponseType(
            typeof(ApiResponse<UserCreatedResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ApiResponse<UserCreatedResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<UserCreatedResponse>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            typeof(ApiResponse<UserCreatedResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAgent(CreateAgentRequest request)
        {
            string? adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return this.ToErrorResponse<UserCreatedResponse>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result = await _authService.CreateAgentAsync(adminId, request);

            if (!result.IsSuccess)
            {
                return this.ToErrorResponse<UserCreatedResponse>(
                        result.ResultType,
                        result.Message);
            }

            UserCreatedResponse createdUser = new()
            {
                UserId = result.Data
            };

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<UserCreatedResponse>.Success(
                    createdUser,
                    result.Message));
        }



        [EnableRateLimiting(RateLimitingPolicies.Auth)]
        [HttpPost("refresh-token")]
        [ProducesResponseType(
            typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<LoginResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<LoginResponse>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            if (!result.IsSuccess)
            {
                return this.ToErrorResponse<LoginResponse>(
                            result.ResultType,
                            result.Message);
            }

            return Ok(
                ApiResponse<LoginResponse>.Success(
                    result.Data,
                    result.Message));
        }



        [HttpPost("logout")]
        [ProducesResponseType(
            typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout(RefreshTokenRequest request)
        {
            var result = await _authService.LogoutAsync(request);

            if (!result.IsSuccess)
            {
                return this.ToErrorResponse<object>(
                    result.ResultType,
                    result.Message);
            }

            await AddSecurityAuditLogAsync(
                userId: result.Data,
                action: "UserLoggedOut",
                details: "User logged out successfully.");

            return Ok(ApiResponse<object>.Success(
                data: null,
                result.Message));
        }



        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(
            typeof(ApiResponse<CurrentUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<CurrentUserResponse>), StatusCodes.Status401Unauthorized)]
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
                return this.ToErrorResponse<CurrentUserResponse>(
                            ResultType.Unauthorized,
                            "Invalid user claims.");
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
