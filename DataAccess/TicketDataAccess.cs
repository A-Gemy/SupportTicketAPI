using Microsoft.Data.SqlClient;
using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
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
    }
}
