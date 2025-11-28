using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models.DepartmentKeyMetric;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Manage.Submissions.KeyKpi.DepartmentKeys.IssueKeys;

public class ImportModel(
    IKpiSubmissionPeriodService kpiPeriodService,
    IDepartmentKeyMetricService dkmService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IDepartmentKeyMetricService _dkmService = dkmService;


    // ============================================================
    [BindProperty]
    public string? TargetPeriodName { get; set; }

    [BindProperty(Name = "sourcePeriod", SupportsGet = true)]
    public string? SourcePeriodName { get; set; }

    [BindProperty]
    public List<SelectListItem> SourcePeriodListItems { get; set; } = [];

    public List<DepartmentKeyMetricViewModel> SourceDKMs { get; set; } = [];

    [BindProperty]
    public string? ReturnUrl { get; set; } = string.Empty;


    // ============================================================
    public async Task<IActionResult> OnGetAsync(string periodName, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
            return Page();
        }

        // find period by name
        var targetPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(Uri.UnescapeDataString(periodName));
        if (targetPeriod != null)
            TargetPeriodName = targetPeriod.PeriodName;
        else
            ModelState.AddModelError(string.Empty, "Unknown Target Period.");

        var allPeriodsResult = await _kpiPeriodService.FindAll_Async();
        if (!allPeriodsResult.IsSuccess || allPeriodsResult.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to load KPI periods.");
            return Page();
        }

        SourcePeriodListItems = allPeriodsResult.Data
            .Where(p => p.PeriodName != TargetPeriodName)
            .Select((p, index) => new SelectListItem
            {
                Value = p.PeriodName,
                Text = p.PeriodName
            }).ToList();

        if (string.IsNullOrEmpty(SourcePeriodName))
        {
            SourcePeriodName = allPeriodsResult.Data.First().PeriodName;
            // TempData["CurrentDepartmentCode"] = SourcePeriodName;
        }

        // load key metircs data by selected source period
        SourceDKMs = await LoadDepartmentKeysByPeriod(SourcePeriodName);
        // ModelState.AddModelError(string.Empty, "No source KPI Period.");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        [FromRoute] string periodName,
        [FromQuery] string sourcePeriod)
    //, string? returnUrl)
    {
        // if (!string.IsNullOrEmpty(returnUrl))
        //     ReturnUrl = returnUrl;

        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
            return Page();
        }

        // find period by name
        var targetPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(Uri.UnescapeDataString(periodName));
        if (targetPeriod != null)
            TargetPeriodName = targetPeriod.PeriodName;
        else
            ModelState.AddModelError(string.Empty, "Unknown Target Period.");

        var allPeriodsResult = await _kpiPeriodService.FindAll_Async();
        if (!allPeriodsResult.IsSuccess || allPeriodsResult.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to load KPI periods.");
            return Page();
        }

        SourcePeriodListItems = allPeriodsResult.Data
            .Where(p => p.PeriodName != TargetPeriodName) // filter out Target Period
            .Select((p, index) => new SelectListItem
            {
                Value = p.PeriodName,
                Text = p.PeriodName
            }).ToList();

        if (string.IsNullOrEmpty(SourcePeriodName))
        {
            SourcePeriodName = sourcePeriod;
            // var sourceName = TempData["CurrentDepartmentCode"]?.ToString();
            // if (!string.IsNullOrEmpty(sourceName))
            // SourcePeriodName = sourceName;
            // else
            // SourcePeriodName = allPeriodsResult.Data.First().PeriodName;
        }

        // load key metircs data by selected source period
        SourceDKMs = await LoadDepartmentKeysByPeriod(SourcePeriodName);

        var sourceId = allPeriodsResult.Data.Where(s => s.PeriodName == SourcePeriodName).Select(s => s.Id).FirstOrDefault();
        var targetId = allPeriodsResult.Data.Where(t => t.PeriodName == TargetPeriodName).Select(t => t.Id).FirstOrDefault();
        var result = await _dkmService.CopyAsync(sourceId, targetId);

        // if (result.IsSuccess)
        // {
        //     if (!string.IsNullOrEmpty(ReturnUrl))
        //         return LocalRedirect(ReturnUrl);

        //     return RedirectToPage("/Manage/KeyKpi/IssuedKeys/Index");
        // }

        ModelState.AddModelError(string.Empty, "Failed to import department keys from source period.");

        return Page();
    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }
        return RedirectToPage("Index");
    }

    private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeysByPeriod(string sourcePeriodName)
    {
        var dkms = await _dkmService.FindByPeriodNameAsync(sourcePeriodName);
        if (!dkms.IsSuccess || dkms.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to fetch department keys by source period.");
            return [];
        }

        return dkms.Data
            .Where(dkm => dkm.IsDeleted == false)
            .OrderBy(dkm => dkm.KeyIssueDepartment.DepartmentName)
                .ThenBy(dkm => dkm.KeyMetric.MetricTitle)
            .Select(dkm => dkm.MapToViewModel()).ToList();
    }

    // private async Task<KpiPeriodViewModel?> LoadKpiPeriod(string periodName)
    // {
    //     var kpiPeriod = await _kpiPeriodService
    //         .FindByKpiPeriodNameAsync(periodName);

    //     if (kpiPeriod != null)
    //     {
    //         return new KpiPeriodViewModel()
    //         {
    //             Id = kpiPeriod.Id,
    //             PeriodName = kpiPeriod.PeriodName,
    //             SubmissionStartDate = kpiPeriod.SubmissionStartDate,
    //             SubmissionEndDate = kpiPeriod.SubmissionEndDate
    //         };
    //     }

    //     return null;
    // }
}
