using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metrics.Infrastructure.Services;

public class UserRoleService : IUserRoleService
{
    private readonly ILogger<UserRoleService> _logger;

    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserRoleService(ILogger<UserRoleService> logger,
        RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _roleManager = roleManager;
    }

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


    public async Task<List<ApplicationRole>> FindAllAsync()
    {
        try
        {
            return await _roleManager.Roles.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying roles.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }

    }
}
