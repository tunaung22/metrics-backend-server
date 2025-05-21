using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Manage.Users;

[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class EditModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IDepartmentService _departmentService;

    public EditModel(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IDepartmentService departmentService
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _departmentService = departmentService;
    }

    public class InputModel
    {
        [Required]
        public long DepartmentId { get; set; }

        [Required(ErrorMessage = "User Role is required.")]
        public List<string> RoleIds { get; set; } = [];
    }

    [BindProperty]
    public required List<string> SelectedRoleIds { get; set; } = [];

    [BindProperty]
    public List<SelectListItem> DepartmentListItems { get; set; } = [];

    // public string? UserName { get; set; }

    public List<ApplicationRole> AvaiableRoles { get; set; } = [];

    public required InputModel Input { get; set; }
    public string? TargetUserCode { get; set; }
    public string ReturnUrl { get; set; }
    public ApplicationUser TargetUser { get; set; }

    // ========== HANDLERS ==============================
    public async Task<PageResult> OnGetAsync(string user, string? returnUrl)
    {
        AvaiableRoles = await LoadAvaiableRoles();
        DepartmentListItems = await LoadDepartmentList();


        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        if (string.IsNullOrEmpty(user))
        {
            ModelState.AddModelError("", "User not found.");
            return Page();
        }

        TargetUserCode = user;
        var selectedUser = await _userManager.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.UserCode == user);
        if (selectedUser != null)
            TargetUser = selectedUser;




        return Page();
    }

    public async Task<PageResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        AvaiableRoles = await LoadAvaiableRoles();
        DepartmentListItems = await LoadDepartmentList();

        return Page();
    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }
        return RedirectToPage("./Index");
    }

    // ========== METHODS ==========
    private async Task<List<SelectListItem>> LoadDepartmentList()
    {
        var departments = await _departmentService.FindAllAsync();
        if (departments.Any())
        {
            return departments.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.DepartmentName
            }).ToList();
        }

        ModelState.AddModelError("", "Departments not exist. Try to add department and continue.");
        return [];
    }
    private async Task<List<ApplicationRole>> LoadAvaiableRoles()
    {
        var roles = await _roleManager.Roles
            .Where(r => r.Name != "sysadmin")
            .ToListAsync();

        if (roles.Any())
        {
            return roles.Select(e => new ApplicationRole
            {
                Id = e.Id,
                Name = e.Name
            }).ToList();
        }

        ModelState.AddModelError("", "Roles does not exist.");
        return [];
    }

}
