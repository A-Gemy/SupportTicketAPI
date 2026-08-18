using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketAPI.Common;
using SupportTicketAPI.Constants;
using SupportTicketAPI.DTOs.Common;
using SupportTicketAPI.Extensions;
using SupportTicketAPI.Models;
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
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<AuditLog>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<AuditLog>>), StatusCodes.Status500InternalServerError)]
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
                return this.ToErrorResponse<PagedResult<AuditLog>>(
                    ResultType.Unauthorized,
                    "Invalid user token.");
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
                return this.ToErrorResponse<PagedResult<AuditLog>>(
                    result.ResultType,
                    result.Message);
            }

            return Ok(
                ApiResponse<PagedResult<AuditLog>>.Success(
                    result.Data!,
                    result.Message));
        }


    }
}
