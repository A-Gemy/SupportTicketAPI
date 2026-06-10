using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketAPI.Constants;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Services.Interfaces;
using System.Security.Claims;

namespace SupportTicketAPI.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId);
        }



        [Authorize(Roles = UserRoles.Customer)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    IsSuccess = false,
                    Message = "Invalid request."
                });
            }

            if (!TryGetCurrentUserId(out int customerId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.CreateTicketAsync(customerId, request);

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
                TicketId = result.Data
            });
        }



        [Authorize(Roles = UserRoles.Customer)]
        [HttpGet("my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyTickets()
        {
            if (!TryGetCurrentUserId(out int customerId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.GetCustomerTicketsAsync(customerId);

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
                Tickets = result.Data
            });
        }



        [Authorize(Roles = UserRoles.Customer)]
        [HttpGet("{ticketId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTicketDetails(int ticketId)
        {
            if (!TryGetCurrentUserId(out int customerId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.GetCustomerTicketDetailsAsync(customerId, ticketId);

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
                Ticket = result.Data
            });
        }



        [Authorize(Roles = UserRoles.Customer)]
        [HttpPatch("{ticketId:int}/close")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CloseTicket(int ticketId)
        {
            if (!TryGetCurrentUserId(out int customerId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.CloseCustomerTicketAsync(customerId, ticketId);

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
                result.Message
            });
        }



        [Authorize(Roles = UserRoles.Customer)]
        [HttpPost("{ticketId:int}/comments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddComment(int ticketId, [FromBody] AddTicketCommentRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    IsSuccess = false,
                    Message = "Invalid request."
                });
            }

            if (!TryGetCurrentUserId(out int customerId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AddCustomerTicketCommentAsync(customerId, ticketId, request);

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
                CommentId = result.Data
            });
        }


    }
}