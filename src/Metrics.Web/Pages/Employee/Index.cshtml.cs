using Metrics.Application.Services.IServices;
using Metrics.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Metrics.Web.Pages.Employee
{
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;

        public IndexModel(IEmployeeService employeeService, IDepartmentService departmentService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            EmployeeList = [];
        }


        // ===== Models =====



        [BindProperty]
        public List<EmployeeModel> EmployeeList { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var result = await _employeeService.FindAll2_Async();
            EmployeeList = result.Select(e => new EmployeeModel
            {
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Address = e.Address,
                PhoneNumber = e.PhoneNumber,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.CurrentDepartment.DepartmentName,
                ApplicationUserId = e.ApplicationUserId
            }).ToList();

            // var result = await _employeeService.FindAll_Async();
            // var loadData = result.Select(async dto =>
            // {
            //     var department = await _departmentService.FindById_Async(dto.DepartmentId);
            //     return new EmployeeModel
            //     {
            //         EmployeeCode = dto.EmployeeCode,
            //         FullName = dto.FullName,
            //         Address = dto.Address,
            //         PhoneNumber = dto.PhoneNumber,
            //         DepartmentId = dto.DepartmentId,
            //         DepartmentName = department.DepartmentName,
            //         ApplicationUserId = dto.ApplicationUserId
            //     };
            // });
            // EmployeeList = (await Task.WhenAll(loadData)).ToList();

            return Page();
        }

        public class EmployeeModel
        {
            public string EmployeeCode { get; set; } = null!;
            public string FullName { get; set; } = null!;
            public string? Address { get; set; }
            public string? PhoneNumber { get; set; }
            public long DepartmentId { get; set; }
            public string DepartmentName { get; set; } = null!;
            public string ApplicationUserId { get; set; } = null!;
            // public Department CurrentDepartment { get; set; } = null!;
            // public ApplicationUser UserAccount { get; set; } = null!;
            // public List<KpiSubmission> KpiSubmissions { get; set; } = [];
        }
    }
}
