using Microsoft.Data.SqlClient;
using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.Models;
using System.Data;

namespace SupportTicketAPI.DataAccess
{
    public class TicketDataAccess : ITicketDataAccess
    {
        private readonly string _connectionString;

        public TicketDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        }


        public async Task<ServiceResult<int>> CreateTicketAsync(int customerId, string title, string description, string priority)
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
                string message = reader.GetString(reader.GetOrdinal("Message"));

                int ticketId = reader.IsDBNull(reader.GetOrdinal("TicketId"))
                    ? 0
                    : reader.GetInt32(reader.GetOrdinal("TicketId"));

                if (!isSuccess)
                {
                    return message switch
                    {
                        "Customer not found or inactive." =>
                             ServiceResult<int>.Forbidden(message),

                        "Invalid priority." =>
                            ServiceResult<int>.ValidationFailure(message),

                        _ => ServiceResult<int>.Failure(message),
                    };
                }

                return ServiceResult<int>.Success(ticketId, message);
            }

            return ServiceResult<int>.Failure("Failed to create ticket.");
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> GetCustomerTicketsAsync(int customerId, int pageNumber = 1, int pageSize = 10)
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
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Customer not found or inactive." =>
                        ServiceResult<PagedResult<Ticket>>
                            .Forbidden(message),

                    "Page number must be greater than or equal to 1." =>
                        ServiceResult<PagedResult<Ticket>>
                            .ValidationFailure(message),

                    "Page size must be between 1 and 100." =>
                        ServiceResult<PagedResult<Ticket>>
                            .ValidationFailure(message),

                    _ =>
                        ServiceResult<PagedResult<Ticket>>
                            .Failure(message)
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

            return ServiceResult<PagedResult<Ticket>>.Success(pagedResult, message);
        }

        public async Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(int customerId, int ticketId)
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
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Customer not found or inactive." =>
                        ServiceResult<Ticket?>.Forbidden(
                            message),

                    "Ticket not found." =>
                        ServiceResult<Ticket?>.NotFound(
                            message),

                    _ =>
                        ServiceResult<Ticket?>.Failure(
                            message)
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

            return ServiceResult<Ticket?>.Success(ticket, message);
        }

        public async Task<ServiceResult<bool>> CloseCustomerTicketAsync(int customerId, int ticketId)
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
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Customer not found or inactive." =>
                        ServiceResult<bool>.Forbidden(
                            message),

                    "Ticket not found." =>
                        ServiceResult<bool>.NotFound(
                            message),

                    "Ticket is already closed." =>
                        ServiceResult<bool>.Conflict(
                            message),

                    _ => ServiceResult<bool>.Failure(message)
                };
            }

            return ServiceResult<bool>.Success(true, message);
        }

        public async Task<ServiceResult<int>> AddTicketCommentAsync(int userId, int ticketId, string commentText)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AddTicketComment", connection);
            command.CommandType = CommandType.StoredProcedure;


            command.Parameters.Add("@UserId", SqlDbType.Int)
                .Value = userId;

            command.Parameters.Add("@TicketId", SqlDbType.Int)
                .Value = ticketId;

            command.Parameters.Add("@CommentText", SqlDbType.NVarChar, 1000)
                .Value = commentText;


            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<int>.Failure("Failed to add comment.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Comment text is required." =>
                        ServiceResult<int>.ValidationFailure(message),

                    "User not found or inactive." =>
                        ServiceResult<int>.Forbidden(message),

                    "Ticket not found." =>
                        ServiceResult<int>.NotFound(message),

                    "Comments cannot be added to a closed ticket." =>
                        ServiceResult<int>.Conflict(message),

                    "You are not authorized to add comments to this ticket." =>
                        ServiceResult<int>.Forbidden(message),

                    _ =>
                        ServiceResult<int>.Failure(message)
                };
            }

            int commentId = reader.GetInt32(reader.GetOrdinal("CommentId"));

            return ServiceResult<int>.Success(commentId, message);
        }

        public async Task<ServiceResult<List<TicketComment>>> GetTicketCommentsAsync(int userId, int ticketId)
        {
            List<TicketComment> comments = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_GetTicketComments", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.Int)
                .Value = userId;

            command.Parameters.Add("@TicketId", SqlDbType.Int)
                .Value = ticketId;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<List<TicketComment>>.Failure("Failed to retrieve ticket comments.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "User not found or inactive." =>
                        ServiceResult<List<TicketComment>>.Forbidden(message),

                    "Ticket not found." =>
                        ServiceResult<List<TicketComment>>.NotFound(message),

                    "You do not have permission to view comments for this ticket." =>
                        ServiceResult<List<TicketComment>>.Forbidden(message),

                    _ =>
                        ServiceResult<List<TicketComment>>.Failure(message)
                };
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    TicketComment comment = new()
                    {
                        CommentId = reader.GetInt32(reader.GetOrdinal("CommentId")),
                        TicketId = reader.GetInt32(reader.GetOrdinal("TicketId")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        UserFullName = reader.GetString(reader.GetOrdinal("UserFullName")),
                        UserRole = reader.GetString(reader.GetOrdinal("UserRole")),
                        CommentText = reader.GetString(reader.GetOrdinal("CommentText")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    };
                    comments.Add(comment);
                }
            }

            return ServiceResult<List<TicketComment>>.Success(comments, message);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetAllTicketsAsync(int adminId, int pageNumber = 1, int pageSize = 10)
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
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Admin not found or inactive." =>
                        ServiceResult<PagedResult<Ticket>>.Forbidden(message),

                    "Page number must be greater than or equal to 1." =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(message),

                    "Page size must be between 1 and 100." =>
                        ServiceResult<PagedResult<Ticket>>.ValidationFailure(message),

                    _ => ServiceResult<PagedResult<Ticket>>.Failure(message)
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

        public async Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(int adminId, int ticketId)
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
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Admin not found or inactive." =>
                        ServiceResult<Ticket>.Forbidden(message),

                    "Ticket not found." =>
                        ServiceResult<Ticket>.NotFound(message),

                    _ => ServiceResult<Ticket>.Failure(message)
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

        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetUnassignedTicketsAsync(int adminId, int pageNumber = 1, int pageSize = 10)
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
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Admin not found or inactive." =>
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

        public async Task<ServiceResult<bool>> AssignTicketToAgentAsync(int adminId, int ticketId, int agentId)
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
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Admin not found or inactive." =>
                        ServiceResult<bool>.Forbidden(message),

                    "Ticket not found." =>
                        ServiceResult<bool>.NotFound(message),

                    "Agent not found or inactive." =>
                        ServiceResult<bool>.NotFound(message),

                    "Closed tickets cannot be assigned." =>
                        ServiceResult<bool>.Conflict(message),

                    _ => ServiceResult<bool>.Failure(message)
                };
            }

            return ServiceResult<bool>.Success(true, message);
        }

        public async Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(int adminId, int ticketId, string status)
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
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Admin not found or inactive." =>
                        ServiceResult<bool>.Forbidden(message),

                    "Ticket not found." =>
                        ServiceResult<bool>.NotFound(message),

                    "Invalid ticket status." =>
                        ServiceResult<bool>.ValidationFailure(message),

                    "Closed tickets cannot be updated." =>
                        ServiceResult<bool>.Conflict(message),

                    "An assigned ticket cannot be moved to Open." =>
                        ServiceResult<bool>.Conflict(message),

                    "The ticket must be assigned before using this status." =>
                        ServiceResult<bool>.Conflict(message),

                    _ => ServiceResult<bool>.Failure(message)
                };
            }

            return ServiceResult<bool>.Success(true, message);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetTicketsByAgentAsync(int adminId, int agentId, int pageNumber = 1, int pageSize = 10)
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
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                return message switch
                {
                    "Admin not found or inactive." =>
                        ServiceResult<PagedResult<Ticket>>.Forbidden(message),

                    "Agent not found or inactive." =>
                        ServiceResult<PagedResult<Ticket>>.NotFound(message),

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

        public async Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(int ticketId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_GetTicketAccessInfo", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@TicketId", SqlDbType.Int)
                .Value = ticketId;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return ServiceResult<TicketAccessInfo>.Failure("Failed to retrieve ticket access info.");
            }

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
            {
                if (message == "Ticket not found.")
                {
                    return ServiceResult<TicketAccessInfo>.NotFound(message);
                }

                return ServiceResult<TicketAccessInfo>.Failure(message);
            }

            if (!await reader.NextResultAsync() ||
                !await reader.ReadAsync())
            {
                return ServiceResult<TicketAccessInfo>.Failure(
                    "Ticket access information not found.");
            }

            TicketAccessInfo ticketAccessInfo = new()
            {
                TicketId = reader.GetInt32(reader.GetOrdinal("TicketId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),

                AssignedAgentId = reader.IsDBNull(reader.GetOrdinal("AssignedAgentId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("AssignedAgentId")),

                Status = reader.GetString(reader.GetOrdinal("Status"))
            };

            return ServiceResult<TicketAccessInfo>.Success(ticketAccessInfo, message);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> AgentGetAssignedTicketsAsync(int agentId, int pageNumber = 1, int pageSize = 10)
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

        public async Task<ServiceResult<Ticket>> AgentGetAssignedTicketDetailsAsync(int agentId, int ticketId)
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

        public async Task<ServiceResult<bool>> AgentUpdateAssignedTicketStatusAsync(int agentId, int ticketId, string status)
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