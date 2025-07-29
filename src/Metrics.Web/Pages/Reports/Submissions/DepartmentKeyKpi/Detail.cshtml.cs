using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentKeyKpi;

public class DetailModel : PageModel
{

    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;
    private readonly IKeyKpiSubmissionService _keyKpiSubmissionService;

    public DetailModel(
        IKpiSubmissionPeriodService kpiSubmissionPeriodService,
        IUserService userService,
        IDepartmentService departmentService,
        IKeyKpiSubmissionService keyKpiSubmissionService)
    {
        _kpiPeriodService = kpiSubmissionPeriodService;
        _userService = userService;
        _departmentService = departmentService;
        _keyKpiSubmissionService = keyKpiSubmissionService;
    }


    // ========== MODELS =======================================================
    public class KeyKpiSubmissionViewModel
    {

    }

    public List<KeyKpiSubmissionViewModel> KeyKpiSubmissions { get; set; } = [];

    public class KeyKpiSubmissionExportViewModel
    {

    }

    // public List<KeyKpiSubmissionExportViewModel>

    public KpiPeriodViewModel SelectedPeriod { get; set; } = new();

    public string SelectedPeriodName { get; set; } = null!;

    // public string? Submitter { get; set; }


    // ========== HANDLERS =======================================================

    public async Task<IActionResult> OnGet(string periodName)
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }

        // CHECK periodName exist in submissions
        var period = await LoadKpiPeriod(periodName);
        if (period != null)
        {
            SelectedPeriod = period;
            SelectedPeriodName = period.PeriodName;
        }
        else
        {
            ModelState.AddModelError("", $"Period {periodName} not found.");
            return Page();
        }

        try
        {
            var keyKpiSubmissions = await _keyKpiSubmissionService
                .FindByKpiPeriodAsync(SelectedPeriod.Id);
            if (keyKpiSubmissions.Count > 0)
            {
                // entity to viewModel
                // CaseFeedbackSubmissions = ToViewModels(keyKpiSubmissions);
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Error fetching department case feedbacks.");
        }

        return Page();
    }

    // ========== Methods ==================================================
    private async Task<KpiPeriodViewModel?> LoadKpiPeriod(string periodName)
    {
        var kpiPeriod = await _kpiPeriodService
            .FindByKpiPeriodNameAsync(periodName);

        if (kpiPeriod != null)
        {
            return new KpiPeriodViewModel()
            {
                Id = kpiPeriod.Id,
                PeriodName = kpiPeriod.PeriodName,
                SubmissionStartDate = kpiPeriod.SubmissionStartDate,
                SubmissionEndDate = kpiPeriod.SubmissionEndDate
            };
        }

        return new KpiPeriodViewModel();
    }
}
