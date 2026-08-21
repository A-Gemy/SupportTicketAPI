using SupportTicketAPI.Common;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface ITicketService
    {
        Task<ServiceResult<int>> CreateTicketAsync(int customerId, CreateTicketRequest request);

        Task<ServiceResult<PagedResult<Ticket>>> GetCustomerTicketsAsync(int customerId, int pageNumber = 1, int pageSize = 10);

        Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(int customerId, int ticketId);

        Task<ServiceResult<bool>> CloseCustomerTicketAsync(int customerId, int ticketId);

        Task<ServiceResult<int>> AddTicketCommentAsync(int userId, int ticketId, AddTicketCommentRequest request);

        Task<ServiceResult<List<TicketComment>>> GetTicketCommentsAsync(int userId, int ticketId);

        Task<ServiceResult<PagedResult<Ticket>>> AdminGetAllTicketsAsync(int adminId, int pageNumber = 1, int pageSize = 10);

        Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(int adminId, int ticketId);

        Task<ServiceResult<PagedResult<Ticket>>> AdminGetUnassignedTicketsAsync(int adminId, int pageNumber = 1, int pageSize = 10);

        Task<ServiceResult<bool>> AssignTicketToAgentAsync(int adminId, int ticketId, AssignTicketRequest request);

        Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(int adminId, int ticketId, UpdateTicketStatusRequest request);

        Task<ServiceResult<PagedResult<Ticket>>> AdminGetTicketsByAgentAsync(int adminId, int agentId, int pageNumber = 1, int pageSize = 10);

        Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(int ticketId);

        Task<ServiceResult<PagedResult<Ticket>>> AgentGetAssignedTicketsAsync(int agentId, int pageNumber = 1, int pageSize = 10);

        Task<ServiceResult<Ticket>> AgentGetAssignedTicketDetailsAsync(int agentId, int ticketId);

        Task<ServiceResult<bool>> AgentUpdateAssignedTicketStatusAsync(int agentId, int ticketId, UpdateTicketStatusRequest request);

    }
}
