using SupportTicketAPI.Common;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.DataAccess.Interfaces
{
    public interface ITicketDataAccess
    {
        Task<ServiceResult<int>> CreateTicketAsync(int customerId, string title, string description, string priority);

        Task<ServiceResult<PagedResult<Ticket>>> GetCustomerTicketsAsync(int customerId, int pageNumber = 1, int pageSize = 10);

        Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(int customerId, int ticketId);

        Task<ServiceResult<bool>> CloseCustomerTicketAsync(int customerId, int ticketId);

        Task<ServiceResult<int>> AddTicketCommentAsync(int userId, int ticketId, string commentText);

        Task<ServiceResult<List<TicketComment>>> GetTicketCommentsAsync(int userId, int ticketId);

        Task<ServiceResult<PagedResult<Ticket>>> AdminGetAllTicketsAsync(int adminId, int pageNumber = 1, int pageSize = 10);

        Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(int adminId, int ticketId);

        Task<ServiceResult<PagedResult<Ticket>>> AdminGetUnassignedTicketsAsync(int adminId, int pageNumber = 1, int pageSize = 10);

        Task<ServiceResult<bool>> AssignTicketToAgentAsync(int adminId, int ticketId, int agentId);

        Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(int adminId, int ticketId, string status);

        Task<ServiceResult<PagedResult<Ticket>>> AdminGetTicketsByAgentAsync(int adminId, int agentId, int pageNumber = 1, int pageSize = 10);

        Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(int ticketId);

        Task<ServiceResult<PagedResult<Ticket>>> AgentGetAssignedTicketsAsync(int agentId, int pageNumber = 1, int pageSize = 10);

        Task<ServiceResult<Ticket>> AgentGetAssignedTicketDetailsAsync(int agentId, int ticketId);

        Task<ServiceResult<bool>> AgentUpdateAssignedTicketStatusAsync(int agentId, int ticketId, string status);

    }
}
