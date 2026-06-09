using SupportTicketAPI.Common;

namespace SupportTicketAPI.DataAccess.Interfaces
{
    public interface ITicketDataAccess
    {
        Task<ServiceResult<int>> CreateTicketAsync(
            int customerId,
            string title,
            string description,
            string priority);

    }
}
