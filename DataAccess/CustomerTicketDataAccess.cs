using Microsoft.Data.SqlClient;
using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.Models;
using System.Data;

namespace SupportTicketAPI.DataAccess
{
    public class CustomerTicketDataAccess : ICustomerTicketDataAccess
    {
        private readonly string _connectionString;

        public CustomerTicketDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        }


        public async Task<ServiceResult<int>> CreateTicketAsync(
            int customerId,
            string title,
            string description,
            string priority)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_CreateTicket", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@CustomerId", SqlDbType.Int)
                .Value = customerId;

            command.Parameters.Add("@Title", SqlDbType.NVarChar, 200)
                .Value = title;

            command.Parameters.Add("@Description", SqlDbType.NVarChar, 1000)
                .Value = description;

            command.Parameters.Add("@Priority", SqlDbType.NVarChar, 20)
                .Value = priority;

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
                        DatabaseResultCodes.CustomerNotFoundOrInactive =>
                            ServiceResult<int>.Forbidden(
                                "Customer not found or inactive."),

                        DatabaseResultCodes.InvalidPriority =>
                            ServiceResult<int>.ValidationFailure(
                                "Invalid priority."),

                        _ =>
                            ServiceResult<int>.Failure(
                                "Failed to create ticket.")
                    };
                }

                int ticketId = reader.GetInt32(reader.GetOrdinal("TicketId"));

                return ServiceResult<int>.Success(ticketId, "Ticket created successfully.");
            }

            return ServiceResult<int>.Failure("Failed to create ticket.");
        }


        public async Task<ServiceResult<PagedResult<Ticket>>> GetCustomerTicketsAsync(
            int customerId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            PagedResult<Ticket> pagedResult = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_GetCustomerTickets", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@CustomerId", SqlDbType.Int)
                .Value = customerId;

            command.Parameters.Add("@PageNumber", SqlDbType.Int)
                .Value = pageNumber;

            command.Parameters.Add("@PageSize", SqlDbType.Int)
                .Value = pageSize;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<PagedResult<Ticket>>.Failure("Failed to retrieve customer tickets.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.CustomerNotFoundOrInactive =>
                        ServiceResult<PagedResult<Ticket>>.Forbidden(
                            "Customer not found or inactive."),

                    DatabaseResultCodes.InvalidPageNumber =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(
                            "Page number must be greater than or equal to 1."),

                    DatabaseResultCodes.InvalidPageSize =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(
                            "Page size must be between 1 and 100."),

                    _ =>
                        ServiceResult<PagedResult<Ticket>>.Failure(
                            "Failed to retrieve customer tickets.")
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
                        AssignedAgentId = reader.IsDBNull(reader.GetOrdinal("AssignedAgentId"))
                            ? null
                            : reader.GetInt32(reader.GetOrdinal("AssignedAgentId")),
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

            return ServiceResult<PagedResult<Ticket>>.Success(pagedResult, "Customer tickets retrieved successfully.");
        }


        public async Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(
            int customerId,
            int ticketId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_GetCustomerTicketDetails", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@CustomerId", SqlDbType.Int)
                .Value = customerId;

            command.Parameters.Add("@TicketId", SqlDbType.Int)
                .Value = ticketId;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<Ticket?>.Failure("Failed to retrieve ticket details.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.CustomerNotFoundOrInactive =>
                        ServiceResult<Ticket?>.Forbidden(
                            "Customer not found or inactive."),

                    DatabaseResultCodes.TicketNotFound =>
                        ServiceResult<Ticket?>.NotFound(
                            "Ticket not found."),

                    _ =>
                        ServiceResult<Ticket?>.Failure(
                            "Failed to retrieve ticket details.")
                };
            }

            if (!await reader.NextResultAsync() ||
                !await reader.ReadAsync())
            {
                return ServiceResult<Ticket?>.Failure("Failed to retrieve ticket details.");
            }

            Ticket ticket = new()
            {
                TicketId = reader.GetInt32(reader.GetOrdinal("TicketId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                AssignedAgentId = reader.IsDBNull(reader.GetOrdinal("AssignedAgentId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("AssignedAgentId")),
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

            return ServiceResult<Ticket?>.Success(ticket, "Ticket details retrieved successfully.");
        }


        public async Task<ServiceResult<bool>> CloseCustomerTicketAsync(
            int customerId,
            int ticketId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_CloseCustomerTicket", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@CustomerId", SqlDbType.Int)
                .Value = customerId;

            command.Parameters.Add("@TicketId", SqlDbType.Int)
                .Value = ticketId;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<bool>.Failure("Failed to close ticket.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.CustomerNotFoundOrInactive =>
                        ServiceResult<bool>.Forbidden(
                            "Customer not found or inactive."),

                    DatabaseResultCodes.TicketNotFound =>
                        ServiceResult<bool>.NotFound(
                            "Ticket not found."),

                    DatabaseResultCodes.TicketAlreadyClosed =>
                        ServiceResult<bool>.Conflict(
                            "Ticket is already closed."),

                    DatabaseResultCodes.TicketCloseFailed =>
                        ServiceResult<bool>.Failure(
                            "Failed to close ticket."),

                    _ =>
                        ServiceResult<bool>.Failure(
                            "Failed to close ticket.")
                };
            }

            return ServiceResult<bool>.Success(true, "Ticket closed successfully.");
        }

    }
}
