using SupportTicketAPI.Common;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface ITicketService
    {
        Task<ServiceResult<int>> CreateTicketAsync(int customerId, CreateTicketRequest request);

        Task<ServiceResult<List<Ticket>>> GetCustomerTicketsAsync(int customerId);

    }
}
