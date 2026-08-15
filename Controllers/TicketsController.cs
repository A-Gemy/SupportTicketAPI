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
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IAuthorizationService _authorizationService;

        public TicketsController(ITicketService ticketService, IAuthorizationService authorizationService)
        {
            _ticketService = ticketService;
            _authorizationService = authorizationService;
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId) && userId > 0;
        }



        [Authorize(Roles = UserRoles.Customer)]
        [HttpPost]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCreatedResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCreatedResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCreatedResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCreatedResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCreatedResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
        {
            if (!TryGetCurrentUserId(out int customerId))
            {
                return this.ToErrorResponse<TicketCreatedResponse>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result = await _ticketService.CreateTicketAsync(customerId, request);

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



        [Authorize(Roles = UserRoles.Customer)]
        [HttpGet("my")]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<Ticket>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyTickets([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out int customerId))
            {
                return this.ToErrorResponse<PagedResult<Ticket>>(
                    ResultType.Unauthorized, "Invalid user token.");
            }

            var result = await _ticketService.GetCustomerTicketsAsync(customerId, pageNumber, pageSize);

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



        [Authorize(Roles = UserRoles.Customer)]
        [HttpGet("{ticketId:int}")]
        [ProducesResponseType(
            typeof(ApiResponse<Ticket>), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<Ticket>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<Ticket>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<Ticket>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketDetails(int ticketId)
        {
            if (!TryGetCurrentUserId(out int customerId))
            {
                return this.ToErrorResponse<Ticket>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result = await _ticketService.GetCustomerTicketDetailsAsync(customerId, ticketId);

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



        [Authorize(Roles = UserRoles.Customer)]
        [HttpPatch("{ticketId:int}/close")]
        [ProducesResponseType(
            typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CloseTicket(int ticketId)
        {
            if (!TryGetCurrentUserId(out int customerId))
            {
                return this.ToErrorResponse<object>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var result = await _ticketService.CloseCustomerTicketAsync(customerId, ticketId);

            if (!result.IsSuccess)
            {
                return this.ToErrorResponse<object>(
                    result.ResultType,
                    result.Message);
            }

            return Ok(ApiResponse<object>.Success(
                data: null,
                message: result.Message));
        }



        [Authorize]
        [HttpPost("{ticketId:int}/comments")]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCommentCreatedResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCommentCreatedResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCommentCreatedResponse>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            typeof(ApiResponse<TicketCommentCreatedResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddComment(int ticketId, [FromBody] AddTicketCommentRequest request)
        {
            if (!TryGetCurrentUserId(out int currentUserId))
            {
                return this.ToErrorResponse<TicketCommentCreatedResponse>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
            }

            var accessResult = await _ticketService.GetTicketAccessInfoAsync(ticketId);

            if (!accessResult.IsSuccess)
            {
                return this.ToErrorResponse<TicketCommentCreatedResponse>(
                    accessResult.ResultType,
                    accessResult.Message);
            }

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                accessResult.Data!,
                AuthorizationPolicies.CanAddTicketComment);

            if (!authorizationResult.Succeeded)
            {
                return this.ToErrorResponse<TicketCommentCreatedResponse>(
                    ResultType.Forbidden,
                    "You do not have permission to add comments to this ticket.");
            }

            var result = await _ticketService.AddTicketCommentAsync(currentUserId, ticketId, request);

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
                    createdComment, result.Message));
        }



        [Authorize]
        [HttpGet("{ticketId:int}/comments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetComments(int ticketId)
        {
            var accessResult = await _ticketService.GetTicketAccessInfoAsync(ticketId);

            if (!accessResult.IsSuccess || accessResult.Data == null)
            {
                return BadRequest(new
                {
                    accessResult.IsSuccess,
                    accessResult.Message
                });
            }

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                accessResult.Data,
                AuthorizationPolicies.CanViewTicketComments);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await _ticketService.GetTicketCommentsAsync(ticketId);

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
                Comments = result.Data
            });
        }



        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AdminGetAllTickets([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AdminGetAllTicketsAsync(adminId, pageNumber, pageSize);

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
                Tickets = result.Data!.Items,
                result.Data.PageNumber,
                result.Data.PageSize,
                result.Data.TotalCount,
                result.Data.TotalPages
            });
        }



        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("admin/{ticketId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AdminGetTicketDetails(int ticketId)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AdminGetTicketDetailsAsync(adminId, ticketId);

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



        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("unassigned")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AdminGetUnassignedTickets([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AdminGetUnassignedTicketsAsync(adminId, pageNumber, pageSize);

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
                Tickets = result.Data!.Items,
                result.Data.PageNumber,
                result.Data.PageSize,
                result.Data.TotalCount,
                result.Data.TotalPages
            });
        }



        [Authorize(Roles = UserRoles.Admin)]
        [HttpPatch("{ticketId:int}/assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AssignTicketToAgent(int ticketId, [FromBody] AssignTicketRequest request)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AssignTicketToAgentAsync(adminId, ticketId, request);

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



        [Authorize(Roles = UserRoles.Admin)]
        [HttpPatch("{ticketId:int}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AdminUpdateTicketStatus(int ticketId, [FromBody] UpdateTicketStatusRequest request)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AdminUpdateTicketStatusAsync(adminId, ticketId, request);

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



        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("assigned-to-agent/{agentId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AdminGetTicketsByAgent(int agentId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AdminGetTicketsByAgentAsync(
                adminId,
                agentId,
                pageNumber,
                pageSize);

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
                Tickets = result.Data!.Items,
                result.Data.PageNumber,
                result.Data.PageSize,
                result.Data.TotalCount,
                result.Data.TotalPages
            });
        }



        [Authorize(Roles = UserRoles.Agent)]
        [HttpGet("assigned-to-me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AgentGetAssignedTickets([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out int agentId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AgentGetAssignedTicketsAsync(agentId, pageNumber, pageSize);

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
                Tickets = result.Data!.Items,
                result.Data.PageNumber,
                result.Data.PageSize,
                result.Data.TotalCount,
                result.Data.TotalPages
            });
        }



        [Authorize(Roles = UserRoles.Agent)]
        [HttpGet("assigned-to-me/{ticketId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AgentGetAssignedTicketDetails(int ticketId)
        {
            if (!TryGetCurrentUserId(out int agentId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AgentGetAssignedTicketDetailsAsync(agentId, ticketId);

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



        [Authorize(Roles = UserRoles.Agent)]
        [HttpPatch("assigned-to-me/{ticketId:int}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AgentUpdateAssignedTicketStatus(int ticketId, [FromBody] UpdateTicketStatusRequest request)
        {
            if (!TryGetCurrentUserId(out int agentId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AgentUpdateAssignedTicketStatusAsync(agentId, ticketId, request);

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

    }
}