using Microsoft.AspNetCore.Authorization;

namespace Metrics.Infrastructure.Identity.Requirements;

public class GiveFeedbackRequirement : IAuthorizationRequirement
{
    public List<string> AllowedUserTitles { get; } = [];

    public GiveFeedbackRequirement(List<string> allowedUserTitles)
    {
        AllowedUserTitles = allowedUserTitles ?? [];
    }

}
