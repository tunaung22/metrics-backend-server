using Metrics.Application.Common.Mappers;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.Role;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metrics.Infrastructure.Services;

public class UserRoleService(
    ILogger<UserRoleService> logger,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager)
    : IUserRoleService
{
    private readonly ILogger<UserRoleService> _logger = logger;

    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

    public async Task<IdentityResult> RegisterRoleAsync(string rolename)
    {
        try
        {
            var roleInstance = new ApplicationRole
            {
                Name = rolename,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            return await _roleManager.CreateAsync(roleInstance);

            // if (!newRole.Succeeded)
            //     return newRole;

            // return IdentityResult.Success;
        }
        catch (Exception)
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationRole)}'.");
        }
    }


    public async Task<ResultT<List<RoleDto>>> FindAllAsync()
    {
        try
        {
            var roles = await _roleManager.Roles
                .Where(r => r.Name != "sysadmin")
                .ToListAsync();
            var result = roles.Select(r => r.MapToDto()).ToList();

            return ResultT<List<RoleDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying roles.");
            return ResultT<List<RoleDto>>.Fail("Failed to fetch user roles.", ErrorType.UnexpectedError);
        }

    }

    public async Task<ResultT<List<string>>> FindUserRoleAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ResultT<List<string>>.Success([]);

            var userRoles = await _userManager.GetRolesAsync(user);

            var roles = userRoles.Select(role => role).ToList();
            return ResultT<List<string>>.Success(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find user roles by user id. {msg}", ex.Message);
            return ResultT<List<string>>.Fail("Failed to find user roles by user id.", ErrorType.UnexpectedError);
        }
    }
}
