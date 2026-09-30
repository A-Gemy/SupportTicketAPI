using Microsoft.Data.SqlClient;
using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.Models;
using System.Data;

namespace SupportTicketAPI.DataAccess
{
    public class AdminTicketDataAccess : IAdminTicketDataAccess
    {
        private readonly string _connectionString;

        public AdminTicketDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        }


        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetAllTicketsAsync(
            int adminId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            PagedResult<Ticket> pagedResult = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AdminGetAllTickets", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AdminId", SqlDbType.Int)
                .Value = adminId;

            command.Parameters.Add("@PageNumber", SqlDbType.Int)
                .Value = pageNumber;

            command.Parameters.Add("@PageSize", SqlDbType.Int)
                .Value = pageSize;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<PagedResult<Ticket>>.Failure("Failed to retrieve tickets.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.AdminNotFoundOrInactive =>
                        ServiceResult<PagedResult<Ticket>>.Forbidden(
                            "Admin not found or inactive."),

                    DatabaseResultCodes.InvalidPageNumber =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(
                            "Page number must be greater than or equal to 1."),

                    DatabaseResultCodes.InvalidPageSize =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(
                            "Page size must be between 1 and 100."),

                    _ =>
                        ServiceResult<PagedResult<Ticket>>.Failure(
                            "Failed to retrieve tickets.")
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

            return ServiceResult<PagedResult<Ticket>>.Success(pagedResult, "Tickets retrieved successfully.");
        }


        public async Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(
            int adminId,
            int ticketId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AdminGetTicketDetails", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AdminId", SqlDbType.Int)
                .Value = adminId;

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
                    DatabaseResultCodes.AdminNotFoundOrInactive =>
                        ServiceResult<Ticket>.Forbidden(
                            "Admin not found or inactive."),

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


        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetUnassignedTicketsAsync(
            int adminId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            PagedResult<Ticket> pagedResult = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AdminGetUnassignedTickets", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AdminId", SqlDbType.Int)
                .Value = adminId;

            command.Parameters.Add("@PageNumber", SqlDbType.Int)
                .Value = pageNumber;

            command.Parameters.Add("@PageSize", SqlDbType.Int)
                .Value = pageSize;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<PagedResult<Ticket>>.Failure("Failed to retrieve unassigned tickets.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.AdminNotFoundOrInactive =>
                        ServiceResult<PagedResult<Ticket>>.Forbidden(
                            "Admin not found or inactive."),

                    DatabaseResultCodes.InvalidPageNumber =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(
                            "Page number must be greater than or equal to 1."),

                    DatabaseResultCodes.InvalidPageSize =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(
                            "Page size must be between 1 and 100."),

                    _ =>
                        ServiceResult<PagedResult<Ticket>>.Failure(
                            "Failed to retrieve unassigned tickets.")
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

            return ServiceResult<PagedResult<Ticket>>.Success(pagedResult, "Unassigned tickets retrieved successfully.");
        }


        public async Task<ServiceResult<bool>> AssignTicketToAgentAsync(
            int adminId,
            int ticketId,
            int agentId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AssignTicketToAgent", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AdminId", SqlDbType.Int)
                .Value = adminId;

            command.Parameters.Add("@TicketId", SqlDbType.Int)
                .Value = ticketId;

            command.Parameters.Add("@AgentId", SqlDbType.Int)
                .Value = agentId;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<bool>.Failure("Failed to assign ticket to agent.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.AdminNotFoundOrInactive =>
                        ServiceResult<bool>.Forbidden(
                            "Admin not found or inactive."),

                    DatabaseResultCodes.TicketNotFound =>
                        ServiceResult<bool>.NotFound(
                            "Ticket not found."),

                    DatabaseResultCodes.AgentNotFoundOrInactive =>
                        ServiceResult<bool>.NotFound(
                            "Agent not found or inactive."),

                    DatabaseResultCodes.ClosedTicketCannotBeAssigned =>
                        ServiceResult<bool>.Conflict(
                            "Closed tickets cannot be assigned."),

                    _ =>
                        ServiceResult<bool>.Failure(
                            "Failed to assign ticket to agent.")
                };
            }

            string successMessage =
                resultCode == DatabaseResultCodes.TicketAlreadyAssignedToAgent
                    ? "Ticket is already assigned to this agent."
                    : "Ticket assigned to agent successfully.";

            return ServiceResult<bool>.Success(true, successMessage);
        }


        public async Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(
            int adminId,
            int ticketId,
            string status)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AdminUpdateTicketStatus", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AdminId", SqlDbType.Int)
                .Value = adminId;

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
                    DatabaseResultCodes.AdminNotFoundOrInactive =>
                        ServiceResult<bool>.Forbidden(
                            "Admin not found or inactive."),

                    DatabaseResultCodes.TicketNotFound =>
                        ServiceResult<bool>.NotFound(
                            "Ticket not found."),

                    DatabaseResultCodes.InvalidTicketStatus =>
                        ServiceResult<bool>.ValidationFailure(
                            "Invalid ticket status."),

                    DatabaseResultCodes.ClosedTicketCannotBeUpdated =>
                        ServiceResult<bool>.Conflict(
                            "Closed tickets cannot be updated."),

                    DatabaseResultCodes.AssignedTicketCannotBeMovedToOpen =>
                        ServiceResult<bool>.Conflict(
                            "An assigned ticket cannot be moved to Open."),

                    DatabaseResultCodes.TicketMustBeAssignedForStatus =>
                        ServiceResult<bool>.Conflict(
                            "The ticket must be assigned before using this status."),

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


        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetTicketsByAgentAsync(
            int adminId,
            int agentId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            PagedResult<Ticket> pagedResult = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AdminGetTicketsByAgent", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AdminId", SqlDbType.Int)
                .Value = adminId;

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
                return ServiceResult<PagedResult<Ticket>>.Failure(
                    "Failed to retrieve agent tickets.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.AdminNotFoundOrInactive =>
                        ServiceResult<PagedResult<Ticket>>.Forbidden(
                            "Admin not found or inactive."),

                    DatabaseResultCodes.AgentNotFoundOrInactive =>
                        ServiceResult<PagedResult<Ticket>>.NotFound(
                            "Agent not found or inactive."),

                    DatabaseResultCodes.InvalidPageNumber =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(
                            "Page number must be greater than or equal to 1."),

                    DatabaseResultCodes.InvalidPageSize =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(
                            "Page size must be between 1 and 100."),

                    _ =>
                        ServiceResult<PagedResult<Ticket>>.Failure(
                            "Failed to retrieve agent tickets.")
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

            return ServiceResult<PagedResult<Ticket>>.Success(pagedResult, "Agent tickets retrieved successfully.");
        }

    }
}
