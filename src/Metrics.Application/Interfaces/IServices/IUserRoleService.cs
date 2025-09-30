using Metrics.Application.Domains;
using Metrics.Application.DTOs.Role;
using Metrics.Application.Results;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Application.Interfaces.IServices;

public interface IUserRoleService
{
    Task<IdentityResult> RegisterRoleAsync(String rolename);
    Task<ResultT<List<RoleDto>>> FindAllAsync();

    Task<ResultT<List<string>>> FindUserRoleAsync(string userId);
}
