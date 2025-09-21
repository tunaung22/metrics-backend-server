using Metrics.Application.Domains;
using Metrics.Application.DTOs.User;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Metrics.Web.Pages.Manage.Users;

public class IndexModel(
    IConfiguration config,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IUserService userService,
    IDepartmentService departmentService
) : PageModel
{
    private readonly IConfiguration _config = config;
    private readonly IUserService _userService = userService;
    private readonly IDepartmentService _departmentService = departmentService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

    // =============== MODELS ==================================================
    public class UserModel
    {
        public string? Id { get; set; } = string.Empty;
        public string? UserName { get; set; } = string.Empty;
        public string? UserCode { get; set; } = string.Empty;
        public string? FullName { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public long DepartmentId { get; set; }
        public string? DepartmentName { get; set; } = string.Empty;
        public string? UserTitleName { get; set; } = string.Empty;
        public List<string> UserRoles { get; set; } = [];
        public bool IsActive { get; set; }
        // public required string ApplicationUserId { get; set; }
        // public Department CurrentDepartment { get; set; } = null!;
        // public required ApplicationUser UserAccount { get; set; }
        // public List<KpiSubmission> KpiSubmissions { get; set; } = [];
        // public required ApplicationRole UserRole { get; set; }
    }

    public List<UserModel> UsersList { get; set; } = [];

    [BindProperty]
    public List<SelectListItem> DepartmentList { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Department { get; set; } = string.Empty; // filter by department

    // ============ Search =====================================================
    public string? Search { get; set; } = string.Empty;
    // ============ Pagination =================================================
    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;
    [BindProperty(SupportsGet = true)]
    public int Show { get; set; }
    public int PageSize { get; set; } = 20;
    public long TotalItems { get; set; } // Overall Count
    public long QueryResultCount { get; set; } = 0; // Current page result Count
    public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalItems, PageSize));
    public bool ShowPrevious => CurrentPage > 1;
    public bool ShowNext => CurrentPage < TotalPages;


    [TempData]
    public string? StatusMessage { get; set; }


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(
        [FromQuery] int currentPage,
        [FromQuery] int show,
        [FromQuery] string? searchQuery = "")
    {
        currentPage = currentPage < 1 ? 1 : currentPage;
        show = show < 1 ? 50 : show;
        // CurrentPage = currentPage;
        // Show = show;

        var users = new ResultT<List<UserDto>>();



        // ===== Query ============================================
        if (!string.IsNullOrEmpty(searchQuery))
        {
            // search view
            Search = searchQuery;
            users = await _userService.FindAllAsync(
               searchTerm: Search,
               pageNumber: currentPage,
               pageSize: show);
        }
        else
        {
            // non search view
            PageSize = _config.GetValue<int>("Pagination:PageSize");
            users = await _userService.FindAllAsync(
               pageNumber: currentPage,
               pageSize: show);
        }

        if (!users.IsSuccess || users.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to load users.");
            return Page();
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // self user id



        // DepartmentList
        if (string.IsNullOrEmpty(Department))
            Department = "All";
        var departments = await _departmentService.FindAllAsync();
        if (departments.Any())
        {
            DepartmentList = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentCode.ToString(),
                Text = $"{d.DepartmentName}"
            }).ToList();
        }

        // Department guid string to guid
        var isDepartmentCodeCorrect = Guid.TryParse(Department, out Guid departmentCode);

        var usersList = users.Data
            .Where(u =>
                u.Id != currentUserId &&
                (!isDepartmentCodeCorrect || u.Department.DepartmentCode == departmentCode)
            )
            .ToList();


        TotalItems = await _userService.FindCountAsync();
        QueryResultCount = usersList.Count;

        if (usersList.Any())
        {
            foreach (var user in usersList)
            {
                // var roles = await _userManager.GetRolesAsync(user.ApplicationUser);
                var roles = await _userManager.GetRolesAsync(new ApplicationUser
                {
                    Id = user.Id,
                    UserName = user.UserName
                });
                var userModel = new UserModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    UserCode = user.UserCode,
                    FullName = user.FullName,
                    Address = user.ContactAddress,
                    PhoneNumber = user.PhoneNumber,
                    DepartmentId = user.Department.Id,
                    DepartmentName = user.Department.DepartmentName,
                    UserTitleName = user.UserGroup.GroupName,
                    UserRoles = roles.ToList(),
                    IsActive =
                        user.LockoutEnabled == true &&
                        (user.LockoutEnd == null ||
                        user.LockoutEnd < DateTimeOffset.UtcNow)
                };
                UsersList.Add(userModel);
            }
        }

        return Page();
    }


}
