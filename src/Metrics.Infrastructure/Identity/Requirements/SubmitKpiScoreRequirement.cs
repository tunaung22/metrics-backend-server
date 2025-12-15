using Microsoft.AspNetCore.Authorization;

namespace Metrics.Infrastructure.Identity.Requirements;

public class SubmitKpiScoreRequirement : IAuthorizationRequirement
{
    public List<string> AllowedUserGroups { get; } = [];
    // public List<string> BannedDepartments { get; } = [];

    public SubmitKpiScoreRequirement(List<string> allowedUserGroups)
    // List<string> bannedDepartments)
    {
        AllowedUserGroups = allowedUserGroups ?? [];
        // BannedDepartments = bannedDepartments ?? [];
    }
}
