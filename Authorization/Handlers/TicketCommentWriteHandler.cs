using Microsoft.AspNetCore.Authorization;
using SupportTicketAPI.Authorization.Requirements;
using SupportTicketAPI.Constants;
using SupportTicketAPI.Models;
using System.Security.Claims;

namespace SupportTicketAPI.Authorization.Handlers
{
    public class TicketCommentWriteHandler
        : AuthorizationHandler<TicketCommentWriteRequirement, TicketAccessInfo>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TicketCommentWriteRequirement requirement,
            TicketAccessInfo resource)
        {
            string? userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var currentUserId))
            {
                return Task.CompletedTask;
            }

            // Admin can access comments for any ticket.
            if (context.User.IsInRole(UserRoles.Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Customer can access comments for owned tickets.
            if (context.User.IsInRole(UserRoles.Customer) &&
                resource.CustomerId == currentUserId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Agent can access comments for assigned tickets.
            if (context.User.IsInRole(UserRoles.Agent) &&
                resource.AssignedAgentId == currentUserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
