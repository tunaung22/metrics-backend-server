using Microsoft.AspNetCore.Authorization;

namespace Metrics.Infrastructure.Identity.Requirements;

public class SubmitKeyKpiScoreRequirement : IAuthorizationRequirement
{
    public List<string> AllowedUserTitles { get; } = [];

    public SubmitKeyKpiScoreRequirement(List<string> allowedUserTitles)
    {
        AllowedUserTitles = allowedUserTitles ?? [];
    }
}
