using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage.KeyKpi;


public class IndexModel(
    IKpiSubmissionPeriodService kpiPeriodService,
    IDepartmentKeyMetricService departmentKeyMetricService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IDepartmentKeyMetricService _departmentKeyMetricService = departmentKeyMetricService;


    // ========== MODELS ==========
    public class KeyKpiViewModel
    {
        public string? MetricTitle { get; set; }
        public string? Description { get; set; }
        public string? KpiPeriod { get; set; }
    }

    public List<KeyKpiViewModel> KeyKpis { get; set; } = [];

    public List<KpiPeriodViewModel> KpiPeriodList { get; set; } = [];

    public class KpiPeriodWithStatusViewModel : KpiPeriodViewModel
    {
        public bool Active { get; set; }
    }
    public List<KpiPeriodWithStatusViewModel> KpiPeriodWithStatus { get; set; } = [];


    // Pagination
    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public long TotalItems { get; set; } // Count

    public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalItems, PageSize));
    public bool ShowPrevious => CurrentPage > 1;
    public bool ShowNext => CurrentPage < TotalPages;


    // ========== HANDLERS ==========
    public async Task OnGetAsync()
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

            // Check Issued Department Keys for each periods
            // foreach (var period in KpiPeriodList)
            // {
            //     var departmentKeyMetrics = await _departmentKeyMetricService.FindByPeriodIdAsync(period.Id);
            //     if (departmentKeyMetrics.IsSuccess)
            //     {
            //         if (departmentKeyMetrics.Data != null)
            //         {
            //             var p = departmentKeyMetrics.Data.Select(d => d.SubmissionPeriod).FirstOrDefault();
            //             if (p != null)
            //                 KpiPeriodWithStatus.Add(new KpiPeriodWithStatusViewModel
            //                 {
            //                     Id = p.Id,
            //                     PeriodName = p.PeriodName,
            //                     SubmissionStartDate = p.SubmissionStartDate,
            //                     SubmissionEndDate = p.SubmissionEndDate,
            //                     Active = true
            //                 });
            //         }
            //     }
            // }

            // if (departmentKeyMetrics.IsSuccess)
            // {
            //     if (departmentKeyMetrics.Data != null)
            //     {
            //         DepartmentKeyMetrics = departmentKeyMetrics.Data
            //             .Select(dkm => dkm.MapToViewModel())
            //             .ToList();
            //     }
            // }
            // else
            // {
            //     ModelState.AddModelError(string.Empty, "Failed to fetch department key metrics.");
            // }
        }


    }
}
