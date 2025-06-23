using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentKpi;

[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class IndexModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;

    public IndexModel(IKpiSubmissionPeriodService kpiPeriodService)
    {
        _kpiPeriodService = kpiPeriodService;
    }

    // ========== MODELS ==========
    public List<KpiPeriodViewModel> KpiPeriodList { get; set; } = [];

    // ========== HANDLERS ==========
    public async Task<PageResult> OnGet()
    {
        var periods = await _kpiPeriodService.FindAllAsync();
        if (periods.Any())
        {
            KpiPeriodList = periods.Select(p => new KpiPeriodViewModel()
            {
                Id = p.Id,
                PeriodName = p.PeriodName,
                SubmissionStartDate = p.SubmissionStartDate,
                SubmissionEndDate = p.SubmissionEndDate
            }).ToList();
        }

        return Page();
    }

}
