using Metrics.Infrastructure.Identity.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Metrics.Infrastructure.Identity.RequirementHandlers;

public class StaffRoleRequirementHandler : AuthorizationHandler<StaffRoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StaffRoleRequirement requirement)
    {
        // -----Check Roles-----
        var roles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value.ToLower());
        if (roles.Contains(ApplicationRoles.Staff.ToLower()))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}