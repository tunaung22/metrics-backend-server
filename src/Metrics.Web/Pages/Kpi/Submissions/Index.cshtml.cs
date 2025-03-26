using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Kpi.Submissions;

[Authorize()]
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
        var kpiPeriods = await _kpiPeriodService.FindAllByValidDate_Async(SubmissionTime = DateTimeOffset.UtcNow);

        if (!kpiPeriods.Any())
            return [];

        return kpiPeriods.Select(e => new KpiPeriodGetDto
        {
            PeriodName = e.PeriodName,
            SubmissionStartDate = e.SubmissionStartDate,
            SubmissionEndDate = e.SubmissionEndDate
        }).ToList();
    }
}
