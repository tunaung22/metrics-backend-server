using Metrics.Application.Domains;
using Metrics.Application.DTOs.UserAccountDtos;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Employee;

[Authorize(Roles = "Admin")]
public class RegisterModel : PageModel
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserAccountService _userAccountService;
    private readonly IDepartmentService _departmentService;

    public RegisterModel(
        RoleManager<ApplicationRole> roleManager,
        IUserAccountService userAccountService,
        IDepartmentService departmentService)
    {
        _roleManager = roleManager;
        _userAccountService = userAccountService;
        _departmentService = departmentService;
    }

    // ========== MODELS ===============
    public class FormInputModel
    {
        [Required(ErrorMessage = "Username is required.")]
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
        public required string RoleId { get; set; }
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
        RoleId = string.Empty
    };
    [BindProperty]
    public List<SelectListItem> DepartmentListItems { get; set; } = [];
    [BindProperty]
    public List<SelectListItem> RoleListItems { get; set; } = [];
    // public long SelectedDepartmentId { get; set; }
    // public string SelectedRoleId { get; set; } = string.Empty;


    // ========== HANDLERS ==============================
    public async Task<IActionResult> OnGetAsync()
    {
        DepartmentListItems = await LoadDepartmentList();
        if (DepartmentListItems.Count <= 0)
            return Page();

        RoleListItems = await LoadRoleList();
        if (RoleListItems.Count <= 0)
            return Page();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            DepartmentListItems = await LoadDepartmentList();
            if (DepartmentListItems.Count <= 0)
                return Page();

            RoleListItems = await LoadRoleList();
            if (RoleListItems.Count <= 0)
                return Page();

            return Page();

        }
        // load departments
        // load accounts
        // save employee
        try
        {
            var createDto = new UserAccountCreateDto
            {
                // account
                UserName = FormInput.UserName,
                Email = FormInput.Email,
                Password = FormInput.Password,
                // profile
                EmployeeCode = FormInput.EmployeeCode,
                FullName = FormInput.FullName,
                Address = FormInput.Address,
                PhoneNumber = FormInput.PhoneNumber,
                DepartmentId = FormInput.DepartmentId
                // ApplicationUserId??? <- account not created then id unknown 
            };

            var result = await _userAccountService.RegisterUserAsync(createDto);
            if (result.Succeeded)
            {
                TempData["Username"] = createDto.UserName.ToString();
                TempData["FullName"] = createDto.FullName.ToString();

                return RedirectToPage("Success");
            }

            return Page();
        }
        catch (Exception e)
        {
            ModelState.AddModelError(string.Empty, e.Message);

            return Page();
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
}
