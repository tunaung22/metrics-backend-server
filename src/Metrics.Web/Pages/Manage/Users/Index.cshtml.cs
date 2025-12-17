using Metrics.Application.Domains;
using Metrics.Application.DTOs.User;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using System.Security.Claims;

namespace Metrics.Web.Pages.Manage.Users;

public class IndexModel(
    Microsoft.Extensions.Configuration.IConfiguration config,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IUserService userService,
    IDepartmentService departmentService
) : PageModel
{
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config = config;
    private readonly IUserService _userService = userService;
    private readonly IDepartmentService _departmentService = departmentService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

    // =============== MODELS ==================================================
    public class UserModel
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string UserCode { get; set; } = null!;
        public string? FullName { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public string UserTitleName { get; set; } = null!;
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
    public long TotalUsers { get; set; } // Overall Count
    public long ActiveUsers { get; set; }
    public long QueryResultCount { get; set; } = 0; // Current page result Count
    public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalUsers, PageSize));
    public bool ShowPrevious => CurrentPage > 1;
    public bool ShowNext => CurrentPage < TotalPages;


    [TempData]
    public string? StatusMessage { get; set; }


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync()
    // [FromQuery] int currentPage,
    // [FromQuery] int show,
    // [FromQuery] string? searchQuery = "")
    {
        // currentPage = currentPage < 1 ? 1 : currentPage;
        // show = show < 1 ? 50 : show;
        var users = new ResultT<List<UserDto>>();
        // ===== Query ============================================
        // if (string.IsNullOrEmpty(searchQuery))
        //     Search = searchQuery;


        // users = await _userService.FindAllAsync(
        //     searchTerm: Search,
        //     pageNumber: currentPage,
        //     pageSize: show,
        //     includeLockedUser: true);
        users = await _userService.FindAll_Async(includeLockedUser: true);

        // if (!string.IsNullOrEmpty(searchQuery))
        // {
        //     // search view
        //     Search = searchQuery;
        //     users = await _userService.FindAllAsync(
        //        searchTerm: Search,
        //        pageNumber: currentPage,
        //        pageSize: show);
        // }
        // else
        // {
        //     // non search view
        //     PageSize = _config.GetValue<int>("Pagination:PageSize");
        //     users = await _userService.FindAllAsync(
        //        pageNumber: currentPage,
        //        pageSize: show);
        // }

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
        UsersList = await LoadUserListWithRoles(usersList);


        TotalUsers = await _userService.FindCountAsync(includeLockedUser: true);
        ActiveUsers = await _userService.FindCountAsync(includeLockedUser: false);
        QueryResultCount = usersList.Count;

        return Page();
    }

    private async Task<List<UserModel>> LoadUserListWithRoles(List<UserDto> users)
    {
        List<UserModel> userList = [];

        if (users.Count > 0)
        {
            foreach (var user in users)
            {
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
                        (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow)
                };
                userList.Add(userModel);
            }
        }
        return userList;
    }


    public async Task<IActionResult> OnPostExportExcelAsync(bool includeLockedUser = false)
    {

        var users = await _userService.FindAll_Async(includeLockedUser);
        if (!users.IsSuccess || users.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to load users.");
            return Page();
        }
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // self user id

        // dto to viewmodel
        // var usersList = result.Data.Select(u => u.MapToViewModel()).ToList();
        var usersList = users.Data
            .Where(u => u.Id != currentUserId)
            .ToList();
        UsersList = await LoadUserListWithRoles(usersList);


        string excelFileName = "";
        var memStream = new MemoryStream();

        if (includeLockedUser)
        {
            excelFileName = $"Export_UsersList_IncludesLockedUsers_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";
            memStream = await PrepareUserListExcelData(UsersList);
        }
        else
        {
            excelFileName = $"Export_UsersList_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";
            memStream = await PrepareUserListExcelData(UsersList);
        }

        return File(
            memStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            excelFileName
        );

    }

    private static async Task<MemoryStream> PrepareUserListExcelData(List<UserModel> users)
    {
        List<Dictionary<string, object>> excelData = [];
        var colUserCode = "User Code";
        var colUserName = "User Name";
        var colUserFullName = "Full Name";
        var colUserGroup = "User Group";
        var colDepartment = "Department";
        var colRole = "Role";
        var colLockStatus = "Status";

        var dynamicCols = new List<DynamicExcelColumn>()
            .Concat(
            [
                new(colUserCode) { Width = 25 },
                new(colUserName) { Width = 25 },
                new(colUserFullName) { Width = 26 },
                new(colUserGroup) { Width = 20 },
                new(colDepartment) { Width = 30 },
                new(colRole) { Width = 20 },
            ]).ToList();

        foreach (var user in users)
        {
            var row = new Dictionary<string, object>();
            row[colUserCode] = user.UserCode;
            row[colUserName] = user.UserName;
            row[colUserFullName] = user.FullName ?? string.Empty;
            row[colUserGroup] = user.UserTitleName;
            row[colDepartment] = user.DepartmentName;

            var roles = string.Join(" ", user.UserRoles.Select(r => r).ToArray()).Trim();

            row[colRole] = roles;
            row[colLockStatus] = user.IsActive ? "Active" : "Locked";
            excelData.Add(row);
        }

        var newStream = new MemoryStream();
        await MiniExcel.SaveAsAsync(
            stream: newStream,
            value: excelData,
            configuration: new OpenXmlConfiguration
            {
                DynamicColumns = dynamicCols.ToArray(),
                // TableStyles = TableStyles.None,
            }
        );
        newStream.Position = 0;

        return newStream;
    }
}
