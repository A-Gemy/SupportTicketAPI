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
        private readonly IAuthorizationService _authorizationService;

        public TicketsController(ITicketService ticketService, IAuthorizationService authorizationService)
        {
            _ticketService = ticketService;
            _authorizationService = authorizationService;
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



        [Authorize]
        [HttpPost("{ticketId:int}/comments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddComment(int ticketId, [FromBody] AddTicketCommentRequest request)
        {
            if (!TryGetCurrentUserId(out int currentUserId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var accessResult = await _ticketService.GetTicketAccessInfoAsync(ticketId);

            if (!accessResult.IsSuccess ||
                accessResult.Data == null)
            {
                return BadRequest(new
                {
                    accessResult.IsSuccess,
                    accessResult.Message
                });
            }

            // Return a clear business error instead of a generic 403.
            if (accessResult.Data.Status == "Closed")
            {
                return BadRequest(new
                {
                    IsSuccess = false,
                    Message =
                        "Comments cannot be added to a closed ticket."
                });
            }

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                accessResult.Data,
                AuthorizationPolicies.CanAddTicketComment);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await _ticketService.AddTicketCommentAsync(currentUserId, ticketId, request);

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
        public async Task<IActionResult> AdminGetAllTickets()
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AdminGetAllTicketsAsync(adminId);

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
        public async Task<IActionResult> AdminGetUnassignedTickets()
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _ticketService.AdminGetUnassignedTicketsAsync(adminId);

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
        public async Task<IActionResult> AdminGetTicketsByAgent(int agentId)
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
                agentId);

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



        [Authorize(Roles = UserRoles.Agent)]
        [HttpGet("assigned-to-me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AgentGetAssignedTickets()
        {
            if (!TryGetCurrentUserId(out int agentId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }
            var result = await _ticketService.AgentGetAssignedTicketsAsync(agentId);

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

    }
}