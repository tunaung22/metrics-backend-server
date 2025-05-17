using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage.Roles;

public class IndexModel : PageModel
{
    private readonly IUserRoleService _userRoleService;
    public IndexModel(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }


    // =============== MODELS ==================================================
    public class UserRoleModel
    {
        public required string RoleName { get; set; }
        public required string Id { get; set; }
    }

    public List<UserRoleModel> UserRoleList { get; set; } = new List<UserRoleModel>();


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGet()
    {
        var roles = await _userRoleService.FindAllAsync();
        UserRoleList = roles.Select(r => new UserRoleModel()
        {
            RoleName = r.Name ?? string.Empty,
            Id = r.Id
        }).ToList();

        return Page();
    }
}
