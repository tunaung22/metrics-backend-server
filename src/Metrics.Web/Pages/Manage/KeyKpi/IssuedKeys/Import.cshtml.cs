using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models.DepartmentKeyMetric;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Manage.KeyKpi.IssuedKeys;

public class ImportModel(
    IKpiSubmissionPeriodService kpiPeriodService,
    IDepartmentKeyImportService dkmImportService,
    IDepartmentKeyMetricService dkmService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IDepartmentKeyMetricService _dkmService = dkmService;
    private readonly IDepartmentKeyImportService _dkmImportService = dkmImportService;


    // ============================================================
    [BindProperty]
    public string? TargetPeriodName { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? SourcePeriodName { get; set; }

    [BindProperty]
    public List<SelectListItem> SourcePeriodListItems { get; set; } = [];

    public List<DepartmentKeyMetricViewModel> SourceDKMs { get; set; } = [];

    [BindProperty]
    public string? ReturnUrl { get; set; } = string.Empty;


    // ============================================================
    public async Task<IActionResult> OnGetAsync([FromRoute] string periodName)
    {
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

        var periods = await _kpiPeriodService.FindAll_Async();
        if (!periods.IsSuccess || periods.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to load KPI periods.");
            return Page();
        }

        // Load periods for source period dropdown **excluding target period
        SourcePeriodListItems = periods.Data
            .Where(p => p.PeriodName != TargetPeriodName) // filter out Target Period
            .Select((p, index) => new SelectListItem
            {
                Value = p.PeriodName,
                Text = p.PeriodName
            }).ToList();

        if (string.IsNullOrEmpty(SourcePeriodName))
        {
            SourcePeriodName = Uri.EscapeDataString(periods.Data.First().PeriodName);
            // SourcePeriodName = allPeriodsResult.Data.First().PeriodName;
            // TempData["CurrentDepartmentCode"] = SourcePeriodName;
        }

        // load key metircs data by selected source period
        SourceDKMs = await LoadDepartmentKeysByPeriod(SourcePeriodName);
        // ModelState.AddModelError(string.Empty, "No source KPI Period.");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromRoute] string periodName)
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

        var periods = await _kpiPeriodService.FindAll_Async();
        if (!periods.IsSuccess || periods.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to load KPI periods.");
            return Page();
        }

        SourcePeriodListItems = periods.Data
            .Where(p => p.PeriodName != TargetPeriodName) // filter out Target Period
            .Select((p, index) => new SelectListItem
            {
                Value = p.PeriodName,
                Text = p.PeriodName
            }).ToList();

        if (string.IsNullOrEmpty(SourcePeriodName))
        {
            ModelState.AddModelError(string.Empty, "Source period is required.");
            return Page();
        }

        // load key metircs data by selected source period
        SourceDKMs = await LoadDepartmentKeysByPeriod(SourcePeriodName);

        var sourceId = periods.Data.Where(s => s.PeriodName == SourcePeriodName).Select(s => s.Id).FirstOrDefault();
        var targetId = periods.Data.Where(t => t.PeriodName == TargetPeriodName).Select(t => t.Id).FirstOrDefault();
        var result = await _dkmImportService.CopyAsync(sourceId, targetId);

        if (result.IsSuccess)
        {
            if (!string.IsNullOrEmpty(ReturnUrl))
                return LocalRedirect(ReturnUrl);
            if (!string.IsNullOrEmpty(TargetPeriodName))
                return RedirectToPage("View", new { periodName = Uri.EscapeDataString(TargetPeriodName) });
            return RedirectToPage("/Manage/Index");
        }

        ModelState.AddModelError(string.Empty, "Failed to import department keys from source period.");

        return Page();
    }

    // public IActionResult OnPostCancel()
    // {
    //     if (!string.IsNullOrEmpty(ReturnUrl))
    //     {
    //         return LocalRedirect(ReturnUrl);
    //     }
    //     // return RedirectToPage("View", new { periodName = TargetPeriodName });
    //     return RedirectToRoute("/Manage/KeyKpi/IssuedKeys/View", new { periodName = TargetPeriodName });
    // }

    private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeysByPeriod(string sourcePeriodName)
    {
        var dkms = await _dkmService.FindByPeriodNameAsync(Uri.UnescapeDataString(sourcePeriodName));
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
