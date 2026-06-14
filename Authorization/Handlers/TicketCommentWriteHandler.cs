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
            // No comments are allowed on closed tickets.
            if (resource.Status == "Closed")
                return Task.CompletedTask;

            string? userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var currentUserId))
                return Task.CompletedTask;

            // Admin can comment on any non-closed ticket.
            if (context.User.IsInRole(UserRoles.Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Customer can comment on owned non-closed tickets.
            if (context.User.IsInRole(UserRoles.Customer) &&
                resource.CustomerId == currentUserId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Agent can comment on assigned non-closed tickets.
            if (context.User.IsInRole(UserRoles.Agent) &&
                resource.AssignedAgentId == currentUserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
