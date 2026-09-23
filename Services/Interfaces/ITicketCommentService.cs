using SupportTicketAPI.Common;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services.Interfaces
{
    public interface ITicketCommentService
    {
        Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(
            int ticketId);

        Task<ServiceResult<int>> AddTicketCommentAsync(
            int userId,
            int ticketId,
            AddTicketCommentRequest request);

        Task<ServiceResult<List<TicketComment>>> GetTicketCommentsAsync(
            int userId,
            int ticketId);
    }
}
