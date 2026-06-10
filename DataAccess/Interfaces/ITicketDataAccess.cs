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

        Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(int customerId, int ticketId);

        Task<ServiceResult<bool>> CloseCustomerTicketAsync(int customerId, int ticketId);

        Task<ServiceResult<int>> AddCustomerTicketCommentAsync(int customerId, int ticketId, string commentText);

    }
}
