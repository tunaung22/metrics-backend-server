using Metrics.Application.DTOs.KeyKpiSubmissionConstraints;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Metrics.Web.Models.KeyMetric;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Manage.KeyKpi.AssignedKeys;

public class ImportModel(
    IKpiSubmissionPeriodService kpiPeriodService,
    IKeyKpiAssignmentImportService keyKpiAssignmentImportService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IKeyKpiAssignmentImportService _keyKpiAssignmentImportService = keyKpiAssignmentImportService;

    // ============================================================
    public async Task<IActionResult> OnGetAsync(string periodName)
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
        {
            ModelState.AddModelError(string.Empty, "Unknown Target Period.");
            return Page();
        }


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
            SourcePeriodName = periods.Data.First().PeriodName;

        // Compare Source's assignments with Target's DKMs
        // 1. GET Source Assignment's Issuer ID + Key ID              as SourceAssignmentKeys/A
        // 2. GET Target DepartmentKeyMetrics's Issuer ID + Key ID    as TargetDKMs/B
        // 3. GET SourceAssignmentKeys in TargetDKMs                  as AssignmentToImport
        // 4. INSERT KeysToImport into Target Assignments
        var sourcePeriodId = periods.Data
            .Where(s => s.PeriodName == SourcePeriodName)
            .Select(s => s.Id)
            .FirstOrDefault();
        var targetPeriodId = periods.Data
            .Where(t => t.PeriodName == TargetPeriodName)
            .Select(t => t.Id)
            .FirstOrDefault();

        var assignmentsToImport = await _keyKpiAssignmentImportService
            .FindAssignmentsToImportByPeriod(sourcePeriodId, targetPeriodId);
        if (assignmentsToImport.IsSuccess && assignmentsToImport.Data != null)
            SourceAssignments = assignmentsToImport.Data
                .Select(MapToViewModel)
                .ToList();
        else
        {
            ModelState.AddModelError(string.Empty, "No assignment keys to import.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string periodName)
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
            return Page();
        }

        // find period by name
        var targetPeriod = await _kpiPeriodService
            .FindByKpiPeriodNameAsync(Uri.UnescapeDataString(periodName));
        if (targetPeriod != null)
            TargetPeriodName = targetPeriod.PeriodName;
        else
        {
            ModelState.AddModelError(string.Empty, "Unknown Target Period.");
            return Page();
        }

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

        var sourcePeriodId = periods.Data
            .Where(s => s.PeriodName == SourcePeriodName)
            .Select(s => s.Id)
            .FirstOrDefault();
        var targetPeriodId = periods.Data
            .Where(t => t.PeriodName == TargetPeriodName)
            .Select(t => t.Id)
            .FirstOrDefault();

        var assignmentsToImport = await _keyKpiAssignmentImportService
            .FindAssignmentsToImportByPeriod(sourcePeriodId, targetPeriodId);
        if (assignmentsToImport.IsSuccess && assignmentsToImport.Data != null)
        {
            SourceAssignments = assignmentsToImport.Data
                .Select(MapToViewModel).ToList();

            if (SourceAssignments != null && SourceAssignments.Count > 0)
            {
                // viewModel to DTO
                var sourceImportDto = SourceAssignments.Select(source => new KeyKpiSubmissionConstraintImportDto
                {
                    CandidateDepartmentId = source.CandidateDepartmentId,
                    KeyIssueDepartmentId = source.KeyIssueDepartmentId,
                    KeyMetricId = source.KeyMetricId,
                }).ToList();

                var result = await _keyKpiAssignmentImportService.ImportAsync(targetPeriodId, sourceImportDto);

                if (result.IsSuccess)
                {
                    if (!string.IsNullOrEmpty(ReturnUrl))
                        return LocalRedirect(ReturnUrl);
                    if (!string.IsNullOrEmpty(TargetPeriodName))
                        return RedirectToPage("View", new { periodName = Uri.EscapeDataString(TargetPeriodName) });
                    return RedirectToPage("/Manage/Index");
                }
                ModelState.AddModelError(string.Empty, "Failed to import department keys from source period.");
            }
        }

        ModelState.AddModelError(string.Empty, "No assignment keys to import.");

        return Page();
    }

    // public IActionResult OnPostCancel()
    // {
    //     if (!string.IsNullOrEmpty(ReturnUrl))
    //     {
    //         return LocalRedirect(ReturnUrl);
    //     }
    //     return RedirectToPage("Index");
    // }


    // =========================================================================
    private static AssignmentViewModel MapToViewModel(KeyKpiSubmissionConstraintDto dto)
    {
        return new AssignmentViewModel
        {
            CandidateDepartmentId = dto.CandidateDepartmentId,
            CandidateDepartment = dto.CandidateDepartment.MapToViewModel(),

            KeyIssueDepartmentId = dto.DepartmentKeyMetric.KeyIssueDepartmentId,
            KeyIssueDepartment = dto.DepartmentKeyMetric.KeyIssueDepartment.MapToViewModel(),
            KeyMetricId = dto.DepartmentKeyMetric.KeyMetricId,
            KeyMetric = dto.DepartmentKeyMetric.KeyMetric.MapToViewModel(),
        };
    }

    // private static KeyKpiSubmissionConstraintImportDto MapToImportDto(AssignmentViewModel model)
    // {
    //     return new KeyKpiSubmissionConstraintImportDto
    //     {
    //         CandidateDepartmentId = model.CandidateDepartmentId,
    //         KeyIssueDepartmentId = model.KeyIssueDepartmentId,
    //         KeyMetricId = model.KeyMetricId,
    //     };
    // }


    // =========================================================================
    [BindProperty]
    public string? TargetPeriodName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SourcePeriodName { get; set; }

    [BindProperty]
    public List<SelectListItem> SourcePeriodListItems { get; set; } = [];

    [BindProperty]
    public string? ReturnUrl { get; set; } = string.Empty;

    public List<AssignmentViewModel> SourceAssignments { get; set; } = [];
    public class AssignmentViewModel
    {
        public DepartmentViewModel? CandidateDepartment { get; set; }
        public long CandidateDepartmentId { get; set; }
        public long KeyIssueDepartmentId { get; set; } // of DKM
        public DepartmentViewModel? KeyIssueDepartment { get; set; }
        public long KeyMetricId { get; set; } // of DKM
        public KeyMetricViewModel? KeyMetric { get; set; }
        // public long DepartmentKeyMetricId { get; set; }
    }
}



// NOTICE:
// Get Assignments from Source/Selected Period
// source.period == sourcePeriod
// from     source
// select   issuer, key, candidate
// where    source.period AND
//          source's department keys (issuer + keys)
//          is in
//          target's department keys (issuer + keys)