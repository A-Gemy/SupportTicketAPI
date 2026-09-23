using SupportTicketAPI.Common;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface IAgentTicketService
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
            UpdateTicketStatusRequest request);
    }
}
