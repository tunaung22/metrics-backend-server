using Metrics.Infrastructure.Identity.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Metrics.Infrastructure.Identity.RequirementHandlers;


public class AllowLockedUserHandler : AuthorizationHandler<AllowLockedUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AllowLockedUserRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "AccountLocked" && c.Value == "true"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
