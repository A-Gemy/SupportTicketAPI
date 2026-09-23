using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;
using SupportTicketAPI.Services.Interfaces;

namespace SupportTicketAPI.Services
{
    public class AgentTicketService : IAgentTicketService
    {
        private readonly IAgentTicketDataAccess _agentTicketDataAccess;

        public AgentTicketService(
            IAgentTicketDataAccess agentTicketDataAccess)
        {
            _agentTicketDataAccess = agentTicketDataAccess;
        }

        public async Task<ServiceResult<Ticket>> AgentGetAssignedTicketDetailsAsync(
            int agentId, 
            int ticketId)
        {
            if (agentId <= 0)
            {
                return ServiceResult<Ticket>.Unauthorized("Invalid agent id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<Ticket>.ValidationFailure("Invalid ticket id.");
            }

            return await _agentTicketDataAccess.AgentGetAssignedTicketDetailsAsync(
                agentId,
                ticketId);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> AgentGetAssignedTicketsAsync(
            int agentId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (agentId <= 0)
            {
                return ServiceResult<PagedResult<Ticket>>.Unauthorized("Invalid agent id.");
            }

            if (pageNumber < 1)
            {
                return ServiceResult<PagedResult<Ticket>>.ValidationFailure("Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return ServiceResult<PagedResult<Ticket>>.ValidationFailure("Page size must be between 1 and 100.");
            }

            return await _agentTicketDataAccess.AgentGetAssignedTicketsAsync(
                agentId,
                pageNumber,
                pageSize);
        }

        public async Task<ServiceResult<bool>> AgentUpdateAssignedTicketStatusAsync(
            int agentId,
            int ticketId,
            UpdateTicketStatusRequest request)
        {
            if (agentId <= 0)
            {
                return ServiceResult<bool>.Unauthorized("Invalid agent id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<bool>.ValidationFailure("Invalid ticket id.");
            }

            if (request == null)
            {
                return ServiceResult<bool>.ValidationFailure("Invalid request.");
            }

            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return ServiceResult<bool>.ValidationFailure("Ticket status is required.");
            }

            string status = request.Status.Trim();

            if (status != "InProgress" &&
                status != "Resolved")
            {
                return ServiceResult<bool>.ValidationFailure("Agent can only change ticket status to InProgress or Resolved.");
            }

            return await _agentTicketDataAccess.AgentUpdateAssignedTicketStatusAsync(
                agentId,
                ticketId,
                status);
        }

    }
}
