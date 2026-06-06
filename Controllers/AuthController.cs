using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketAPI.Constants;
using SupportTicketAPI.DTOs.Auth;
using SupportTicketAPI.Services.Interfaces;
using System.Security.Claims;

namespace SupportTicketAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }



        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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



        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

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



        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var fullName = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new
            {
                UserId = userId,
                FullName = fullName,
                Email = email,
                Role = role
            });
        }



        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("admin-test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult AdminTest()
        {
            return Ok(new
            {
                Message = "You are authorized as Admin."
            });
        }

    }
}
