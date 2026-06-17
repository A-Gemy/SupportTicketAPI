using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.Models;
using SupportTicketAPI.Services.Interfaces;

namespace SupportTicketAPI.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogDataAccess _auditLogDataAccess;

        public AuditLogService(IAuditLogDataAccess auditLogDataAccess)
        {
            _auditLogDataAccess = auditLogDataAccess;
        }


        public async Task<ServiceResult<List<AuditLog>>> AdminGetAuditLogsAsync(
            int adminId,
            string? action = null,
            int? actorUserId = null,
            string? entityName = null,
            int? entityId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            if (adminId <= 0)
            {
                return ServiceResult<List<AuditLog>>.Failure("Invalid admin id.");
            }

            if (actorUserId.HasValue && actorUserId.Value <= 0)
            {
                return ServiceResult<List<AuditLog>>.Failure("Invalid actor user id.");
            }

            if (entityId.HasValue && entityId.Value <= 0)
            {
                return ServiceResult<List<AuditLog>>.Failure("Invalid entity id.");
            }

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value > toDate.Value)
            {
                return ServiceResult<List<AuditLog>>.Failure("FromDate cannot be later than ToDate.");
            }

            string? normalizedAction = string.IsNullOrWhiteSpace(action)
                        ? null
                        : action.Trim();

            string? normalizedEntityName = string.IsNullOrWhiteSpace(entityName)
                        ? null
                        : entityName.Trim();

            if (normalizedAction?.Length > 100)
            {
                return ServiceResult<List<AuditLog>>.Failure("Action cannot exceed 100 characters.");
            }

            if (normalizedEntityName?.Length > 100)
            {
                return ServiceResult<List<AuditLog>>.Failure("Entity name cannot exceed 100 characters.");
            }

            return await _auditLogDataAccess.AdminGetAuditLogsAsync(
                adminId,
                normalizedAction,
                actorUserId,
                normalizedEntityName,
                entityId,
                fromDate,
                toDate);
        }

    }
}
