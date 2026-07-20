using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketAPI.Constants;
using SupportTicketAPI.Services.Interfaces;
using System.Security.Claims;

namespace SupportTicketAPI.Controllers
{
    [ApiController]
    [Route("api/admin/audit-logs")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId);
        }



        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] string? action = null,
            [FromQuery] int? actorUserId = null,
            [FromQuery] string? entityName = null,
            [FromQuery] int? entityId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!TryGetCurrentUserId(out int adminId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Message = "Invalid user token."
                });
            }

            var result = await _auditLogService.AdminGetAuditLogsAsync(
                adminId,
                action: action,
                actorUserId: actorUserId,
                entityName: entityName,
                entityId: entityId,
                fromDate: fromDate,
                toDate: toDate,
                pageNumber: pageNumber,
                pageSize: pageSize);

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
                AuditLogs = result.Data!.Items,
                result.Data.PageNumber,
                result.Data.PageSize,
                result.Data.TotalCount,
                result.Data.TotalPages,
            });
        }


    }
}
