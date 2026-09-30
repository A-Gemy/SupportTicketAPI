using Microsoft.Data.SqlClient;
using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.Models;
using System.Data;

namespace SupportTicketAPI.DataAccess
{
    public class AuditLogDataAccess : IAuditLogDataAccess
    {
        private readonly string _connectionString;

        public AuditLogDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
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
            PagedResult<AuditLog> pagedResult = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AdminGetAuditLogs", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AdminId", SqlDbType.Int)
                .Value = adminId;

            command.Parameters.Add("@Action", SqlDbType.NVarChar, 100)
                .Value = (object?)action ?? DBNull.Value;

            command.Parameters.Add("@ActorUserId", SqlDbType.Int)
                .Value = (object?)actorUserId ?? DBNull.Value;

            command.Parameters.Add("@EntityName", SqlDbType.NVarChar, 100)
                .Value = (object?)entityName ?? DBNull.Value;

            command.Parameters.Add("@EntityId", SqlDbType.Int)
                .Value = (object?)entityId ?? DBNull.Value;

            command.Parameters.Add("@FromDate", SqlDbType.DateTime2)
                .Value = (object?)fromDate ?? DBNull.Value;

            command.Parameters.Add("@ToDate", SqlDbType.DateTime2)
                .Value = (object?)toDate ?? DBNull.Value;

            command.Parameters.Add("@PageNumber", SqlDbType.Int)
                .Value = pageNumber;

            command.Parameters.Add("@PageSize", SqlDbType.Int)
                .Value = pageSize;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<PagedResult<AuditLog>>.Failure("Failed to retrieve audit logs.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.AdminNotFoundOrInactive =>
                        ServiceResult<PagedResult<AuditLog>>.Forbidden(
                            "Admin not found or inactive."),

                    DatabaseResultCodes.InvalidDateRange =>
                        ServiceResult<PagedResult<AuditLog>>.ValidationFailure(
                            "FromDate cannot be later than ToDate."),

                    DatabaseResultCodes.InvalidPageNumber =>
                        ServiceResult<PagedResult<AuditLog>>.ValidationFailure(
                            "Page number must be greater than or equal to 1."),

                    DatabaseResultCodes.InvalidPageSize =>
                        ServiceResult<PagedResult<AuditLog>>.ValidationFailure(
                            "Page size must be between 1 and 100."),

                    _ =>
                        ServiceResult<PagedResult<AuditLog>>.Failure(
                            "Failed to retrieve audit logs.")
                };
            }

            pagedResult.TotalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
            pagedResult.PageNumber = reader.GetInt32(reader.GetOrdinal("PageNumber"));
            pagedResult.PageSize = reader.GetInt32(reader.GetOrdinal("PageSize"));

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    AuditLog auditLog = new()
                    {
                        AuditLogId = reader.GetInt32(reader.GetOrdinal("AuditLogId")),

                        UserId = reader.IsDBNull(reader.GetOrdinal("UserId"))
                            ? null
                            : reader.GetInt32(reader.GetOrdinal("UserId")),

                        ActorFullName = reader.IsDBNull(reader.GetOrdinal("ActorFullName"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("ActorFullName")),

                        ActorRole = reader.IsDBNull(reader.GetOrdinal("ActorRole"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("ActorRole")),

                        Action = reader.GetString(reader.GetOrdinal("Action")),

                        EntityName = reader.IsDBNull(reader.GetOrdinal("EntityName"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("EntityName")),

                        EntityId = reader.IsDBNull(reader.GetOrdinal("EntityId"))
                            ? null
                            : reader.GetInt32(reader.GetOrdinal("EntityId")),

                        Details = reader.IsDBNull(reader.GetOrdinal("Details"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Details")),

                        IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("IpAddress")),

                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    };

                    pagedResult.Items.Add(auditLog);
                }
            }

            return ServiceResult<PagedResult<AuditLog>>.Success(pagedResult, "Audit logs retrieved successfully.");
        }

        public async Task<ServiceResult<int>> AddAuditLogAsync(
            int? userId,
            string action,
            string? entityName = null,
            int? entityId = null,
            string? details = null,
            string? ipAddress = null)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AddAuditLog", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.Int)
                .Value = (object?)userId ?? DBNull.Value;

            command.Parameters.Add("@Action", SqlDbType.NVarChar, 100)
                .Value = action;

            command.Parameters.Add("@EntityName", SqlDbType.NVarChar, 100)
                .Value = (object?)entityName ?? DBNull.Value;

            command.Parameters.Add("@EntityId", SqlDbType.Int)
                .Value = (object?)entityId ?? DBNull.Value;

            command.Parameters.Add("@Details", SqlDbType.NVarChar, 1000)
                .Value = (object?)details ?? DBNull.Value;

            command.Parameters.Add("@IpAddress", SqlDbType.NVarChar, 50)
                .Value = (object?)ipAddress ?? DBNull.Value;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
                string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

                if (!isSuccess)
                {
                    return resultCode switch
                    {
                        DatabaseResultCodes.ActionRequired =>
                            ServiceResult<int>.Failure(
                                "Action is required."),

                        DatabaseResultCodes.UserNotFound =>
                            ServiceResult<int>.Failure(
                                "User not found."),

                        _ =>
                            ServiceResult<int>.Failure(
                                "Failed to add audit log.")
                    };
                }

                int auditLogId = reader.GetInt32(reader.GetOrdinal("AuditLogId"));

                return ServiceResult<int>.Success(auditLogId, "Audit log added successfully.");
            }

            return ServiceResult<int>.Failure("Failed to add audit log.");
        }

    }
}
