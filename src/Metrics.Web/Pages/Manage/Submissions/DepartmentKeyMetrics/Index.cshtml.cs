using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage.Submissions.DepartmentKeyMetrics;


public class IndexModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;

    public IndexModel(IKpiSubmissionPeriodService kpiPeriodService)
    {
        _kpiPeriodService = kpiPeriodService;
    }


    // ========== MODELS ==========
    public class KeyKpiViewModel
    {
        public string? MetricTitle { get; set; }
        public string? Description { get; set; }
        public string? KpiPeriod { get; set; }
    }

    public List<KeyKpiViewModel> KeyKpis { get; set; } = [];

    public List<KpiPeriodViewModel> KpiPeriodList { get; set; } = [];

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public long TotalItems { get; set; } // Count

    public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalItems, PageSize));
    public bool ShowPrevious => CurrentPage > 1;
    public bool ShowNext => CurrentPage < TotalPages;


    // ========== HANDLERS ==========
    public async Task OnGet()
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

    }
}
