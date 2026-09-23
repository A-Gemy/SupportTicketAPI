using SupportTicketAPI.Common;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface IAdminTicketService
    {
        Task<ServiceResult<PagedResult<Ticket>>> AdminGetAllTicketsAsync(
            int adminId,
            int pageNumber = 1,
            int pageSize = 10);

        Task<ServiceResult<Ticket>> AdminGetTicketDetailsAsync(
            int adminId,
            int ticketId);

        Task<ServiceResult<PagedResult<Ticket>>> AdminGetUnassignedTicketsAsync(
            int adminId,
            int pageNumber = 1,
            int pageSize = 10);

        Task<ServiceResult<bool>> AssignTicketToAgentAsync(
            int adminId,
            int ticketId,
            AssignTicketRequest request);

        Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(
            int adminId,
            int ticketId,
            UpdateTicketStatusRequest request);

        Task<ServiceResult<PagedResult<Ticket>>> AdminGetTicketsByAgentAsync(
            int adminId,
            int agentId,
            int pageNumber = 1,
            int pageSize = 10);
    }
}
