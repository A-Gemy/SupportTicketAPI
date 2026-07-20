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
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return ServiceResult<PagedResult<AuditLog>>.Failure(message);
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

                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    };

                    pagedResult.Items.Add(auditLog);
                }
            }

            return ServiceResult<PagedResult<AuditLog>>.Success(pagedResult, message);
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
                string message = reader.GetString(reader.GetOrdinal("Message"));

                int auditLogId = reader.IsDBNull(reader.GetOrdinal("AuditLogId"))
                    ? 0
                    : reader.GetInt32(reader.GetOrdinal("AuditLogId"));

                if (isSuccess)
                {
                    return ServiceResult<int>.Success(auditLogId, message);
                }

                return ServiceResult<int>.Failure(message);
            }

            return ServiceResult<int>.Failure("Failed to add audit log.");
        }

    }
}
