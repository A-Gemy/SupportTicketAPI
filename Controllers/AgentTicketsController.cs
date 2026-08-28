using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketAPI.Common;
using SupportTicketAPI.Constants;
using SupportTicketAPI.DTOs.Common;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Extensions;
using SupportTicketAPI.Models;
using SupportTicketAPI.Services.Interfaces;
using System.Security.Claims;

namespace SupportTicketAPI.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    [Authorize(Roles = UserRoles.Agent)]
    public class AgentTicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public AgentTicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            string? userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId) && userId > 0;
        }



        [HttpGet("assigned-to-me")]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AgentGetAssignedTickets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out int agentId))
            {
                return this.ToErrorResponse<PagedResult<Ticket>>(
                   ResultType.Unauthorized,
                   "Invalid user token.");
            }

            var result =
                await _ticketService.AgentGetAssignedTicketsAsync(
                    agentId,
                    pageNumber,
                    pageSize);

            if (!result.IsSuccess)
            {
                return this.ToErrorResponse<PagedResult<Ticket>>(
                    result.ResultType,
                    result.Message);
            }

            return Ok(
                ApiResponse<PagedResult<Ticket>>.Success(
                    result.Data!,
                    result.Message));
        }



        [HttpGet("assigned-to-me/{ticketId:int}")]
        [ProducesResponseType(
            typeof(ApiResponse<Ticket>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<Ticket>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<Ticket>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<Ticket>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AgentGetAssignedTicketDetails(
            int ticketId)
        {
            if (!TryGetCurrentUserId(out int agentId))
            {
                return this.ToErrorResponse<Ticket>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result =
                await _ticketService.AgentGetAssignedTicketDetailsAsync(
                    agentId,
                    ticketId);

            if (!result.IsSuccess)
            {
                return this.ToErrorResponse<Ticket>(
                    result.ResultType,
                    result.Message);
            }

            return Ok(
                ApiResponse<Ticket>.Success(
                    result.Data!,
                    result.Message));
        }



        [HttpPatch("assigned-to-me/{ticketId:int}/status")]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AgentUpdateAssignedTicketStatus(
            int ticketId,
            [FromBody] UpdateTicketStatusRequest request)
        {
            if (!TryGetCurrentUserId(out int agentId))
            {
                return this.ToErrorResponse<object>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result =
                await _ticketService.AgentUpdateAssignedTicketStatusAsync(
                    agentId,
                    ticketId,
                    request);

            if (!result.IsSuccess)
            {
                return this.ToErrorResponse<object>(
                    result.ResultType,
                    result.Message);
            }

            return Ok(
                ApiResponse<object>.Success(
                    data: null,
                    message: result.Message));
        }


    }
}
