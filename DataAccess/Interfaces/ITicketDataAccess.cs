using SupportTicketAPI.Common;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.DataAccess.Interfaces
{
    public interface ITicketDataAccess
    {
        Task<ServiceResult<int>> CreateTicketAsync(int customerId, string title, string description, string priority);

        Task<ServiceResult<List<Ticket>>> GetCustomerTicketsAsync(int customerId);

        Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(int customerId, int ticketId);

        Task<ServiceResult<bool>> CloseCustomerTicketAsync(int customerId, int ticketId);

        Task<ServiceResult<int>> AddTicketCommentAsync(int userId, int ticketId, string commentText);

        Task<ServiceResult<List<TicketComment>>> GetTicketCommentsAsync(int ticketId);

        Task<ServiceResult<List<Ticket>>> AdminGetAllTicketsAsync(int adminId);

        Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(int adminId, int ticketId);

        Task<ServiceResult<List<Ticket>>> AdminGetUnassignedTicketsAsync(int adminId);

        Task<ServiceResult<bool>> AssignTicketToAgentAsync(int adminId, int ticketId, int agentId);

        Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(int adminId, int ticketId, string status);

        Task<ServiceResult<List<Ticket>>> AdminGetTicketsByAgentAsync(int adminId, int agentId);

        Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(int ticketId);

        Task<ServiceResult<List<Ticket>>> AgentGetAssignedTicketsAsync(int agentId);

        Task<ServiceResult<Ticket>> AgentGetAssignedTicketDetailsAsync(int agentId, int ticketId);

        Task<ServiceResult<bool>> AgentUpdateAssignedTicketStatusAsync(int agentId, int ticketId, string status);

    }
}
