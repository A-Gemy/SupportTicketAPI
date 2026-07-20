using SupportTicketAPI.Common;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.DataAccess.Interfaces
{
    public interface IAuditLogDataAccess
    {
        Task<ServiceResult<PagedResult<AuditLog>>> AdminGetAuditLogsAsync(
            int adminId,
            string? action = null,
            int? actorUserId = null,
            string? entityName = null,
            int? entityId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 10);

        Task<ServiceResult<int>> AddAuditLogAsync(
            int? userId,
            string action,
            string? entityName = null,
            int? entityId = null,
            string? details = null,
            string? ipAddress = null);

    }
}
