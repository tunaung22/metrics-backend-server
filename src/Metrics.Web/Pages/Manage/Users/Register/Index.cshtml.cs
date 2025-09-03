using Metrics.Application.Domains;
using Metrics.Application.DTOs.User;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;

namespace Metrics.Web.Pages.Manage.Users.Register;

[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    // private readonly IUserAccountService _userAccountService;
    private readonly IUserService _userService;
    private readonly IUserTitleService _userTitleService;
    private readonly IDepartmentService _departmentService;

    public IndexModel(
        ILogger<IndexModel> logger,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        // IUserAccountService userAccountService,
        IUserService userService,
        IUserTitleService userTitleService,
        IDepartmentService departmentService)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
        // _userAccountService = userAccountS/ervice;
        _userService = userService;
        _userTitleService = userTitleService;
        _departmentService = departmentService;
    }

    // ========== MODELS ===============
    public class FormInputModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        [RegularExpression(@"^[a-z0-9]*$", ErrorMessage = "Username can only contain letters and numbers.")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = null!; // Use username instead of email

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Confirm-password is required.")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm-password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "User Code is required.")]
        [Display(Name = "User Code")]
        public string UserCode { get; set; } = null!;

        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public long DepartmentId { get; set; }

        [Required(ErrorMessage = "User Title is required.")]
        public long UserTitleId { get; set; }

        [Required(ErrorMessage = "User Role is required.")]
        public string RoleId { get; set; } = null!;
        // public List<string> RoleIds { get; set; } = [];
    }

    [BindProperty]
    public required FormInputModel FormInput { get; set; }

    [BindProperty]
    public List<SelectListItem> DepartmentListItems { get; set; } = null!;

    [BindProperty]
    public List<SelectListItem> UserTitleListItems { get; set; } = null!;

    [BindProperty]
    public List<SelectListItem> RoleListItems { get; set; } = [];

    // public long SelectedDepartmentId { get; set; }
    // public string SelectedRoleId { get; set; } = string.Empty;
    // [BindProperty]
    // public required List<string> SelectedRoleIds { get; set; } = [];
    public List<ApplicationRole> AvaiableRoles { get; set; } = [];
    // public string? SelectedDepartmentName { get; set; }

    public string ReturnUrl { get; set; } = string.Empty;


    // ========== HANDLERS ==============================
    public async Task<IActionResult> OnGetAsync()
    {
        DepartmentListItems = await LoadDepartmentList();
        UserTitleListItems = await LoadUserTitleList();
        RoleListItems = await LoadRoleList();
        AvaiableRoles = await LoadAvaiableRoles();

        // RoleListItems = await LoadRoleList();
        // if (RoleListItems.Count <= 0)
        //     return Page();
        if (UserTitleListItems.Any())
        {
            var defaultTitle = UserTitleListItems?
                .Where(t => t.Text.ToLower() == "staff")
                .FirstOrDefault();
            var defaultRole = RoleListItems?
                .Where(t => t.Text.ToLower() == "staff")
                .FirstOrDefault();

            FormInput = new FormInputModel
            {
                UserTitleId = defaultTitle?.Value != null ? long.Parse(defaultTitle.Value) : 0,
                RoleId = defaultRole?.Value != null ? defaultRole.Value : string.Empty,
            };
        }


        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // load departments
        // load accounts
        // save user
        try
        {
            var createDto = new UserCreateDto
            {
                // account
                UserName = FormInput.UserName.ToLower(),
                Email = FormInput.Email,
                Password = FormInput.Password,
                // profile
                UserCode = FormInput.UserCode,
                FullName = FormInput.FullName,
                Address = FormInput.Address,
                PhoneNumber = FormInput.PhoneNumber,
                DepartmentId = FormInput.DepartmentId,
                UserTitleId = FormInput.UserTitleId,
                // RoleId = FormInput.RoleId,
                // RoleIds = FormInput.RoleIds
                RoleIds = new List<string> { FormInput.RoleId }
            };

            // var result = await _userAccountService.RegisterUserAsync(createDto);
            // ----- GET User Group Name -------------------------------
            // ----- 1. CREATE user
            // ----- 2. ADD Claim -----------------------------------------
            var result = await _userService.RegisterUserAsync(createDto);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(createDto.UserName);

                if (user != null)
                {
                    var groupName = "";
                    var userTitle = await _userTitleService.FindByIdAsync(createDto.UserTitleId);
                    if (userTitle != null)
                        groupName = userTitle.TitleName;

                    // var assignedRole = await _roleManager.FindByIdAsync(createDto.RoleId);
                    List<string> assignedRoles = [];
                    for (int i = 0; i < createDto.RoleIds.Count; i++) // foreach cannot execute async operation
                    {
                        var role = await _roleManager.FindByIdAsync(createDto.RoleIds[i]);
                        if (role != null)
                            assignedRoles.Add(role.Name!);
                    }

                    TempData["Username"] = createDto.UserName.ToString();
                    TempData["FullName"] = createDto.FullName.ToString();
                    TempData["GroupName"] = groupName;
                    // TempData["RoleName"] = assignedRole?.Name ?? string.Empty;
                    TempData["AssignedRoles"] = JsonSerializer.Serialize(assignedRoles);
                }

                return RedirectToPage("Success");
            }

            // Handle errors
            foreach (var error in result.Errors)
            {
                if (error.Code == "DuplicateUserName")
                    ModelState.AddModelError("FormInput.UserName", $"This username is already in use.");
                else if (error.Code == "DuplicateEmail")
                    ModelState.AddModelError("FormInput.Email", $"This email is already use.");
                else if (error.Code == "DuplicateUserCode")
                    ModelState.AddModelError("FormInput.UserCode", $"This staff id is already in use.");
                else
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            if (!ModelState.IsValid)
            {
                DepartmentListItems = await LoadDepartmentList();
                UserTitleListItems = await LoadUserTitleList();
                RoleListItems = await LoadRoleList();
                AvaiableRoles = await LoadAvaiableRoles();
            }

            return Page();
        }
        catch (MetricsDuplicateContentException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            // Load Select Items & Checkbox Data
            DepartmentListItems = await LoadDepartmentList();
            UserTitleListItems = await LoadUserTitleList();
            RoleListItems = await LoadRoleList();
            AvaiableRoles = await LoadAvaiableRoles();
            // FormInput.RoleIds = SelectedRoleIds;

            return Page();
        }
        catch (MetricsNotFoundException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            // Load Select Items & Checkbox Data
            DepartmentListItems = await LoadDepartmentList();
            UserTitleListItems = await LoadUserTitleList();
            RoleListItems = await LoadRoleList();
            AvaiableRoles = await LoadAvaiableRoles();
            // FormInput.RoleIds = SelectedRoleIds;

            return Page();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e.Message);
            ModelState.AddModelError(string.Empty, "Unexpected error occured. " + e.Message);
            // Load Select Items & Checkbox Data
            DepartmentListItems = await LoadDepartmentList();
            UserTitleListItems = await LoadUserTitleList();
            RoleListItems = await LoadRoleList();
            AvaiableRoles = await LoadAvaiableRoles();
            // FormInput.RoleIds = SelectedRoleIds;
            // RoleListItems = await LoadRoleList();

            return Page();
            // throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
            //        $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
            //        $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
            return LocalRedirect(ReturnUrl);

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

    private async Task<List<SelectListItem>> LoadRoleList()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        if (roles.Any())
        {
            return roles.Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = e.Name
            }).ToList();
        }

        ModelState.AddModelError("", "Roles does not exist.");
        return [];
    }

    private async Task<List<SelectListItem>> LoadUserTitleList()
    {
        var query = await _userTitleService.FindAllAsync();
        var userTitles = query
            .OrderBy(t => t.TitleName == "Staff" ? 0 : 1)
            .ThenBy(t => t.TitleName)
            .ToList();
        if (userTitles.Any())
        {
            return userTitles.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.TitleName
            }).ToList();
        }

        ModelState.AddModelError("", "User Titles not exist. Try to add user title and continue.");
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
