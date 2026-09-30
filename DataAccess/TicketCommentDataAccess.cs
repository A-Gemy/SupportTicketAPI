using Microsoft.Data.SqlClient;
using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.Models;
using System.Data;

namespace SupportTicketAPI.DataAccess
{
    public class TicketCommentDataAccess : ITicketCommentDataAccess
    {
        private readonly string _connectionString;

        public TicketCommentDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        }


        public async Task<ServiceResult<int>> AddTicketCommentAsync(
            int userId,
            int ticketId,
            string commentText)
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
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.CommentTextRequired =>
                        ServiceResult<int>.ValidationFailure(
                            "Comment text is required."),

                    DatabaseResultCodes.UserNotFoundOrInactive =>
                        ServiceResult<int>.Forbidden(
                            "User not found or inactive."),

                    DatabaseResultCodes.TicketNotFound =>
                        ServiceResult<int>.NotFound(
                            "Ticket not found."),

                    DatabaseResultCodes.ClosedTicketCannotBeCommented =>
                        ServiceResult<int>.Conflict(
                            "Comments cannot be added to a closed ticket."),

                    DatabaseResultCodes.TicketCommentAddForbidden =>
                        ServiceResult<int>.Forbidden(
                            "You are not authorized to add comments to this ticket."),

                    _ =>
                        ServiceResult<int>.Failure(
                            "Failed to add comment.")
                };
            }

            int commentId = reader.GetInt32(reader.GetOrdinal("CommentId"));

            return ServiceResult<int>.Success(commentId, "Comment added successfully.");
        }


        public async Task<ServiceResult<List<TicketComment>>> GetTicketCommentsAsync(
            int userId,
            int ticketId)
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
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.UserNotFoundOrInactive =>
                        ServiceResult<List<TicketComment>>.Forbidden(
                            "User not found or inactive."),

                    DatabaseResultCodes.TicketNotFound =>
                        ServiceResult<List<TicketComment>>.NotFound(
                            "Ticket not found."),

                    DatabaseResultCodes.TicketCommentReadForbidden =>
                        ServiceResult<List<TicketComment>>.Forbidden(
                            "You do not have permission to view comments for this ticket."),

                    _ =>
                        ServiceResult<List<TicketComment>>.Failure(
                            "Failed to retrieve ticket comments.")
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

            return ServiceResult<List<TicketComment>>.Success(comments, "Ticket comments retrieved successfully.");
        }


        public async Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(
            int ticketId)
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
            string resultCode = reader.GetString(reader.GetOrdinal("ResultCode"));

            if (!isSuccess)
            {
                return resultCode switch
                {
                    DatabaseResultCodes.TicketNotFound =>
                        ServiceResult<TicketAccessInfo>.NotFound(
                            "Ticket not found."),

                    _ =>
                        ServiceResult<TicketAccessInfo>.Failure(
                            "Failed to retrieve ticket access info.")
                };
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

            return ServiceResult<TicketAccessInfo>.Success(ticketAccessInfo, "Ticket access information retrieved successfully.");
        }

    }
}
