using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Kpi.Submissions;

// [Authorize(Roles = "Employee")]
public class IndexModel : PageModel
{
    private readonly IKpiPeriodService _kpiPeriodService;

    public IndexModel(IKpiPeriodService kpiPeriodService)
    {
        _kpiPeriodService = kpiPeriodService;
    }


    public DateTimeOffset SubmissionTime { get; set; } = DateTimeOffset.UtcNow;
    public List<KpiPeriodGetDto> KpiPeriodList { get; set; } = [];
    public bool IsSubmissionAvaiable { get; set; } = false;

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public long TotalSubmissions { get; set; } // Count

    public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalSubmissions, PageSize));
    public bool ShowPrevious => CurrentPage > 1;
    public bool ShowNext => CurrentPage < TotalPages;


    public async Task<IActionResult> OnGet()
    {

        var listItems = await LoadKpiPeriodListItems();
        if (listItems.Count == 0)
        {
            // ModelState.AddModelError("", "No KPI Periods Avaiable Currently");
            IsSubmissionAvaiable = false;
            return Page();
        }
        KpiPeriodList = listItems;
        IsSubmissionAvaiable = true;

        return Page();
    }


    private async Task<List<KpiPeriodGetDto>> LoadKpiPeriodListItems()
    {
        var kpiPeriods = await _kpiPeriodService.FindAllByDateAsync(SubmissionTime = DateTimeOffset.UtcNow);

        if (!kpiPeriods.Any())
            return [];

        return kpiPeriods.Select(e => new KpiPeriodGetDto
        {
            PeriodName = e.PeriodCode,
            SubmissionStartDate = e.SubmissionStartDate,
            SubmissionEndDate = e.SubmissionEndDate
        }).ToList();
    }
}
