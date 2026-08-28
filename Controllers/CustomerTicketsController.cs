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
    [Authorize(Roles = UserRoles.Customer)]
    public class CustomerTicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public CustomerTicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            string? userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId) && userId > 0;
        }



        [HttpPost]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCreatedResponse>),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCreatedResponse>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCreatedResponse>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCreatedResponse>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCreatedResponse>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTicket(
            [FromBody] CreateTicketRequest request)
        {
            if (!TryGetCurrentUserId(out int customerId))
            {
                return this.ToErrorResponse<TicketCreatedResponse>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result =
                await _ticketService.CreateTicketAsync(
                    customerId,
                    request);

            if (!result.IsSuccess)
            {
                return this.ToErrorResponse<TicketCreatedResponse>(
                    result.ResultType,
                    result.Message);
            }

            TicketCreatedResponse createdTicket = new()
            {
                TicketId = result.Data
            };

            return CreatedAtAction(
                nameof(GetTicketDetails),
                new
                {
                    ticketId = createdTicket.TicketId,
                },
                ApiResponse<TicketCreatedResponse>.Success(
                    createdTicket,
                    result.Message));
        }



        [HttpGet("my")]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyTickets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out int customerId))
            {
                return this.ToErrorResponse<PagedResult<Ticket>>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result =
                await _ticketService.GetCustomerTicketsAsync(
                    customerId,
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



        [HttpGet("{ticketId:int}")]
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
        public async Task<IActionResult> GetTicketDetails(
            int ticketId)
        {
            if (!TryGetCurrentUserId(out int customerId))
            {
                return this.ToErrorResponse<Ticket>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result =
                await _ticketService.GetCustomerTicketDetailsAsync(
                    customerId,
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



        [HttpPatch("{ticketId:int}/close")]
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
        public async Task<IActionResult> CloseTicket(
            int ticketId)
        {
            if (!TryGetCurrentUserId(out int customerId))
            {
                return this.ToErrorResponse<object>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result =
                await _ticketService.CloseCustomerTicketAsync(
                    customerId,
                    ticketId);

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
