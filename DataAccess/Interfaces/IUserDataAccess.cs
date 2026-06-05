using SupportTicketAPI.Common;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.DataAccess.Interfaces
{

    public interface IUserDataAccess
    {
        Task<ServiceResult<int>> RegisterCustomerAsync(string fullName, string email, string passwordHash);

        Task<User?> GetUserByEmailAsync(string email);

    }

}
