using Metrics.Web.Security.PolicyRequirements;
using Microsoft.AspNetCore.Authorization;

namespace Metrics.Web.Security.PolicyHandlers;

public class AllowLockedUserHandler : AuthorizationHandler<AllowLockedUserRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AllowLockedUserRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "AccountLocked" && c.Value == "true"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
