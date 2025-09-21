using Microsoft.AspNetCore.Authorization;

namespace Metrics.Infrastructure.Identity.Requirements;


public class AllowLockedUserRequirement : IAuthorizationRequirement
{

}
