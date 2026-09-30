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
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.AgentNotFoundOrInactive =>
                        ServiceResult<PagedResult<Ticket>>.Forbidden(
                            "Agent not found or inactive."),

                    DatabaseResultCodes.InvalidPageNumber =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(
                            "Page number must be greater than or equal to 1."),

                    DatabaseResultCodes.InvalidPageSize =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(
                            "Page size must be between 1 and 100."),

                    _ =>
                        ServiceResult<PagedResult<Ticket>>.Failure(
                            "Failed to retrieve assigned tickets.")
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

            return ServiceResult<PagedResult<Ticket>>.Success(pagedResult, "Assigned tickets retrieved successfully.");
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
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.AgentNotFoundOrInactive =>
                        ServiceResult<Ticket>.Forbidden(
                            "Agent not found or inactive."),

                    DatabaseResultCodes.TicketNotFound =>
                        ServiceResult<Ticket>.NotFound(
                            "Ticket not found."),

                    _ =>
                        ServiceResult<Ticket>.Failure(
                            "Failed to retrieve ticket details.")
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

            return ServiceResult<Ticket>.Success(ticket, "Ticket details retrieved successfully.");
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
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.AgentNotFoundOrInactive =>
                        ServiceResult<bool>.Forbidden(
                            "Agent not found or inactive."),

                    DatabaseResultCodes.TicketNotFound =>
                        ServiceResult<bool>.NotFound(
                            "Ticket not found."),

                    DatabaseResultCodes.InvalidTicketStatus =>
                        ServiceResult<bool>.ValidationFailure(
                            "Invalid ticket status."),

                    DatabaseResultCodes.ClosedTicketCannotBeUpdated =>
                        ServiceResult<bool>.Conflict(
                            "Closed tickets cannot be updated."),

                    DatabaseResultCodes.ResolvedTicketCannotBeUpdatedByAgent =>
                        ServiceResult<bool>.Conflict(
                            "Resolved tickets cannot be updated by the agent."),

                    DatabaseResultCodes.OnlyAssignedTicketCanMoveToInProgress =>
                        ServiceResult<bool>.Conflict(
                            "Only an assigned ticket can be moved to InProgress."),

                    DatabaseResultCodes.TicketMustBeInProgressBeforeResolved =>
                        ServiceResult<bool>.Conflict(
                            "Ticket must be InProgress before it can be resolved."),

                    _ =>
                        ServiceResult<bool>.Failure(
                            "Failed to update ticket status.")
                };
            }

            string successMessage =
                resultCode == DatabaseResultCodes.TicketAlreadyHasRequestedStatus
                    ? "Ticket already has the requested status."
                    : "Ticket status updated successfully.";

            return ServiceResult<bool>.Success(true, successMessage);
        }

    }
}
