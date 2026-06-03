using SupportTicketAPI.Common;

namespace SupportTicketAPI.DataAccess.Interfaces
{

    public interface IUserDataAccess
    {
        Task<ServiceResult<int>> RegisterCustomerAsync(string fullName, string email, string passwordHash);
    }

}
