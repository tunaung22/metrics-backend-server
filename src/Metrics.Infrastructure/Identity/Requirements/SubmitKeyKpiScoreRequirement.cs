using Microsoft.AspNetCore.Authorization;

namespace Metrics.Infrastructure.Identity.Requirements;

public class SubmitKeyKpiScoreRequirement : IAuthorizationRequirement
{
    public List<string> AllowedUserGroups { get; } = [];

    public SubmitKeyKpiScoreRequirement(List<string> allowedUserGroups)
    {
        AllowedUserGroups = allowedUserGroups ?? [];
    }
}
