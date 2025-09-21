using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Identity.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Infrastructure.Identity.RequirementHandlers;

public class GiveFeedbackHandler(
    UserManager<ApplicationUser> userManager,
    IIdentityService identityService

) : AuthorizationHandler<GiveFeedbackRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IIdentityService _identityService = identityService;


    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        GiveFeedbackRequirement requirement)
    {
        var allowedUserTitles = requirement.AllowedUserTitles;

        var user = await _userManager.GetUserAsync(context.User);
        if (user == null) return;

        var result = await _identityService.IsUserInTitle(
            user,
            allowedUserTitles);

        if (result.IsSuccess && result.Data == true)
            context.Succeed(requirement);
    }
}