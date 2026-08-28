using Microsoft.Data.SqlClient;
using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.Models;
using System.Data;

namespace SupportTicketAPI.DataAccess
{
    public class AgentTicketDataAccess : IAgentTicketDataAccess
    {
        private readonly string _connectionString;

        public AgentTicketDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        }


        public async Task<ServiceResult<PagedResult<Ticket>>> AgentGetAssignedTicketsAsync(
            int agentId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            PagedResult<Ticket> pagedResult = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AgentGetAssignedTickets", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AgentId", SqlDbType.Int)
                .Value = agentId;

            command.Parameters.Add("@PageNumber", SqlDbType.Int)
                .Value = pageNumber;

            command.Parameters.Add("@PageSize", SqlDbType.Int)
                .Value = pageSize;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<PagedResult<Ticket>>.Failure("Failed to retrieve assigned tickets.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Agent not found or inactive." =>
                        ServiceResult<PagedResult<Ticket>>.Forbidden(message),

                    "Page number must be greater than or equal to 1." =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(message),

                    "Page size must be between 1 and 100." =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(message),

                    _ =>
                        ServiceResult<PagedResult<Ticket>>.Failure(message)
                };
            }

            pagedResult.TotalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
            pagedResult.PageNumber = reader.GetInt32(reader.GetOrdinal("PageNumber"));
            pagedResult.PageSize = reader.GetInt32(reader.GetOrdinal("PageSize"));

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    Ticket ticket = new()
                    {
                        TicketId = reader.GetInt32(reader.GetOrdinal("TicketId")),
                        CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                        CustomerFullName = reader.GetString(reader.GetOrdinal("CustomerFullName")),

                        AssignedAgentId = reader.IsDBNull(reader.GetOrdinal("AssignedAgentId"))
                            ? null
                            : reader.GetInt32(reader.GetOrdinal("AssignedAgentId")),

                        AssignedAgentFullName = reader.IsDBNull(reader.GetOrdinal("AssignedAgentFullName"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("AssignedAgentFullName")),

                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        Priority = reader.GetString(reader.GetOrdinal("Priority")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                        UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                            ? null
                            : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),

                        ClosedAt = reader.IsDBNull(reader.GetOrdinal("ClosedAt"))
                            ? null
                            : reader.GetDateTime(reader.GetOrdinal("ClosedAt"))
                    };

                    pagedResult.Items.Add(ticket);
                }
            }

            return ServiceResult<PagedResult<Ticket>>.Success(pagedResult, message);
        }


        public async Task<ServiceResult<Ticket>> AgentGetAssignedTicketDetailsAsync(
            int agentId,
            int ticketId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AgentGetAssignedTicketDetails", connection);
            command.CommandType = CommandType.StoredProcedure;


            command.Parameters.Add("@AgentId", SqlDbType.Int)
                .Value = agentId;

            command.Parameters.Add("@TicketId", SqlDbType.Int)
                .Value = ticketId;


            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<Ticket>.Failure("Failed to retrieve ticket details.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Agent not found or inactive." =>
                        ServiceResult<Ticket>.Forbidden(message),

                    "Ticket not found." =>
                        ServiceResult<Ticket>.NotFound(message),

                    _ =>
                        ServiceResult<Ticket>.Failure(message)
                };
            }

            if (!await reader.NextResultAsync() ||
                !await reader.ReadAsync())
            {
                return ServiceResult<Ticket>.Failure("Ticket details not found.");
            }

            Ticket ticket = new()
            {
                TicketId = reader.GetInt32(reader.GetOrdinal("TicketId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                CustomerFullName = reader.GetString(reader.GetOrdinal("CustomerFullName")),

                AssignedAgentId = reader.IsDBNull(reader.GetOrdinal("AssignedAgentId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("AssignedAgentId")),

                AssignedAgentFullName = reader.IsDBNull(reader.GetOrdinal("AssignedAgentFullName"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("AssignedAgentFullName")),

                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Priority = reader.GetString(reader.GetOrdinal("Priority")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),

                ClosedAt = reader.IsDBNull(reader.GetOrdinal("ClosedAt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("ClosedAt"))
            };

            return ServiceResult<Ticket>.Success(ticket, message);
        }


        public async Task<ServiceResult<bool>> AgentUpdateAssignedTicketStatusAsync(
            int agentId,
            int ticketId,
            string status)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AgentUpdateAssignedTicketStatus", connection);
            command.CommandType = CommandType.StoredProcedure;


            command.Parameters.Add("@AgentId", SqlDbType.Int)
                .Value = agentId;

            command.Parameters.Add("@TicketId", SqlDbType.Int)
                .Value = ticketId;

            command.Parameters.Add("@Status", SqlDbType.NVarChar, 50)
                .Value = status;


            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<bool>.Failure("Failed to update ticket status.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Agent not found or inactive." =>
                        ServiceResult<bool>.Forbidden(message),

                    "Ticket not found." =>
                        ServiceResult<bool>.NotFound(message),

                    "Invalid ticket status." =>
                        ServiceResult<bool>.ValidationFailure(message),

                    "Closed tickets cannot be updated." =>
                        ServiceResult<bool>.Conflict(message),

                    "Resolved tickets cannot be updated by the agent." =>
                        ServiceResult<bool>.Conflict(message),

                    "Only an assigned ticket can be moved to InProgress." =>
                        ServiceResult<bool>.Conflict(message),

                    "Ticket must be InProgress before it can be resolved." =>
                        ServiceResult<bool>.Conflict(message),

                    _ =>
                        ServiceResult<bool>.Failure(message)
                };
            }

            return ServiceResult<bool>.Success(true, message);
        }

    }
}
