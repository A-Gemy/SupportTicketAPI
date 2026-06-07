using Microsoft.Data.SqlClient;
using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.Models;
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

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_GetUserByEmail", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Email", email);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                    Role = reader.GetString(reader.GetOrdinal("Role")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                };
            }

            return null;
        }

        public async Task<ServiceResult<int>> CreateAgentAsync(string fullName, string email, string passwordHash)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_CreateAgent", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@FullName", fullName);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
                string message = reader.GetString(reader.GetOrdinal("Message"));

                int userId = reader.IsDBNull(reader.GetOrdinal("UserId"))
                    ? 0
                    : reader.GetInt32(reader.GetOrdinal("UserId"));

                if (isSuccess)
                    return ServiceResult<int>.Success(userId, message);

                return ServiceResult<int>.Failure(message);
            }

            return ServiceResult<int>.Failure("Failed to create agent.");
        }

        public async Task<ServiceResult<int>> SaveRefreshTokenAsync(int userId, string tokenHash, DateTime expiresAt)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_SaveRefreshToken", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@TokenHash", tokenHash);
            command.Parameters.AddWithValue("@ExpiresAt", expiresAt);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
                string message = reader.GetString(reader.GetOrdinal("Message"));

                int refreshTokenId = reader.IsDBNull(reader.GetOrdinal("RefreshTokenId"))
                    ? 0
                    : reader.GetInt32(reader.GetOrdinal("RefreshTokenId"));

                if (isSuccess)
                    return ServiceResult<int>.Success(refreshTokenId, message);

                return ServiceResult<int>.Failure(message);
            }

            return ServiceResult<int>.Failure("Failed to save refresh token.");
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_GetRefreshToken", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@TokenHash", tokenHash);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new RefreshToken
                {
                    RefreshTokenId = reader.GetInt32(reader.GetOrdinal("RefreshTokenId")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    TokenHash = reader.GetString(reader.GetOrdinal("TokenHash")),
                    ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt")),
                    RevokedAt = reader.IsDBNull(reader.GetOrdinal("RevokedAt"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("RevokedAt")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                    User = new User
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                        Role = reader.GetString(reader.GetOrdinal("Role")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("UserCreatedAt"))
                    }
                };
            }

            return null;
        }

        public async Task<ServiceResult<bool>> RevokeRefreshTokenAsync(string tokenHash)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_RevokeRefreshToken", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@TokenHash", tokenHash);

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

            return ServiceResult<bool>.Failure("Failed to revoke refresh token.");
        }

    }
}
