using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Identity.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Metrics.Infrastructure.Identity.RequirementHandlers;

public class AccessAdminFeaturesHandler(
    MetricsDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
IUserTitleService userTitleService,
    IIdentityService identityService
) : AuthorizationHandler<AccessAdminFeaturesRequirement>
{
    private readonly MetricsDbContext _context = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly IUserTitleService _userTitleService = userTitleService;
    private readonly IIdentityService _identityService = identityService;


    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccessAdminFeaturesRequirement requirement)
    {
        // get user group
        var roles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value.ToLower());

        if (roles.Contains(ApplicationRoles.Admin.ToLower()))
        {
            context.Succeed(requirement);
        }

        // var userId = _userManager.GetUserId(context.User);
        // if (userId != null)
        // {

        //     if (context.User.IsInRole(ApplicationRoles.Admin))
        //     {
        //         context.Succeed(requirement);
        //     }

        // }

        return Task.CompletedTask;
    }
}
