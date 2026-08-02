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


            command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100)
                .Value = fullName;

            command.Parameters.Add("@Email", SqlDbType.NVarChar, 150)
                .Value = email;

            command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500)
                .Value = passwordHash;


            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
                string message = reader.GetString(reader.GetOrdinal("Message")) ?? string.Empty;

                if (!isSuccess)
                {
                    if (message == "Email already exists.")
                    {
                        return ServiceResult<int>.Conflict(message);
                    }
                    return ServiceResult<int>.Failure(message);
                }

                int userId = reader.GetInt32(reader.GetOrdinal("UserId"));

                return ServiceResult<int>.Success(userId, message);
            }

            return ServiceResult<int>.Failure("Registration failed.");
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_GetUserByEmail", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Email", SqlDbType.NVarChar, 150)
                .Value = email;

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


            command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100)
                .Value = fullName;

            command.Parameters.Add("@Email", SqlDbType.NVarChar, 150)
                .Value = email;

            command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500)
                .Value = passwordHash;


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


            command.Parameters.Add("@UserId", SqlDbType.Int)
                .Value = userId;

            command.Parameters.Add("@TokenHash", SqlDbType.NVarChar, 255)
                .Value = tokenHash;

            command.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2)
                .Value = expiresAt;


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

            command.Parameters.Add("@TokenHash", SqlDbType.NVarChar, 255)
                .Value = tokenHash;

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

        public async Task<ServiceResult<RefreshTokenRotationResult>> RotateRefreshTokenAsync(string oldTokenHash, string newTokenHash, DateTime newExpiresAt)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_RotateRefreshToken", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@OldTokenHash", SqlDbType.NVarChar, 255)
                .Value = oldTokenHash;

            command.Parameters.Add("@NewTokenHash", SqlDbType.NVarChar, 255)
                .Value = newTokenHash;

            command.Parameters.Add("@NewExpiresAt", SqlDbType.DateTime2)
                .Value = newExpiresAt;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                bool isSuccess = reader.GetBoolean(reader.GetOrdinal("IsSuccess"));
                string message = reader.GetString(reader.GetOrdinal("Message"));

                if (!isSuccess)
                {
                    return ServiceResult<RefreshTokenRotationResult>.Failure(message);
                }

                RefreshTokenRotationResult rotationResult = new RefreshTokenRotationResult
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Role = reader.GetString(reader.GetOrdinal("Role")),
                    RefreshTokenId = reader.GetInt32(reader.GetOrdinal("RefreshTokenId"))
                };

                return ServiceResult<RefreshTokenRotationResult>.Success(rotationResult, message);
            }

            return ServiceResult<RefreshTokenRotationResult>.Failure("Failed to rotate refresh token.");
        }

        public async Task<ServiceResult<bool>> RevokeRefreshTokenAsync(string tokenHash)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand command = new("usp_RevokeRefreshToken", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@TokenHash", SqlDbType.NVarChar, 255)
                .Value = tokenHash;

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
