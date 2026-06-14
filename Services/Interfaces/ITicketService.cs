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

        Task<ServiceResult<int>> AddCustomerTicketCommentAsync(int customerId, int ticketId, AddTicketCommentRequest request);

        Task<ServiceResult<List<TicketComment>>> GetTicketCommentsAsync(int ticketId);

        Task<ServiceResult<List<Ticket>>> AdminGetAllTicketsAsync(int adminId);

        Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(int adminId, int ticketId);

        Task<ServiceResult<List<Ticket>>> AdminGetUnassignedTicketsAsync(int adminId);

        Task<ServiceResult<bool>> AssignTicketToAgentAsync(int adminId, int ticketId, AssignTicketRequest request);

        Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(int adminId, int ticketId, UpdateTicketStatusRequest request);

        Task<ServiceResult<int>> AddAdminTicketCommentAsync(int adminId, int ticketId, AddTicketCommentRequest request);

        Task<ServiceResult<List<Ticket>>> AdminGetTicketsByAgentAsync(int adminId, int agentId);

        Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(int ticketId);

    }
}
