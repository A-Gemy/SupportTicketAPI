using SupportTicketAPI.Common;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface ICustomerTicketService
    {
        Task<ServiceResult<int>> CreateTicketAsync(
            int customerId, 
            CreateTicketRequest request);

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
