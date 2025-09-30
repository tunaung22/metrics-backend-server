using Microsoft.AspNetCore.Authorization;

namespace Metrics.Infrastructure.Identity.Requirements;

public class SubmitKpiScoreRequirement : IAuthorizationRequirement
{
    public List<string> AllowedUserTitles { get; } = [];

    public SubmitKpiScoreRequirement(List<string> allowedUserTitles)
    {
        AllowedUserTitles = allowedUserTitles ?? [];
    }
}
