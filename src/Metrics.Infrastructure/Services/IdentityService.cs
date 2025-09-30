using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Metrics.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Metrics.Infrastructure.Services;

public class IdentityService(
    ILogger<IdentityService> logger,
    MetricsDbContext context,
    UserManager<ApplicationUser> userManager
) : IIdentityService
{
    private readonly ILogger<IdentityService> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly MetricsDbContext _context = context;

    // public async Task<ResultT<bool>> IsUserInRole(string userId, List<ApplicationRole> userRoles)
    // {
    //     try
    //     {
    //         if (userRoles == null || userRoles.Count <= 0)
    //         {
    //             return ResultT<bool>.Fail("User title cannot be null or empty.", ErrorType.InvalidArgument);
    //         }
    //         var userId = _userManager.GetUserId(context.User);
    //         foreach (var role in userRoles)
    //         {
    //             await _userManager.IsInRoleAsync(user)
    //         }
    //         var userExists = await _userManager.Users
    //             .AnyAsync(user => user.Id == userId &&
    //                 userRoles.Select(role => role.Id)
    //                     .Contains(user.)
    //             );
    //         return ResultT<bool>.Success(userExists);
    //     }
    //     catch (System.Exception)
    //     {
    //         _logger.LogError("Checking failed Is User In Title.");
    //         return ResultT<bool>.Fail("Failed to check user in user title.", ErrorType.UnexpectedError);
    //     }
    // }

    public async Task<ResultT<bool>> IsUserInTitle(ApplicationUser user, List<string> userTitles)
    {
        try
        {
            if (userTitles == null || userTitles.Count <= 0)
            {
                return ResultT<bool>.Fail("User title cannot be null or empty.", ErrorType.InvalidArgument);
            }

            var userExists = await _userManager.Users
                .AnyAsync(u => u.Id == user.Id &&
                    userTitles.Select(t => t.ToLower())
                        .Contains(u.UserTitle.TitleName.ToLower()));

            return ResultT<bool>.Success(userExists);
        }
        catch (System.Exception)
        {
            _logger.LogError("Checking failed Is User In Title.");
            return ResultT<bool>.Fail("Failed to check user in user title.", ErrorType.UnexpectedError);
        }
    }
}
