using SupportTicketAPI.Common;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.DataAccess.Interfaces
{
    public interface ICustomerTicketDataAccess
    {
        Task<ServiceResult<int>> CreateTicketAsync(
            int customerId,
            string title,
            string description,
            string priority);

        Task<ServiceResult<PagedResult<Ticket>>> GetCustomerTicketsAsync(
            int customerId,
            int pageNumber = 1,
            int pageSize = 10);

        Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(
            int customerId,
            int ticketId);

        Task<ServiceResult<bool>> CloseCustomerTicketAsync(
            int customerId,
            int ticketId);

    }
}
