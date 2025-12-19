using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Manage.Submissions;

public class IndexModel(
    IKpiSubmissionPeriodService kpiPeriodService,
    IKpiSubmissionService kpiSubmissionService,
    IKeyKpiSubmissionService keyKpiSubmissionService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IKpiSubmissionService _kpiSubmissionService = kpiSubmissionService;
    private readonly IKeyKpiSubmissionService _keyKpiSubmissionService = keyKpiSubmissionService;



    // ========== HANDLERS ==================================================
    public async Task<PageResult> OnGetAsync()
    {
        // load periods
        PeriodList = await LoadPeriodList();
        // Load Select Items

        PeriodSelectItems = LoadPeriodListItems(PeriodList);
        SubmissionTypeSelectItems = LoadSubmissionTypeListItems();

        if (string.IsNullOrEmpty(Period))
            Period = PeriodSelectItems[0].Value;
        if (string.IsNullOrEmpty(Type))
            Type = SubmissionTypeSelectItems[0].Value;


        SelectedPeriod = PeriodList.First(p => p.PeriodName.ToLower() == Period.ToLower());
        // Load submissions
        if (Type == "kpi")
        {
            KpiSubmissions = await LoadKpiSubmissions(SelectedPeriod.Id);
        }
        else if (Type == "key-kpi")
        {
            KeyKpiSubmissions = await LoadKeyKpiSubmissions(SelectedPeriod.Id);
        }

        return Page();
    }


    public async Task<IActionResult> OnPostDeleteAsync()
    {

        if (!string.IsNullOrEmpty(UserId))
        {
            if (!string.IsNullOrEmpty(Type))
            {
                // kpi
                if (Type.Equals("kpi", StringComparison.OrdinalIgnoreCase))
                {
                    var deleteResult = await _kpiSubmissionService.DeleteByPeriodByCandidateAsync(PeriodId, UserId);
                    if (!deleteResult.IsSuccess)
                        ModelState.AddModelError(string.Empty, "Delete KPI Submissions failed.");

                    StatusMessage = "KPI Submission deleted.";
                    StatusSuccess = true;

                    return RedirectToPage(new { Period, Type });
                }
                // key kpi
                else if (Type.Equals("key-kpi", StringComparison.OrdinalIgnoreCase))
                {
                    var deleteResult = await _keyKpiSubmissionService.DeleteByPeriodByCandidateAsync(PeriodId, UserId);
                    if (!deleteResult.IsSuccess)
                        ModelState.AddModelError(string.Empty, "Delete Key KPI Submissions failed.");

                    StatusMessage = "Key KPI Submission deleted.";
                    StatusSuccess = true;

                    return RedirectToPage(new { Period, Type, });
                }
                // case feedback
                else if (Type.Equals("case-feedback", StringComparison.OrdinalIgnoreCase))
                { }
                else
                    ModelState.AddModelError(string.Empty, "Invalid submissions type selected.");
            }
        }
        else
            ModelState.AddModelError(string.Empty, "Delete submissions failed.");

        // ----------refetch data----------------------------------------
        // load periods
        PeriodList = await LoadPeriodList();
        // Load Select Items

        PeriodSelectItems = LoadPeriodListItems(PeriodList);
        SubmissionTypeSelectItems = LoadSubmissionTypeListItems();

        if (string.IsNullOrEmpty(Period))
            Period = PeriodSelectItems[0].Value;
        if (string.IsNullOrEmpty(Type))
            Type = SubmissionTypeSelectItems[0].Value;


        SelectedPeriod = PeriodList.First(p => p.PeriodName.ToLower() == Period.ToLower());
        // Load submissions
        if (Type == "kpi")
        {
            KpiSubmissions = await LoadKpiSubmissions(SelectedPeriod.Id);
        }
        else if (Type == "key-kpi")
        {
            KeyKpiSubmissions = await LoadKeyKpiSubmissions(SelectedPeriod.Id);
        }

        return Page();
    }






    // ========== Methods ==================================================
    private async Task<List<KpiPeriodViewModel>> LoadPeriodList()
    {
        var periods = await _kpiPeriodService.FindAll_Async();
        if (periods.IsSuccess && periods.Data != null)
        {
            return periods.Data.Select(p => new KpiPeriodViewModel
            {
                Id = p.Id,
                PeriodName = p.PeriodName,
                SubmissionStartDate = p.SubmissionStartDate,
                SubmissionEndDate = p.SubmissionEndDate
            }).ToList();
        }
        return [];
    }

    private List<SelectListItem> LoadPeriodListItems(List<KpiPeriodViewModel> periodList)
    {
        if (PeriodList.Count > 0)
        {
            var items = new List<SelectListItem>();
            foreach (var p in PeriodList)
            {
                items.Add(new SelectListItem
                {
                    Value = p.PeriodName,
                    // Text = $"{p.PeriodName} ({p.SubmissionStartDate.ToLocalTime().Date.ToString("dd-MMM-yyyy")} - {p.SubmissionEndDate.ToLocalTime().Date.ToString("dd-MMM-yyyy")})",
                    Text = $"{p.PeriodName}",
                });
            }
            return items;
        }
        ModelState.AddModelError(string.Empty, "Period does not exist.");
        return [];
    }

    private static List<SelectListItem> LoadSubmissionTypeListItems()
    {
        var submissionTypes = new Dictionary<string, string>
        {
            {"kpi", "Kpi Submissions"},
            {"key-kpi", "Key KPI Submissions"}, 
            // {"case-feedback", "Case Feedback Submissions"}
        };

        var items = new List<SelectListItem>();
        foreach (var s in submissionTypes)
        {
            items.Add(new SelectListItem
            {
                Value = s.Key,
                Text = s.Value,
            });
        }
        return items;
    }

    private async Task<List<KpiSubmissionViewModel>> LoadKpiSubmissions(long periodId)
    {
        var submissions = await _kpiSubmissionService.FindByPeriod_Async(periodId, true);
        if (submissions.IsSuccess && submissions.Data != null)
        {
            // KpiSubmissionDto to KpiSubmissionLiteViewModel
            return submissions.Data
                .DistinctBy(s => s.SubmitterId)
                .Select(s => new KpiSubmissionViewModel
                {
                    KpiPeriod = s.KpiPeriod.MapToViewModel(),
                    SubmittedBy = s.SubmittedBy.MapToViewModel()
                }).ToList();
        }
        return [];
    }


    private async Task<List<KeyKpiSubmissionViewModel>> LoadKeyKpiSubmissions(long periodId)
    {
        var submissions = await _keyKpiSubmissionService.FindByPeriodAsync(periodId, true);
        if (submissions.IsSuccess && submissions.Data != null)
        {
            // KpiSubmissionDto to KpiSubmissionLiteViewModel
            return submissions.Data
                .DistinctBy(s => s.SubmitterId)
                .Select(s => new KeyKpiSubmissionViewModel
                {
                    KpiPeriod = s.DepartmentKeyMetric.SubmissionPeriod.MapToViewModel(),
                    SubmittedBy = s.SubmittedBy.MapToViewModel()
                }).ToList();
        }
        return [];
    }

    // ========== MODELS ============================================================
    public class KpiSubmissionViewModel
    {
        // public long PeriodId { get; set; }
        public KpiPeriodViewModel KpiPeriod { get; set; } = null!;
        public UserViewModel SubmittedBy { get; set; } = null!;
    }

    public class KeyKpiSubmissionViewModel
    {
        // public long PeriodId { get; set; }
        public KpiPeriodViewModel KpiPeriod { get; set; } = null!;
        public UserViewModel SubmittedBy { get; set; } = null!;
    }

    public KpiPeriodViewModel SelectedPeriod { get; set; } = new();
    public List<KpiPeriodViewModel> PeriodList { get; set; } = [];
    public List<SelectListItem> PeriodSelectItems { get; set; } = [];
    public List<SelectListItem> SubmissionTypeSelectItems { get; set; } = [];


    //-----SELECT ITEMS-----------------------------------------------------------
    [BindProperty(Name = "period", SupportsGet = true)]
    public string Period { get; set; } = null!;

    [BindProperty(Name = "type", SupportsGet = true)]
    public string Type { get; set; } = null!; // kpi, key-kpi, case-feedback
    //----------------------------------------------------------------------------


    //-----binding for Delete handler (hidden fields)-----------------------------
    [BindProperty]
    public long PeriodId { get; set; }

    [BindProperty]
    public string UserId { get; set; } = null!;
    //----------------------------------------------------------------------------


    public List<KpiSubmissionViewModel> KpiSubmissions { get; set; } = [];
    public List<KeyKpiSubmissionViewModel> KeyKpiSubmissions { get; set; } = [];


    //-----------STATUS MESSAGE---------------------------------------------------
    [TempData]
    public string? StatusMessage { get; set; }
    [TempData]
    public bool StatusSuccess { get; set; }

}
