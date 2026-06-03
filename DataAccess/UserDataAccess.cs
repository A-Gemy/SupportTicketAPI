using Microsoft.Data.SqlClient;
using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using System.Data;

namespace SupportTicketAPI.DataAccess
{
    public class UserDataAccess : IUserDataAccess
    {
        private readonly string _connectionString;


        public UserDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        }

        public async Task<ServiceResult<int>> RegisterCustomerAsync(string fullName, string email, string passwordHash)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_RegisterCustomer", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@FullName", fullName);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
                string message = reader.GetString(reader.GetOrdinal("Message")) ?? string.Empty;

                if (!isSuccess)
                    return ServiceResult<int>.Failure(message);

                int userId = Convert.ToInt32(reader["UserId"]);
                return ServiceResult<int>.Success(userId, message);
            }

            return ServiceResult<int>.Failure("Registration failed.");
        }

    }
}
