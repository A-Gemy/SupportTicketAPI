using SupportTicketAPI.Common;
using SupportTicketAPI.DTOs.Tickets;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface ITicketService
    {
        Task<ServiceResult<int>> CreateTicketAsync(int customerId, CreateTicketRequest request);

    }
}
