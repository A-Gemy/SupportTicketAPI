using SupportTicketAPI.Common;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.DTOs.Tickets;
using SupportTicketAPI.Models;
using SupportTicketAPI.Services.Interfaces;

namespace SupportTicketAPI.Services
{
    public class TicketCommentService : ITicketCommentService
    {
        private readonly ITicketCommentDataAccess _ticketCommentDataAccess;

        public TicketCommentService(
            ITicketCommentDataAccess ticketCommentDataAccess)
        {
            _ticketCommentDataAccess = ticketCommentDataAccess;
        }

        public async Task<ServiceResult<int>> AddTicketCommentAsync(
            int userId, 
            int ticketId, 
            AddTicketCommentRequest request)
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

            return await _ticketCommentDataAccess.AddTicketCommentAsync(
                userId,
                ticketId,
                commentText);
        }

        public async Task<ServiceResult<List<TicketComment>>> GetTicketCommentsAsync(
            int userId,
            int ticketId)
        {
            if (userId <= 0)
            {
                return ServiceResult<List<TicketComment>>.Unauthorized("Invalid user id.");
            }

            if (ticketId <= 0)
            {
                return ServiceResult<List<TicketComment>>.ValidationFailure("Invalid ticket id.");
            }

            return await _ticketCommentDataAccess.GetTicketCommentsAsync(
                userId,
                ticketId);
        }

        public async Task<ServiceResult<TicketAccessInfo>> GetTicketAccessInfoAsync(
            int ticketId)
        {
            if (ticketId <= 0)
            {
                return ServiceResult<TicketAccessInfo>.ValidationFailure("Invalid ticket id.");
            }

            return await _ticketCommentDataAccess.GetTicketAccessInfoAsync(ticketId);
        }

    }
}
