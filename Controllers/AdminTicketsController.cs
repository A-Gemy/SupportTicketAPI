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
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminTicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public AdminTicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            string? userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId) && userId > 0;
        }



        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AdminGetAllTickets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return this.ToErrorResponse<PagedResult<Ticket>>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result =
                await _ticketService.AdminGetAllTicketsAsync(
                    adminId,
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



        [HttpGet("admin/{ticketId:int}")]
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
        public async Task<IActionResult> AdminGetTicketDetails(
            int ticketId)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return this.ToErrorResponse<Ticket>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result =
                await _ticketService.AdminGetTicketDetailsAsync(
                    adminId,
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



        [HttpGet("unassigned")]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AdminGetUnassignedTickets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return this.ToErrorResponse<PagedResult<Ticket>>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result =
                await _ticketService.AdminGetUnassignedTicketsAsync(
                    adminId,
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



        [HttpPatch("{ticketId:int}/assign")]
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
        public async Task<IActionResult> AssignTicketToAgent(
            int ticketId,
            [FromBody] AssignTicketRequest request)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return this.ToErrorResponse<object>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result =
                await _ticketService.AssignTicketToAgentAsync(
                    adminId,
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



        [HttpPatch("{ticketId:int}/status")]
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
        public async Task<IActionResult> AdminUpdateTicketStatus(
            int ticketId,
            [FromBody] UpdateTicketStatusRequest request)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return this.ToErrorResponse<object>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result =
                await _ticketService.AdminUpdateTicketStatusAsync(
                    adminId,
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



        [HttpGet("assigned-to-agent/{agentId:int}")]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AdminGetTicketsByAgent(
            int agentId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return this.ToErrorResponse<PagedResult<Ticket>>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result = await _ticketService.AdminGetTicketsByAgentAsync(
                adminId,
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


    }
}
