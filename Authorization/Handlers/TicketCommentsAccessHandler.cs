using Microsoft.AspNetCore.Authorization;
using SupportTicketAPI.Authorization.Requirements;
using SupportTicketAPI.Constants;
using SupportTicketAPI.Models;
using System.Security.Claims;

namespace SupportTicketAPI.Authorization.Handlers
{
    public class TicketCommentsAccessHandler
        : AuthorizationHandler<TicketCommentsAccessRequirement, TicketAccessInfo>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TicketCommentsAccessRequirement requirement,
            TicketAccessInfo resource)
        {
            string? userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int currentUserId))
                return Task.CompletedTask;

            // Admin can view comments for any ticket.
            if (context.User.IsInRole(UserRoles.Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Customer can view comments for owned tickets only.
            if (context.User.IsInRole(UserRoles.Customer)
                && resource.CustomerId == currentUserId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Agent can view comments for assigned tickets only.
            if (context.User.IsInRole(UserRoles.Agent)
                && resource.AssignedAgentId == currentUserId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

    }
}
