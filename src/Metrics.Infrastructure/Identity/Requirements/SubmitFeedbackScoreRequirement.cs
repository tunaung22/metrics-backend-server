using Microsoft.AspNetCore.Authorization;

namespace Metrics.Infrastructure.Identity.Requirements;

public class SubmitFeedbackScoreRequirement : IAuthorizationRequirement
{
    public List<string> AllowedUserTitles { get; } = [];

    public SubmitFeedbackScoreRequirement(List<string> allowedUserTitles)
    {
        AllowedUserTitles = allowedUserTitles ?? [];
    }
}
