using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Web.Pages.Manage.Users;

[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUserService userService,
        IDepartmentService departmentService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userService = userService;
        _departmentService = departmentService;
    }


    // =============== MODELS ==================================================
    public class UserModel
    {
        public string? UserName { get; set; } = string.Empty;
        public string? UserCode { get; set; } = string.Empty;
        public string? FullName { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public long DepartmentId { get; set; }
        public string? DepartmentName { get; set; } = string.Empty;
        public string? UserTitleName { get; set; } = string.Empty;
        public List<string> UserRoles { get; set; } = [];
        // public required string ApplicationUserId { get; set; }
        // public Department CurrentDepartment { get; set; } = null!;
        // public required ApplicationUser UserAccount { get; set; }
        // public List<KpiSubmission> KpiSubmissions { get; set; } = [];
        // public required ApplicationRole UserRole { get; set; }
    }

    public List<UserModel> UsersList { get; set; } = [];


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync()
    {
        // var users = await _userService.FindAllAsync();
        var usersList = await _userManager.Users
            .Include(u => u.Department)
            .Include(u => u.UserTitle)
            .ToListAsync();

        if (usersList.Any())
        {
            foreach (var user in usersList)
            {
                // var roles = await _userManager.GetRolesAsync(user.ApplicationUser);
                var roles = await _userManager.GetRolesAsync(user);
                var userModel = new UserModel
                {
                    UserName = user.UserName,
                    UserCode = user.UserCode,
                    FullName = user.FullName,
                    Address = user.ContactAddress,
                    PhoneNumber = user.PhoneNumber,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department.DepartmentName,
                    UserTitleName = user.UserTitle.TitleName,
                    UserRoles = roles.ToList()
                };
                UsersList.Add(userModel);
            }
        }

        return Page();
    }


}
