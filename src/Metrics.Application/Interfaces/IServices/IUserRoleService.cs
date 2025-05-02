using Metrics.Application.Domains;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Application.Interfaces.IServices;

public interface IUserRoleService
{
    Task<IdentityResult> RegisterRoleAsync(String rolename);
    Task<List<ApplicationRole>> FindAllAsync();

}
