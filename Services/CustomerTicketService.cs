using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;
using SupportTicketAPI.Services.Interfaces;

namespace SupportTicketAPI.Services
{
    public class CustomerTicketService : ICustomerTicketService
    {
        private readonly ICustomerTicketDataAccess _customerTicketDataAccess;

        public CustomerTicketService(
            ICustomerTicketDataAccess customerTicketDataAccess)
        {
            _customerTicketDataAccess = customerTicketDataAccess;
        }

        public async Task<ServiceResult<int>> CreateTicketAsync(
            int customerId,
            CreateTicketRequest request)
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

            return await _customerTicketDataAccess.CreateTicketAsync(
                customerId,
                request.Title.Trim(),
                request.Description.Trim(),
                priority);
        }

        public async Task<ServiceResult<PagedResult<Ticket>>> GetCustomerTicketsAsync(
            int customerId, 
            int pageNumber = 1,
            int pageSize = 10)
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

            return await _customerTicketDataAccess.GetCustomerTicketsAsync(
                customerId,
                pageNumber,
                pageSize);
        }

        public async Task<ServiceResult<Ticket?>> GetCustomerTicketDetailsAsync(
            int customerId, 
            int ticketId)
        {
            if (customerId <= 0)
            {
                return ServiceResult<Ticket?>.Unauthorized("Invalid customer id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<Ticket?>.ValidationFailure("Invalid ticket id.");
            }

            return await _customerTicketDataAccess.GetCustomerTicketDetailsAsync(
                customerId,
                ticketId);
        }

        public async Task<ServiceResult<bool>> CloseCustomerTicketAsync(
            int customerId, 
            int ticketId)
        {
            if (customerId <= 0)
            {
                return ServiceResult<bool>.Unauthorized("Invalid customer id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<bool>.ValidationFailure("Invalid ticket id.");
            }

            return await _customerTicketDataAccess.CloseCustomerTicketAsync(
                customerId,
                ticketId);
        }

    }
}
