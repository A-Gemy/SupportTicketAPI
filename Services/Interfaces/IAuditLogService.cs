using SupportTicketAPI.Common;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<ServiceResult<List<AuditLog>>> AdminGetAuditLogsAsync(
            int adminId,
            string? action = null,
            int? actorUserId = null,
            string? entityName = null,
            int? entityId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);
    }
}
