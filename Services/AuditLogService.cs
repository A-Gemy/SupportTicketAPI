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


        public async Task<ServiceResult<PagedResult<AuditLog>>> AdminGetAuditLogsAsync(
            int adminId,
            string? action = null,
            int? actorUserId = null,
            string? entityName = null,
            int? entityId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (adminId <= 0)
            {
                return ServiceResult<PagedResult<AuditLog>>.Failure("Invalid admin id.");
            }

            if (actorUserId.HasValue && actorUserId.Value <= 0)
            {
                return ServiceResult<PagedResult<AuditLog>>.Failure("Invalid actor user id.");
            }

            if (entityId.HasValue && entityId.Value <= 0)
            {
                return ServiceResult<PagedResult<AuditLog>>.Failure("Invalid entity id.");
            }

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value > toDate.Value)
            {
                return ServiceResult<PagedResult<AuditLog>>.Failure("FromDate cannot be later than ToDate.");
            }

            if (pageNumber < 1)
            {
                return ServiceResult<PagedResult<AuditLog>>.Failure("Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return ServiceResult<PagedResult<AuditLog>>.Failure("Page size must be between 1 and 100.");
            }

            string? normalizedAction = string.IsNullOrWhiteSpace(action)
                        ? null
                        : action.Trim();

            string? normalizedEntityName = string.IsNullOrWhiteSpace(entityName)
                        ? null
                        : entityName.Trim();

            if (normalizedAction?.Length > 100)
            {
                return ServiceResult<PagedResult<AuditLog>>.Failure("Action cannot exceed 100 characters.");
            }

            if (normalizedEntityName?.Length > 100)
            {
                return ServiceResult<PagedResult<AuditLog>>.Failure("Entity name cannot exceed 100 characters.");
            }

            return await _auditLogDataAccess.AdminGetAuditLogsAsync(
                adminId,
                normalizedAction,
                actorUserId,
                normalizedEntityName,
                entityId,
                fromDate,
                toDate,
                pageNumber,
                pageSize);
        }

        public async Task<ServiceResult<int>> AddAuditLogAsync(
            int? userId,
            string action,
            string? entityName = null,
            int? entityId = null,
            string? details = null,
            string? ipAddress = null)
        {
            if (userId.HasValue && userId.Value <= 0)
            {
                return ServiceResult<int>.Failure("Invalid user id.");
            }

            if (string.IsNullOrWhiteSpace(action))
            {
                return ServiceResult<int>.Failure("Action is required.");
            }

            if (action.Trim().Length > 100)
            {
                return ServiceResult<int>.Failure("Action cannot exceed 100 characters.");
            }

            if (!string.IsNullOrWhiteSpace(entityName) &&
                entityName.Trim().Length > 100)
            {
                return ServiceResult<int>.Failure("Entity name cannot exceed 100 characters.");
            }

            if (entityId.HasValue && entityId.Value <= 0)
            {
                return ServiceResult<int>.Failure("Invalid entity id.");
            }

            if (!string.IsNullOrWhiteSpace(details) &&
                details.Trim().Length > 1000)
            {
                return ServiceResult<int>.Failure("Details cannot exceed 1000 characters.");
            }

            if (!string.IsNullOrWhiteSpace(ipAddress) &&
                ipAddress.Trim().Length > 50)
            {
                return ServiceResult<int>.Failure("IP address cannot exceed 50 characters.");
            }

            return await _auditLogDataAccess.AddAuditLogAsync(
                userId,
                action.Trim(),
                string.IsNullOrWhiteSpace(entityName) ? null : entityName.Trim(),
                entityId,
                string.IsNullOrWhiteSpace(details) ? null : details.Trim(),
                string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim());
        }

    }
}
