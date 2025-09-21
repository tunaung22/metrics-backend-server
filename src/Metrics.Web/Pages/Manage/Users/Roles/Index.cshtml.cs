using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models.UserRoles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage.Users.Roles;

public class IndexModel : PageModel
{
    private readonly IUserRoleService _userRoleService;
    public IndexModel(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }


    // =============== MODELS ==================================================
    public List<RoleViewModel> UserRoleList { get; set; } = [];


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGet()
    {
        var roles = await _userRoleService.FindAllAsync();
        if (!roles.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "Failed to fetch user roles.");
            return Page();
        }

        if (roles.IsSuccess && roles.Data != null)
            UserRoleList = roles.Data.Select(r => r.MapToViewModel()).ToList();

        return Page();
    }
}
