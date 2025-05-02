using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Employee;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IEmployeeService employeeService,
        IDepartmentService departmentService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _employeeService = employeeService;
        _departmentService = departmentService;
    }


    // =============== MODELS ==================================================
    public class EmployeeModel
    {
        public required string EmployeeCode { get; set; }
        public required string FullName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public long DepartmentId { get; set; }
        public required string DepartmentName { get; set; }
        public required string ApplicationUserId { get; set; }
        // public Department CurrentDepartment { get; set; } = null!;
        public required ApplicationUser UserAccount { get; set; }
        // public List<KpiSubmission> KpiSubmissions { get; set; } = [];
        // public required ApplicationRole UserRole { get; set; }
        public List<string> UserRoles { get; set; } = [];
    }

    public List<EmployeeModel> EmployeeList { get; set; } = [];


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGet()
    {
        var employees = await _employeeService.FindAllAsync();

        if (employees.Any())
        {
            foreach (var employee in employees)
            {
                var roles = await _userManager.GetRolesAsync(employee.UserAccount);
                var employeeModel = new EmployeeModel
                {
                    EmployeeCode = employee.EmployeeCode,
                    FullName = employee.FullName,
                    Address = employee.Address,
                    PhoneNumber = employee.PhoneNumber,
                    DepartmentId = employee.DepartmentId,
                    DepartmentName = employee.CurrentDepartment.DepartmentName,
                    ApplicationUserId = employee.ApplicationUserId,
                    UserAccount = employee.UserAccount,
                    // UserRole = roles
                    UserRoles = roles.ToList()
                };

                EmployeeList.Add(employeeModel);

            }
        }

        return Page();
    }


}
