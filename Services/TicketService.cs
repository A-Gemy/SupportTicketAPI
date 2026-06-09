using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.DTOs.Tickets;
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

            if (string.IsNullOrWhiteSpace(request.Priority))
                return ServiceResult<int>.Failure("Priority is required.");

            return await _ticketDataAccess.CreateTicketAsync(
                customerId,
                request.Title.Trim(),
                request.Description.Trim(),
                request.Priority.Trim());
        }

    }
}
