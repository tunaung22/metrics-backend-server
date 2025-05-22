using Metrics.Application.Domains;
using Metrics.Application.DTOs.AccountDtos;
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
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;
    private readonly IUserTitleService _userTitleService;

    public EditModel(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUserService userService,
        IDepartmentService departmentService,
        IUserTitleService userTitleService
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userService = userService;
        _departmentService = departmentService;
        _userTitleService = userTitleService;
    }

    public class InputModel
    {
        [Required]
        public string UserCode { get; set; } = null!;
        [Required]
        public long DepartmentId { get; set; }
        [Required]
        public long UserTitleId { get; set; }
        [Required(ErrorMessage = "User Role is required.")]
        public string RoleName { get; set; } = null!; // role id not used.
        // public List<string>? Roles { get; set; }
    }

    [BindProperty]
    public required InputModel Input { get; set; } // form

    [BindProperty]
    public List<SelectListItem> DepartmentListItems { get; set; } = []; // for select element
    [BindProperty]
    public List<SelectListItem> UserTitleListItems { get; set; } = []; // for select element
    [BindProperty]
    public List<SelectListItem> RoleListItems { get; set; } = []; // for select element
    [BindProperty]
    public string SelectedUserId { get; set; } = null!; // for get and post
    public ApplicationUser TargetUser { get; set; }

    public string ReturnUrl { get; set; } // return url

    // public string? UserName { get; set; }
    // public List<ApplicationRole> AvaiableRoles { get; set; } = [];
    // [BindProperty]
    // public required List<string> SelectedRoleIds { get; set; } = [];

    // ========== HANDLERS ==============================
    public async Task<PageResult> OnGetAsync(string userId, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError("", "Invalid User ID.");
            return Page();
        }

        // AvaiableRoles = await LoadAvaiableRoles();
        DepartmentListItems = await LoadDepartmentListItems();
        UserTitleListItems = await LoadUserTitleListItems();
        RoleListItems = await LoadRoleListItems();

        var selectedUser = await _userService.FindByIdAsync(userId);
        if (selectedUser == null)
            ModelState.AddModelError(string.Empty, "User not found.");
        else
        {
            TargetUser = selectedUser;
            SelectedUserId = selectedUser.Id;
            var roleNames = await _userManager.GetRolesAsync(selectedUser);

            if (roleNames.Count <= 0)
            {
                ModelState.AddModelError(string.Empty, "User does not have role assigned.");
                return Page();
            }
            else
            {
                // var role = await _roleManager.FindByNameAsync(roleNames[0]);
                // if (role == null)
                // {
                //     ModelState.AddModelError(string.Empty, "Role not found.");
                //     return Page();
                // }

                Input = new InputModel
                {
                    UserCode = selectedUser.UserCode,
                    DepartmentId = selectedUser.DepartmentId,
                    UserTitleId = selectedUser.UserTitleId,
                    RoleName = roleNames[0], // only accept 1 role per user
                };
            }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            DepartmentListItems = await LoadDepartmentListItems();
            UserTitleListItems = await LoadUserTitleListItems();
            RoleListItems = await LoadRoleListItems();
            // AvaiableRoles = await LoadAvaiableRoles();
            return Page();
        }

        // update user
        List<string> roleNames = [];
        roleNames.Add(Input.RoleName);

        var updateDto = new UserUpdateDto
        {
            UserCode = Input.UserCode,
            DepartmentId = Input.DepartmentId,
            UserTitleId = Input.UserTitleId,
            RoleNames = roleNames
        };

        try
        {
            var updatedUser = await _userService.UpdateAsync(SelectedUserId, updateDto);

            var returnUrl = ViewData["ReturnUrl"] as string;
            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToPage("./Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Update user failed.");
        }

        return Page();
    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToPage("Index");
    }

    // ========== METHODS ==========
    private async Task<List<SelectListItem>> LoadDepartmentListItems()
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

    private async Task<List<SelectListItem>> LoadUserTitleListItems()
    {
        var userTitles = await _userTitleService.FindAllAsync();
        if (userTitles.Any())
        {
            return userTitles.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.TitleName
            }).ToList();
        }

        ModelState.AddModelError("", "User group not exist. Try to add group and continue.");
        return [];
    }

    private async Task<List<SelectListItem>> LoadRoleListItems()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        if (roles.Any())
        {
            return roles.Select(e => new SelectListItem
            {
                Value = e.Name,
                Text = e.Name
            }).ToList();
        }

        ModelState.AddModelError("", "Roles does not exist. Try to add role and continue.");
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
