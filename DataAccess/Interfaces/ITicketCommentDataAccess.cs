using SupportTicketAPI.Common;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.DataAccess.Interfaces
{
    public interface ITicketCommentDataAccess
    {
        Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(
            int ticketId);

        Task<ServiceResult<int>> AddTicketCommentAsync(
            int userId,
            int ticketId,
            string commentText);

        Task<ServiceResult<List<TicketComment>>> GetTicketCommentsAsync(
            int userId,
            int ticketId);

    }
}
