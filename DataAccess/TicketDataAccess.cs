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

            command.Parameters.AddWithValue("@CustomerId", customerId);
            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@Priority", priority);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
                string message = reader.GetString(reader.GetOrdinal("Message"));

                int ticketId = reader.IsDBNull(reader.GetOrdinal("TicketId"))
                    ? 0
                    : reader.GetInt32(reader.GetOrdinal("TicketId"));

                if (isSuccess)
                    return ServiceResult<int>.Success(ticketId, message);

                return ServiceResult<int>.Failure(message);
            }

            return ServiceResult<int>.Failure("Failed to create ticket.");
        }

        public async Task<ServiceResult<List<Ticket>>> GetCustomerTicketsAsync(int customerId)
        {
            List<Ticket> tickets = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_GetCustomerTickets", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@CustomerId", customerId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<List<Ticket>>.Failure("Failed to retrieve customer tickets.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<List<Ticket>>.Failure(message);

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
                    tickets.Add(ticket);
                }
            }

            return ServiceResult<List<Ticket>>.Success(tickets, message);
        }

        public async Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(int customerId, int ticketId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_GetCustomerTicketDetails", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@CustomerId", customerId);
            command.Parameters.AddWithValue("@TicketId", ticketId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<Ticket?>.Failure("Failed to retrieve ticket details.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<Ticket?>.Failure(message);

            Ticket? ticket = null;

            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                ticket = new()
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
            }
            return ServiceResult<Ticket?>.Success(ticket, message);
        }

        public async Task<ServiceResult<bool>> CloseCustomerTicketAsync(int customerId, int ticketId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_CloseCustomerTicket", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@CustomerId", customerId);
            command.Parameters.AddWithValue("@TicketId", ticketId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
                string message = reader.GetString(reader.GetOrdinal("Message"));

                if (isSuccess)
                    return ServiceResult<bool>.Success(true, message);

                return ServiceResult<bool>.Failure(message);
            }
            return ServiceResult<bool>.Failure("Failed to close ticket.");
        }

        public async Task<ServiceResult<int>> AddTicketCommentAsync(int userId, int ticketId, string commentText)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AddTicketComment", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@TicketId", ticketId);
            command.Parameters.AddWithValue("@CommentText", commentText);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<int>.Failure("Failed to add comment.");


            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<int>.Failure(message);

            int commentId = reader.GetInt32(reader.GetOrdinal("CommentId"));

            return ServiceResult<int>.Success(commentId, message);
        }

        public async Task<ServiceResult<List<TicketComment>>> GetTicketCommentsAsync(int ticketId)
        {
            List<TicketComment> comments = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_GetTicketComments", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@TicketId", ticketId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<List<TicketComment>>.Failure("Failed to retrieve ticket comments.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<List<TicketComment>>.Failure(message);

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

        public async Task<ServiceResult<List<Ticket>>> AdminGetAllTicketsAsync(int adminId)
        {
            List<Ticket> tickets = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AdminGetAllTickets", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@AdminId", adminId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<List<Ticket>>.Failure("Failed to retrieve tickets.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<List<Ticket>>.Failure(message);

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
                    tickets.Add(ticket);
                }
            }
            return ServiceResult<List<Ticket>>.Success(tickets, message);
        }

        public async Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(int adminId, int ticketId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AdminGetTicketDetails", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@AdminId", adminId);
            command.Parameters.AddWithValue("@TicketId", ticketId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<Ticket>.Failure("Failed to retrieve ticket details.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<Ticket>.Failure(message);

            if (!await reader.NextResultAsync() || !await reader.ReadAsync())
                return ServiceResult<Ticket>.Failure("Ticket details not found.");

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

        public async Task<ServiceResult<List<Ticket>>> AdminGetUnassignedTicketsAsync(int adminId)
        {
            List<Ticket> tickets = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AdminGetUnassignedTickets", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@AdminId", adminId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<List<Ticket>>.Failure("Failed to retrieve unassigned tickets.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<List<Ticket>>.Failure(message);

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
                    tickets.Add(ticket);
                }
            }
            return ServiceResult<List<Ticket>>.Success(tickets, message);
        }

        public async Task<ServiceResult<bool>> AssignTicketToAgentAsync(int adminId, int ticketId, int agentId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AssignTicketToAgent", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@AdminId", adminId);
            command.Parameters.AddWithValue("@TicketId", ticketId);
            command.Parameters.AddWithValue("@AgentId", agentId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<bool>.Failure("Failed to assign ticket to agent.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<bool>.Failure(message);

            return ServiceResult<bool>.Success(true, message);
        }

        public async Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(int adminId, int ticketId, string status)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AdminUpdateTicketStatus", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@AdminId", adminId);
            command.Parameters.AddWithValue("@TicketId", ticketId);
            command.Parameters.AddWithValue("@Status", status);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<bool>.Failure("Failed to update ticket status.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<bool>.Failure(message);

            return ServiceResult<bool>.Success(true, message);
        }

        public async Task<ServiceResult<List<Ticket>>> AdminGetTicketsByAgentAsync(int adminId, int agentId)
        {
            List<Ticket> tickets = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AdminGetTicketsByAgent", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@AdminId", adminId);
            command.Parameters.AddWithValue("@AgentId", agentId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<List<Ticket>>.Failure(
                    "Failed to retrieve agent tickets.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<List<Ticket>>.Failure(message);

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

                    tickets.Add(ticket);
                }
            }

            return ServiceResult<List<Ticket>>.Success(tickets, message);
        }

        public async Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(int ticketId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_GetTicketAccessInfo", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@TicketId", ticketId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<TicketAccessInfo>.Failure("Failed to retrieve ticket access info.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<TicketAccessInfo>.Failure(message);

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

        public async Task<ServiceResult<List<Ticket>>> AgentGetAssignedTicketsAsync(int agentId)
        {
            List<Ticket> tickets = new();

            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AgentGetAssignedTickets", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@AgentId", agentId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<List<Ticket>>.Failure("Failed to retrieve assigned tickets.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<List<Ticket>>.Failure(message);

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

                    tickets.Add(ticket);
                }
            }

            return ServiceResult<List<Ticket>>.Success(tickets, message);
        }

        public async Task<ServiceResult<Ticket>> AgentGetAssignedTicketDetailsAsync(int agentId, int ticketId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_AgentGetAssignedTicketDetails", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@AgentId", agentId);
            command.Parameters.AddWithValue("@TicketId", ticketId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return ServiceResult<Ticket>.Failure("Failed to retrieve ticket details.");

            bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
            string message = reader.GetString(reader.GetOrdinal("Message"));

            if (!isSuccess)
                return ServiceResult<Ticket>.Failure(message);

            if (!await reader.NextResultAsync() || !await reader.ReadAsync())
                return ServiceResult<Ticket>.Failure("Ticket details not found.");

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

    }
}