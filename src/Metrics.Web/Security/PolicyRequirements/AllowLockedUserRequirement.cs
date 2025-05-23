using Microsoft.AspNetCore.Authorization;

namespace Metrics.Web.Security.PolicyRequirements;

public class AllowLockedUserRequirement : IAuthorizationRequirement
{

}
