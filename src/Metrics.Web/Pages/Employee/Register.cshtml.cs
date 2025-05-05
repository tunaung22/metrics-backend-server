using Metrics.Application.Domains;
using Metrics.Application.DTOs.UserAccountDtos;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Employee;

[Authorize(Roles = "Admin")]
public class RegisterModel : PageModel
{
    private readonly ILogger<RegisterModel> _logger;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserAccountService _userAccountService;
    private readonly IDepartmentService _departmentService;

    public RegisterModel(
        ILogger<RegisterModel> logger,
        RoleManager<ApplicationRole> roleManager,
        IUserAccountService userAccountService,
        IDepartmentService departmentService)
    {
        _logger = logger;
        _roleManager = roleManager;
        _userAccountService = userAccountService;
        _departmentService = departmentService;
    }

    // ========== MODELS ===============
    public class FormInputModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Username can only contain letters and numbers.")]
        [Display(Name = "Username")]
        public required string UserName { get; set; } // Use username instead of email

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm-password is required.")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm-password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Employee Code is required.")]
        [Display(Name = "Employee Code")]
        public required string EmployeeCode { get; set; }

        [Required(ErrorMessage = "Employee name is required.")]
        [Display(Name = "Full Name")]
        public required string FullName { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public long DepartmentId { get; set; }

        [Required(ErrorMessage = "User Role is required.")]
        public List<string> RoleIds { get; set; } = [];
    }

    [BindProperty]
    public FormInputModel FormInput { get; set; } = new FormInputModel()
    {
        UserName = string.Empty,
        Email = string.Empty,
        Password = string.Empty,
        ConfirmPassword = string.Empty,
        EmployeeCode = string.Empty,
        FullName = string.Empty,
        // RoleIds = []
    };

    [BindProperty]
    public List<SelectListItem> DepartmentListItems { get; set; } = null!;
    public string? SelectedDepartmentName { get; set; }
    // [BindProperty]
    // public List<SelectListItem> RoleListItems { get; set; } = [];

    // public long SelectedDepartmentId { get; set; }
    // public string SelectedRoleId { get; set; } = string.Empty;
    [BindProperty]
    public required List<string> SelectedRoleIds { get; set; } = [];
    public List<ApplicationRole> AvaiableRoles { get; set; } = [];



    // ========== HANDLERS ==============================
    public async Task<IActionResult> OnGetAsync()
    {
        DepartmentListItems = await LoadDepartmentList();
        if (DepartmentListItems.Count <= 0)
            return Page();

        // RoleListItems = await LoadRoleList();
        // if (RoleListItems.Count <= 0)
        //     return Page();

        AvaiableRoles = await LoadAvaiableRoles();
        if (AvaiableRoles.Count <= 0)
            return Page();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // load departments
        // load accounts
        // save employee
        // if (!ModelState.IsValid)
        // {
        //     DepartmentListItems = await LoadDepartmentList();
        //     AvaiableRoles = await LoadAvaiableRoles();
        //     return Page();
        // }

        try
        {
            var createDto = new UserAccountCreateDto
            {
                // account
                UserName = FormInput.UserName.ToLower(),
                Email = FormInput.Email,
                Password = FormInput.Password,
                // profile
                EmployeeCode = FormInput.EmployeeCode,
                FullName = FormInput.FullName,
                Address = FormInput.Address,
                PhoneNumber = FormInput.PhoneNumber,
                DepartmentId = FormInput.DepartmentId,
                // RoleId = FormInput.RoleId,
                RoleIds = SelectedRoleIds
                // RoleIds = FormInput.RoleIds
                // ApplicationUserId??? <- account not created then id unknown 
            };

            var result = await _userAccountService.RegisterUserAsync(createDto);
            if (result.Succeeded)
            {
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
                // TempData["RoleName"] = assignedRole?.Name ?? string.Empty;
                TempData["AssignedRoles"] = JsonConvert.SerializeObject(assignedRoles);

                return RedirectToPage("Success");
            }

            // Handle errors
            foreach (var error in result.Errors)
            {
                if (error.Code == "DuplicateUserName")
                {
                    ModelState.AddModelError("FormInput.UserName", "Username is already taken.");
                }
                else if (error.Code == "DuplicateEmail")
                {
                    ModelState.AddModelError("FormInput.Email", "Email is already registered.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }

            // Load Select Items & Checkbox Data
            DepartmentListItems = await LoadDepartmentList();
            AvaiableRoles = await LoadAvaiableRoles();
            // FormInput.RoleIds = SelectedRoleIds;

            return Page();
        }
        catch (MetricsDuplicateContentException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            // Load Select Items & Checkbox Data
            DepartmentListItems = await LoadDepartmentList();
            AvaiableRoles = await LoadAvaiableRoles();
            // FormInput.RoleIds = SelectedRoleIds;

            return Page();
        }
        catch (MetricsNotFoundException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            // Load Select Items & Checkbox Data
            DepartmentListItems = await LoadDepartmentList();
            AvaiableRoles = await LoadAvaiableRoles();
            // FormInput.RoleIds = SelectedRoleIds;

            return Page();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e.Message);
            ModelState.AddModelError(string.Empty, "Unexpected error occured." + e.Message);
            // Load Select Items & Checkbox Data
            DepartmentListItems = await LoadDepartmentList();
            AvaiableRoles = await LoadAvaiableRoles();
            // FormInput.RoleIds = SelectedRoleIds;
            // RoleListItems = await LoadRoleList();

            return Page();
            // throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
            //        $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
            //        $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
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

    // private async Task<List<SelectListItem>> LoadRoleList()
    // {
    //     var roles = await _roleManager.Roles.ToListAsync();
    //     if (roles.Any())
    //     {
    //         return roles.Select(e => new SelectListItem
    //         {
    //             Value = e.Id,
    //             Text = e.Name
    //         }).ToList();
    //     }

    //     ModelState.AddModelError("", "Roles does not exist.");
    //     return [];
    // }

    private async Task<List<ApplicationRole>> LoadAvaiableRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
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
