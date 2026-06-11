using SupportTicketAPI.Common;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface ITicketService
    {
        Task<ServiceResult<int>> CreateTicketAsync(int customerId, CreateTicketRequest request);

        Task<ServiceResult<List<Ticket>>> GetCustomerTicketsAsync(int customerId);

        Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(int customerId, int ticketId);

        Task<ServiceResult<bool>> CloseCustomerTicketAsync(int customerId, int ticketId);

        Task<ServiceResult<int>> AddCustomerTicketCommentAsync(
            int customerId,
            int ticketId,
            AddTicketCommentRequest request);

        Task<ServiceResult<List<TicketComment>>> GetCustomerTicketCommentsAsync(int customerId, int ticketId);

        Task<ServiceResult<List<Ticket>>> AdminGetAllTicketsAsync(int adminId);

        Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(int adminId, int ticketId);

        Task<ServiceResult<List<Ticket>>> AdminGetUnassignedTicketsAsync(int adminId);

    }
}
