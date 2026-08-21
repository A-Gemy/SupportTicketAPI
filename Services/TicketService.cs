using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;
using SupportTicketAPI.Services.Interfaces;

namespace SupportTicketAPI.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketDataAccess _ticketDataAccess;

        public TicketService(ITicketDataAccess ticketDataAccess)
        {
            _ticketDataAccess = ticketDataAccess;
        }

        public async Task<ServiceResult<int>> CreateTicketAsync(int customerId, CreateTicketRequest request)
        {
            if (customerId <= 0)
            {
                return ServiceResult<int>.Unauthorized("Invalid customer id.");
            }

            if (request == null)
            {
                return ServiceResult<int>.ValidationFailure("Invalid request.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return ServiceResult<int>.ValidationFailure("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return ServiceResult<int>.ValidationFailure("Description is required.");
            }

            string priority = string.IsNullOrWhiteSpace(request.Priority)
                ? "Medium"
                : request.Priority.Trim();

            if (priority != "Low" && priority != "Medium" && priority != "High")
            {
                return ServiceResult<int>.ValidationFailure("Invalid priority.");
            }

            return await _ticketDataAccess.CreateTicketAsync(
                customerId,
                request.Title.Trim(),
                request.Description.Trim(),
                priority);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> GetCustomerTicketsAsync(int customerId, int pageNumber = 1, int pageSize = 10)
        {
            if (customerId <= 0)
            {
                return ServiceResult<PagedResult<Ticket>>.Unauthorized("Invalid customer id.");
            }

            if (pageNumber < 1)
            {
                return ServiceResult<PagedResult<Ticket>>.ValidationFailure("Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return ServiceResult<PagedResult<Ticket>>.ValidationFailure("Page size must be between 1 and 100.");
            }

            return await _ticketDataAccess.GetCustomerTicketsAsync(customerId, pageNumber, pageSize);
        }

        public async Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(int customerId, int ticketId)
        {
            if (customerId <= 0)
            {
                return ServiceResult<Ticket?>.Unauthorized("Invalid customer id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<Ticket?>.ValidationFailure("Invalid ticket id.");
            }

            return await _ticketDataAccess.GetCustomerTicketDetailsAsync(customerId, ticketId);
        }

        public async Task<ServiceResult<bool>> CloseCustomerTicketAsync(int customerId, int ticketId)
        {
            if (customerId <= 0)
            {
                return ServiceResult<bool>.Unauthorized("Invalid customer id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<bool>.ValidationFailure("Invalid ticket id.");
            }

            return await _ticketDataAccess.CloseCustomerTicketAsync(customerId, ticketId);
        }

        public async Task<ServiceResult<int>> AddTicketCommentAsync(int userId, int ticketId, AddTicketCommentRequest request)
        {
            if (userId <= 0)
            {
                return ServiceResult<int>.Unauthorized("Invalid user id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<int>.ValidationFailure("Invalid ticket id.");
            }

            if (request == null)
            {
                return ServiceResult<int>.ValidationFailure("Invalid request.");
            }

            if (string.IsNullOrWhiteSpace(request.CommentText))
            {
                return ServiceResult<int>.ValidationFailure("Comment text is required.");
            }

            string commentText = request.CommentText.Trim();

            return await _ticketDataAccess.AddTicketCommentAsync(
                userId, ticketId, commentText);
        }

        public async Task<ServiceResult<List<TicketComment>>> GetTicketCommentsAsync(int userId, int ticketId)
        {
            if (userId <= 0)
            {
                return ServiceResult<List<TicketComment>>.Unauthorized("Invalid user id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<List<TicketComment>>.ValidationFailure("Invalid ticket id.");
            }

            return await _ticketDataAccess.GetTicketCommentsAsync(userId, ticketId);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetAllTicketsAsync(int adminId, int pageNumber = 1, int pageSize = 10)
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

            return await _ticketDataAccess.AdminGetAllTicketsAsync(adminId, pageNumber, pageSize);
        }

        public async Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(int adminId, int ticketId)
        {
            if (adminId <= 0)
            {
                return ServiceResult<Ticket>.Unauthorized("Invalid admin id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<Ticket>.ValidationFailure("Invalid ticket id.");
            }

            return await _ticketDataAccess.AdminGetTicketDetailsAsync(adminId, ticketId);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetUnassignedTicketsAsync(int adminId, int pageNumber = 1, int pageSize = 10)
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

            return await _ticketDataAccess.AdminGetUnassignedTicketsAsync(adminId, pageNumber, pageSize);
        }

        public async Task<ServiceResult<bool>> AssignTicketToAgentAsync(int adminId, int ticketId, AssignTicketRequest request)
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

            return await _ticketDataAccess.AssignTicketToAgentAsync(
                adminId,
                ticketId,
                request.AgentId);
        }

        public async Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(int adminId, int ticketId, UpdateTicketStatusRequest request)
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

            return await _ticketDataAccess.AdminUpdateTicketStatusAsync(adminId, ticketId, status);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> AdminGetTicketsByAgentAsync(int adminId, int agentId, int pageNumber = 1, int pageSize = 10)
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

            return await _ticketDataAccess.AdminGetTicketsByAgentAsync(adminId, agentId, pageNumber, pageSize);
        }

        public async Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(int ticketId)
        {
            if (ticketId <= 0)
            {
                return ServiceResult<TicketAccessInfo>.ValidationFailure("Invalid ticket id.");
            }

            return await _ticketDataAccess.GetTicketAccessInfoAsync(ticketId);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> AgentGetAssignedTicketsAsync(int agentId, int pageNumber = 1, int pageSize = 10)
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

            return await _ticketDataAccess.AgentGetAssignedTicketsAsync(agentId, pageNumber, pageSize);
        }

        public async Task<ServiceResult<Ticket>> AgentGetAssignedTicketDetailsAsync(int agentId, int ticketId)
        {
            if (agentId <= 0)
            {
                return ServiceResult<Ticket>.Unauthorized("Invalid agent id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<Ticket>.ValidationFailure("Invalid ticket id.");
            }

            return await _ticketDataAccess.AgentGetAssignedTicketDetailsAsync(agentId, ticketId);
        }

        public async Task<ServiceResult<bool>> AgentUpdateAssignedTicketStatusAsync(int agentId, int ticketId, UpdateTicketStatusRequest request)
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

            return await _ticketDataAccess.AgentUpdateAssignedTicketStatusAsync(agentId, ticketId, status);
        }


    }

}
