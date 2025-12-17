using Microsoft.AspNetCore.Authorization;

namespace Metrics.Infrastructure.Identity.Requirements;

public class SubmitKpiScoreRequirement(
    List<string> allowedUserGroups,
    List<string> bannedDepartments) : IAuthorizationRequirement
{
    public List<string> AllowedUserGroups { get; } = allowedUserGroups ?? [];
    public List<string> BannedDepartments { get; } = bannedDepartments ?? [];
}
