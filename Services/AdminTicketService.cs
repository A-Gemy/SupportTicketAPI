using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;
using SupportTicketAPI.Services.Interfaces;

namespace SupportTicketAPI.Services
{
    public class AdminTicketService : IAdminTicketService
    {
        private readonly IAdminTicketDataAccess _adminTicketDataAccess;

        public AdminTicketService(
            IAdminTicketDataAccess adminTicketDataAccess)
        {
            _adminTicketDataAccess = adminTicketDataAccess;
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetAllTicketsAsync(
            int adminId, 
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (adminId <= 0)
            {
                return ServiceResult<PagedResult<Ticket>>.Unauthorized("Invalid admin id.");
            }

            if (pageNumber < 1)
            {
                return ServiceResult<PagedResult<Ticket>>.ValidationFailure("Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return ServiceResult<PagedResult<Ticket>>.ValidationFailure("Page size must be between 1 and 100.");
            }

            return await _adminTicketDataAccess.AdminGetAllTicketsAsync(
                adminId,
                pageNumber,
                pageSize);
        }

        public async Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(
            int adminId, 
            int ticketId)
        {
            if (adminId <= 0)
            {
                return ServiceResult<Ticket>.Unauthorized("Invalid admin id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<Ticket>.ValidationFailure("Invalid ticket id.");
            }

            return await _adminTicketDataAccess.AdminGetTicketDetailsAsync(
                adminId,
                ticketId);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetTicketsByAgentAsync(
            int adminId, 
            int agentId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (adminId <= 0)
            {
                return ServiceResult<PagedResult<Ticket>>.Unauthorized("Invalid admin id.");
            }

            if (agentId <= 0)
            {
                return ServiceResult<PagedResult<Ticket>>.ValidationFailure("Invalid agent id.");
            }

            if (pageNumber < 1)
            {
                return ServiceResult<PagedResult<Ticket>>.ValidationFailure("Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return ServiceResult<PagedResult<Ticket>>.ValidationFailure("Page size must be between 1 and 100.");
            }

            return await _adminTicketDataAccess.AdminGetTicketsByAgentAsync(
                adminId,
                agentId,
                pageNumber,
                pageSize);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetUnassignedTicketsAsync(
            int adminId, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            if (adminId <= 0)
            {
                return ServiceResult<PagedResult<Ticket>>.Unauthorized("Invalid admin id.");
            }

            if (pageNumber < 1)
            {
                return ServiceResult<PagedResult<Ticket>>.ValidationFailure("Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return ServiceResult<PagedResult<Ticket>>.ValidationFailure("Page size must be between 1 and 100.");
            }

            return await _adminTicketDataAccess.AdminGetUnassignedTicketsAsync(
                adminId,
                pageNumber,
                pageSize);
        }

        public async Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(
            int adminId,
            int ticketId,
            UpdateTicketStatusRequest request)
        {
            if (adminId <= 0)
            {
                return ServiceResult<bool>.Unauthorized("Invalid admin id.");
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

            if (status != "Open" &&
                status != "InProgress" &&
                status != "Resolved" &&
                status != "Closed")
            {
                return ServiceResult<bool>.ValidationFailure("Invalid status.");
            }

            return await _adminTicketDataAccess.AdminUpdateTicketStatusAsync(
                adminId,
                ticketId,
                status);
        }

        public async Task<ServiceResult<bool>> AssignTicketToAgentAsync(
            int adminId,
            int ticketId,
            AssignTicketRequest request)
        {
            if (adminId <= 0)
            {
                return ServiceResult<bool>.Unauthorized("Invalid admin id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<bool>.ValidationFailure("Invalid ticket id.");
            }

            if (request == null)
            {
                return ServiceResult<bool>.ValidationFailure("Invalid request.");
            }

            if (request.AgentId <= 0)
            {
                return ServiceResult<bool>.ValidationFailure("Invalid agent id.");
            }

            return await _adminTicketDataAccess.AssignTicketToAgentAsync(
                adminId,
                ticketId,
                request.AgentId);
        }

    }
}
