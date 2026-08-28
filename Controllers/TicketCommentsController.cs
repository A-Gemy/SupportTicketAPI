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
    [Authorize]
    public class TicketCommentsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IAuthorizationService _authorizationService;

        public TicketCommentsController(ITicketService ticketService, IAuthorizationService authorizationService)
        {
            _ticketService = ticketService;
            _authorizationService = authorizationService;
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            string? userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId) && userId > 0;
        }



        [HttpGet("{ticketId:int}/comments")]
        [ProducesResponseType(
            typeof(ApiResponse<List<TicketComment>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<List<TicketComment>>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<List<TicketComment>>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<List<TicketComment>>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetComments(
            int ticketId)
        {
            if (!TryGetCurrentUserId(out int currentUserId))
            {
                return this.ToErrorResponse<List<TicketComment>>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var accessResult =
                await _ticketService.GetTicketAccessInfoAsync(
                    ticketId);

            if (!accessResult.IsSuccess)
            {
                return this.ToErrorResponse<List<TicketComment>>(
                    accessResult.ResultType,
                    accessResult.Message);
            }

            AuthorizationResult authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    accessResult.Data!,
                    AuthorizationPolicies.CanViewTicketComments);

            if (!authorizationResult.Succeeded)
            {
                return this.ToErrorResponse<List<TicketComment>>(
                    ResultType.Forbidden,
                    "You do not have permission to view comments for this ticket.");
            }

            var result =
                await _ticketService.GetTicketCommentsAsync(
                    currentUserId,
                    ticketId);

            if (!result.IsSuccess)
            {
                return this.ToErrorResponse<List<TicketComment>>(
                    result.ResultType,
                    result.Message);
            }

            return Ok(
                ApiResponse<List<TicketComment>>.Success(
                    result.Data!,
                    result.Message));
        }




        [HttpPost("{ticketId:int}/comments")]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCommentCreatedResponse>),
            StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCommentCreatedResponse>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCommentCreatedResponse>),
            StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCommentCreatedResponse>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddComment(
            int ticketId,
            [FromBody] AddTicketCommentRequest request)
        {
            if (!TryGetCurrentUserId(out int currentUserId))
            {
                return this.ToErrorResponse<TicketCommentCreatedResponse>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var accessResult =
                await _ticketService.GetTicketAccessInfoAsync(
                    ticketId);

            if (!accessResult.IsSuccess)
            {
                return this.ToErrorResponse<TicketCommentCreatedResponse>(
                    accessResult.ResultType,
                    accessResult.Message);
            }

            AuthorizationResult authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    accessResult.Data!,
                    AuthorizationPolicies.CanAddTicketComment);

            if (!authorizationResult.Succeeded)
            {
                return this.ToErrorResponse<TicketCommentCreatedResponse>(
                    ResultType.Forbidden,
                    "You do not have permission to add comments to this ticket.");
            }

            var result =
                await _ticketService.AddTicketCommentAsync(
                    currentUserId,
                    ticketId,
                    request);

            if (!result.IsSuccess)
            {
                return this.ToErrorResponse<TicketCommentCreatedResponse>(
                    result.ResultType,
                    result.Message);
            }

            TicketCommentCreatedResponse createdComment = new()
            {
                CommentId = result.Data
            };

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<TicketCommentCreatedResponse>.Success(
                    createdComment,
                    result.Message));
        }



    }
}
