using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage.Submissions.DepartmentKeyMetrics;

public class ViewModel : PageModel
{
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionPeriodService _kpiSubmissionPeriodService;
    private readonly IDepartmentKeyMetricService _departmentKeyMetricService;

    public ViewModel(
        IDepartmentService departmentService,
        IKpiSubmissionPeriodService kpiSubmissionPeriodService,
        IDepartmentKeyMetricService departmentKeyMetricService)
    {
        _departmentService = departmentService;
        _kpiSubmissionPeriodService = kpiSubmissionPeriodService;
        _departmentKeyMetricService = departmentKeyMetricService;
    }


    // =============== MODELS ==================================================
    public class DepartmentKeyMetricViewModel
    {
        public long Id { get; set; }
        public Guid DepartmentKeyMetricCode { get; set; }
        public string? PeriodName { get; set; }
        public string? DepartmentName { get; set; }
        public string? KeyMetricTitle { get; set; }
    }

    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];

    public required string CurrentPeriodName { get; set; }

    // [BindProperty]
    // public List<SelectListItem> DepartmentListItems { get; set; } = [];

    // public string? Department { get; set; } = string.Empty;


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(string periodName, string department)
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }
        CurrentPeriodName = periodName;

        // Departments
        var departments = await _departmentService.FindAllAsync();
        if (departments == null)
        {
            ModelState.AddModelError(string.Empty, "No Department exist.");
            return Page();
        }
        // DepartmentListItems = departments.Select(d => new SelectListItem
        // {
        //     Value = d.Id.ToString(),
        //     Text = d.DepartmentName
        // }).ToList();

        // KPI Period
        var period = await _kpiSubmissionPeriodService.FindByKpiPeriodNameAsync(CurrentPeriodName);
        if (period == null)
        {
            ModelState.AddModelError(string.Empty, "No Period exist.");
            return Page();
        }
        // Department Key Metrics
        var keyMetrics = await _departmentKeyMetricService.FindByPeriodIdAsync(period.Id);
        if (keyMetrics.Any())
        {
            DepartmentKeyMetrics = keyMetrics.Select(k => new DepartmentKeyMetricViewModel
            {
                Id = k.Id,
                DepartmentKeyMetricCode = k.DepartmentKeyMetricCode,
                PeriodName = k.KpiSubmissionPeriod.PeriodName,
                DepartmentName = k.TargetDepartment.DepartmentName,
                KeyMetricTitle = k.KeyMetric.MetricTitle,
            }).ToList();
        }

        return Page();
    }
}