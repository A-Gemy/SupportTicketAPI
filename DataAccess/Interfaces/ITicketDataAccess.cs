using SupportTicketAPI.Common;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.DataAccess.Interfaces
{
    public interface ITicketDataAccess
    {
        Task<ServiceResult<int>> CreateTicketAsync(
            int customerId,
            string title,
            string description,
            string priority);

        Task<ServiceResult<List<Ticket>>> GetCustomerTicketsAsync(int customerId);

    }
}
