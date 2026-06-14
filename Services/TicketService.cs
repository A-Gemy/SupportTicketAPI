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
            if (request == null)
                return ServiceResult<int>.Failure("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.Title))
                return ServiceResult<int>.Failure("Title is required.");

            if (string.IsNullOrWhiteSpace(request.Description))
                return ServiceResult<int>.Failure("Description is required.");

            string priority = string.IsNullOrWhiteSpace(request.Priority)
                ? "Medium"
                : request.Priority.Trim();

            if (priority != "Low" && priority != "Medium" && priority != "High")
                return ServiceResult<int>.Failure("Invalid priority.");

            return await _ticketDataAccess.CreateTicketAsync(
                customerId,
                request.Title.Trim(),
                request.Description.Trim(),
                priority);
        }

        public async Task<ServiceResult<List<Ticket>>> GetCustomerTicketsAsync(int customerId)
        {
            if (customerId <= 0)
                return ServiceResult<List<Ticket>>.Failure("Invalid customer id.");

            return await _ticketDataAccess.GetCustomerTicketsAsync(customerId);
        }

        public async Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(int customerId, int ticketId)
        {
            if (customerId <= 0)
                return ServiceResult<Ticket?>.Failure("Invalid customer id.");

            if (ticketId <= 0)
                return ServiceResult<Ticket?>.Failure("Invalid ticket id.");

            return await _ticketDataAccess.GetCustomerTicketDetailsAsync(customerId, ticketId);
        }

        public async Task<ServiceResult<bool>> CloseCustomerTicketAsync(int customerId, int ticketId)
        {
            if (customerId <= 0)
                return ServiceResult<bool>.Failure("Invalid customer id.");

            if (ticketId <= 0)
                return ServiceResult<bool>.Failure("Invalid ticket id.");

            return await _ticketDataAccess.CloseCustomerTicketAsync(customerId, ticketId);
        }

        public async Task<ServiceResult<int>> AddCustomerTicketCommentAsync(int customerId, int ticketId, AddTicketCommentRequest request)
        {
            if (customerId <= 0)
                return ServiceResult<int>.Failure("Invalid customer id.");

            if (ticketId <= 0)
                return ServiceResult<int>.Failure("Invalid ticket id.");

            if (request == null)
                return ServiceResult<int>.Failure("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.CommentText))
                return ServiceResult<int>.Failure("Comment text is required.");

            return await _ticketDataAccess.AddCustomerTicketCommentAsync(
                customerId, ticketId, request.CommentText.Trim());
        }

        public async Task<ServiceResult<List<TicketComment>>> GetCustomerTicketCommentsAsync(int customerId, int ticketId)
        {
            if (customerId <= 0)
                return ServiceResult<List<TicketComment>>.Failure("Invalid customer id.");

            if (ticketId <= 0)
                return ServiceResult<List<TicketComment>>.Failure("Invalid ticket id.");

            return await _ticketDataAccess.GetCustomerTicketCommentsAsync(customerId, ticketId);
        }

        public async Task<ServiceResult<List<Ticket>>> AdminGetAllTicketsAsync(int adminId)
        {
            if (adminId <= 0)
                return ServiceResult<List<Ticket>>.Failure("Invalid admin id.");

            return await _ticketDataAccess.AdminGetAllTicketsAsync(adminId);
        }

        public async Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(int adminId, int ticketId)
        {
            if (adminId <= 0)
                return ServiceResult<Ticket>.Failure("Invalid admin id.");

            if (ticketId <= 0)
                return ServiceResult<Ticket>.Failure("Invalid ticket id.");

            return await _ticketDataAccess.AdminGetTicketDetailsAsync(adminId, ticketId);
        }

        public async Task<ServiceResult<List<Ticket>>> AdminGetUnassignedTicketsAsync(int adminId)
        {
            if (adminId <= 0)
                return ServiceResult<List<Ticket>>.Failure("Invalid admin id.");

            return await _ticketDataAccess.AdminGetUnassignedTicketsAsync(adminId);
        }

        public async Task<ServiceResult<bool>> AssignTicketToAgentAsync(int adminId, int ticketId, AssignTicketRequest request)
        {
            if (adminId <= 0)
                return ServiceResult<bool>.Failure("Invalid admin id.");

            if (ticketId <= 0)
                return ServiceResult<bool>.Failure("Invalid ticket id.");

            if (request == null)
                return ServiceResult<bool>.Failure("Invalid request.");

            if (request.AgentId <= 0)
                return ServiceResult<bool>.Failure("Invalid agent id.");

            return await _ticketDataAccess.AssignTicketToAgentAsync(
                adminId,
                ticketId,
                request.AgentId);
        }

        public async Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(int adminId, int ticketId, UpdateTicketStatusRequest request)
        {
            if (adminId <= 0)
                return ServiceResult<bool>.Failure("Invalid admin id.");

            if (ticketId <= 0)
                return ServiceResult<bool>.Failure("Invalid ticket id.");

            if (request == null)
                return ServiceResult<bool>.Failure("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.Status))
                return ServiceResult<bool>.Failure("Ticket status is required.");

            string status = request.Status.Trim();

            if (status != "Open" &&
                status != "InProgress" &&
                status != "Resolved" &&
                status != "Closed")
            {
                return ServiceResult<bool>.Failure("Invalid status.");
            }

            return await _ticketDataAccess.AdminUpdateTicketStatusAsync(adminId, ticketId, status);
        }

        public async Task<ServiceResult<int>> AddAdminTicketCommentAsync(int adminId, int ticketId, AddTicketCommentRequest request)
        {
            if (adminId <= 0)
                return ServiceResult<int>.Failure("Invalid admin id.");

            if (ticketId <= 0)
                return ServiceResult<int>.Failure("Invalid ticket id.");

            if (request == null)
                return ServiceResult<int>.Failure("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.CommentText))
                return ServiceResult<int>.Failure("Comment text is required.");

            string commentText = request.CommentText.Trim();

            return await _ticketDataAccess.AddAdminTicketCommentAsync(
                adminId, ticketId, commentText);
        }

        public async Task<ServiceResult<List<Ticket>>> AdminGetTicketsByAgentAsync(int adminId, int agentId)
        {
            if (adminId <= 0)
                return ServiceResult<List<Ticket>>.Failure("Invalid admin id.");

            if (agentId <= 0)
                return ServiceResult<List<Ticket>>.Failure("Invalid agent id.");

            return await _ticketDataAccess.AdminGetTicketsByAgentAsync(adminId, agentId);
        }

        public async Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(int ticketId)
        {
            if (ticketId <= 0)
            {
                return ServiceResult<TicketAccessInfo>.Failure("Invalid ticket id.");
            }

            return await _ticketDataAccess.GetTicketAccessInfoAsync(ticketId);
        }

    }

}
