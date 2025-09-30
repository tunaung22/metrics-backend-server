using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models.UserClaims;
using Metrics.Web.Models.UserRoles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Metrics.Web.Pages.Manage.Users.Permissions;

public class IndexModel(
    IUserService userService,
    IUserRoleService roleService) : PageModel
{
    private readonly IUserService _userService = userService;
    private readonly IUserRoleService _roleService = roleService;

    public List<RoleViewModel> Roles { get; set; } = [];

    public List<UserClaimViewModel> UserClaims { get; set; } = [];

    [BindProperty]
    public List<SelectListItem> RoleListItems { get; set; } = [];
    [BindProperty]
    public List<SelectListItem> UserRoleListItems { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string SelectedRole { get; set; } = null!;

    // TODO: TEMP
    public long UserCount { get; set; } = 0;


    public async Task<IActionResult> OnGet(int page = 1, int show = 50)
    {
        var usersList = await _userService.FindAllAsync(pageNumber: page, pageSize: show);

        if (!usersList.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "Failed to fetch users.");
            return Page();
        }

        if (usersList.Data != null)
        {
            {
                UserCount = usersList.Data.Count;
                var userIDs = usersList.Data.Select(userId => userId.Id).ToList();
                var userClaims = await _userService.GetUserClaimsAsync(userIDs);
                if (!userClaims.IsSuccess)
                {
                    ModelState.AddModelError(string.Empty, "Failed to fetch users claims.");
                    return Page();
                }

                if (userClaims.Data != null)
                {
                    if (userClaims.Data.Count > 0)
                    {
                        UserClaims = userClaims.Data
                            .Select(uc => uc.MapToViewModel()).ToList();
                    }
                }
            }
        }
        else
        {
            // skip
            UserCount = -1;
        }




        var roles = await _roleService.FindAllAsync();
        if (!roles.IsSuccess)
            ModelState.AddModelError(string.Empty, "Failed to fetch user roles.");

        if (roles.IsSuccess && roles.Data != null)
            Roles = roles.Data.Select(r => r.MapToViewModel()).ToList();

        RoleListItems = Roles.Select(role => new SelectListItem
        {
            Value = role.Id.ToString(),
            Text = role.RoleName

        }).ToList();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            ModelState.AddModelError(string.Empty, "Authorized user not found.");
            return Page();
        }

        return Page();
    }
}
