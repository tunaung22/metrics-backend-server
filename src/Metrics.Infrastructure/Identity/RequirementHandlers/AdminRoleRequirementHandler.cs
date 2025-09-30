using Metrics.Infrastructure.Identity.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Metrics.Infrastructure.Identity.RequirementHandlers;

public class AdminRoleRequirementHandler : AuthorizationHandler<AdminRoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminRoleRequirement requirement)
    {
        // -----Check Roles-----
        var roles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value.ToLower());

        if (roles.Contains(ApplicationRoles.Admin.ToLower()))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
