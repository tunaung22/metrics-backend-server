using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Identity.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Infrastructure.Identity.RequirementHandlers;

public class SubmitKpiScoreHandler(
    UserManager<ApplicationUser> userManager,
    IIdentityService identityService) : AuthorizationHandler<SubmitKpiScoreRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IIdentityService _identityService = identityService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SubmitKpiScoreRequirement requirement)
    {
        var user = await _userManager.GetUserAsync(context.User);

        if (user == null)
            return;

        // -----Banned Departments-----
        var bannedDepartments = requirement.BannedDepartments;
        // Banned (if user in banned department, then fail)
        var bannedResult = await _identityService.IsUserInDepartment(user, bannedDepartments);
        if (bannedResult.IsSuccess && bannedResult.Data == true)
            context.Fail();

        // -----Allowed User Groups-----
        var allowedUserGroups = requirement.AllowedUserGroups;
        var result = await _identityService.IsUserInTitle(user, allowedUserGroups);
        if (result.IsSuccess && result.Data == true)
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
