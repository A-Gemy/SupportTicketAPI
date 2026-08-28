using SupportTicketAPI.Common;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.DataAccess.Interfaces
{
    public interface IAdminTicketDataAccess
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
            int agentId);

        Task<ServiceResult<bool>> AdminUpdateTicketStatusAsync(
            int adminId,
            int ticketId,
            string status);

        Task<ServiceResult<PagedResult<Ticket>>> AdminGetTicketsByAgentAsync(
            int adminId,
            int agentId,
            int pageNumber = 1,
            int pageSize = 10);

    }
}
