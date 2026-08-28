using SupportTicketAPI.Common;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.DataAccess.Interfaces
{
    public interface IAgentTicketDataAccess
    {
        Task<ServiceResult<PagedResult<Ticket>>> AgentGetAssignedTicketsAsync(
            int agentId,
            int pageNumber = 1,
            int pageSize = 10);

        Task<ServiceResult<Ticket>> AgentGetAssignedTicketDetailsAsync(
            int agentId,
            int ticketId);

        Task<ServiceResult<bool>> AgentUpdateAssignedTicketStatusAsync(
            int agentId,
            int ticketId,
            string status);

    }
}
