using System.ComponentModel.DataAnnotations;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage.Departments;

public class IndexModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly IDepartmentService _departmentService;

    public IndexModel(
        IConfiguration config,
        IDepartmentService departmentService)
    {
        _config = config;
        _departmentService = departmentService;
    }


    // =============== MODELS ==================================================
    public class DepartmentModel
    {
        public Guid DepartmentCode { get; set; }
        public string DepartmentName { get; set; } = null!;
        public long NumberOfUsers { get; set; }
    }

    public class DepartmentInputModel
    {
        [Required(ErrorMessage = "Department name is required.")]
        [StringLength(200, ErrorMessage = "Department name must not exceed the length of 200 characters.")]
        public string DepartmentName { get; set; } = null!;
    }

    // =========================================================================


    [BindProperty]
    public DepartmentInputModel DepartmentInput { get; set; } = new DepartmentInputModel();

    public IEnumerable<DepartmentModel> Departments { get; set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    // =============== Pagination ==============================================
    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public long TotalDepartments { get; set; } // Count

    public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalDepartments, PageSize));
    public bool ShowPrevious => CurrentPage > 1;
    public bool ShowNext => CurrentPage < TotalPages;


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync()
    // [FromQuery] int currentPage,
    // [FromQuery] int pageSize)
    {
        // try
        // {
        PageSize = _config.GetValue<int>("Pagination:PageSize");
        TotalDepartments = await _departmentService.FindCountAsync();
        Departments = await LoadDepartments(CurrentPage, PageSize);

        return Page();
        // }
        // catch (Exception e)
        // {
        //     ModelState.AddModelError("", e.Message);
        //     return Page();
        // }
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {

        Departments = await LoadDepartments(CurrentPage, PageSize);

        if (!ModelState.IsValid)
        {
            return Page();
        }


        return Page();
    }

    // =============== HELPER METHODS ==========================================
    private async Task<List<DepartmentModel>> LoadDepartments(int currentPage, int pageSize)
    {
        var result = await _departmentService.FindAllAsync(
            pageNumber: currentPage,
            pageSize: pageSize);

        if (result.Any())
        {
            return result.Select(e => new DepartmentModel
            {
                DepartmentCode = e.DepartmentCode,
                DepartmentName = e.DepartmentName,
                NumberOfUsers = e.ApplicationUsers
                    .Where(u => u.UserName != "sysadmin")
                    .ToList()
                    .Count
            }).ToList();
        }

        return [];
    }

}
