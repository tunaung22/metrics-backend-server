using Metrics.Application.Common.Mappers;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Metrics.Web.Models.DepartmentKeyMetric;
using Metrics.Web.Models.KeyKpiSubmissionConstraint;
using Metrics.Web.Models.KeyMetric;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Metrics.Web.Pages.Reports.Submissions.KeyKpi;

public class SummaryModel(
    IKpiSubmissionPeriodService kpiPeriodService,
    IUserService userService,
    IUserTitleService userGroupService,
    IKeyKpiSubmissionService keyKpiSubmissionService,
    IKeyKpiSubmissionConstraintService submissionConstraintService,
    IDepartmentKeyMetricService departmentKeyMetricService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IUserService _userService = userService;
    private readonly IUserTitleService _userGroupService = userGroupService;
    private readonly IKeyKpiSubmissionService _keyKpiSubmissionService = keyKpiSubmissionService;
    private readonly IKeyKpiSubmissionConstraintService _submissionConstraintService = submissionConstraintService;
    public readonly IDepartmentKeyMetricService _departmentKeyMetricService = departmentKeyMetricService;


    // ========== HANDLERS =====================================================
    public async Task<IActionResult> OnGetAsync(string periodName)
    {
        // ----------PERIOD-----------------------------------------------------
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }
        var period = await LoadKpiPeriod(periodName);
        if (period == null)
        {
            ModelState.AddModelError("", $"Period {periodName} not found.");
            return Page();
        }
        else
        {
            SelectedPeriod = period;
            SelectedPeriodName = period.PeriodName;
        }


        // ---------CANDIDATE DEPARTMENT------------------------------------------------
        CandidateDepartmentList = await LoadCandidateDepartmentFromConstraints(SelectedPeriod.Id);
        if (CandidateDepartmentList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "No Candidate Department.");
            return Page();
        }

        // ----------KEY ISSUE DEPARTMENT---------------------------------------
        KeyIssueDepartmentList = await LoadKeyIssueDepartmentFromConstraints(SelectedPeriod.Id);
        if (KeyIssueDepartmentList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "No Key Issue Department..");
            return Page();
        }
        KeyIssueDepartmentSelectItems = LoadKeyIssueDepartmentListItems(KeyIssueDepartmentList);
        if (string.IsNullOrEmpty(KeyDepartment))
        {
            KeyDepartment = KeyIssueDepartmentSelectItems[0].Value;
        }
        var KEY_DEPARTMENT__ALL = KeyDepartment.Equals("all", StringComparison.OrdinalIgnoreCase);


        // ----------SUBMISSION CONSTRAINTS-------------------------------------
        var allSubmissionConstraints = await LoadSubmissionConstraintByPeriod(SelectedPeriod.Id);
        // ----------DEPARTMENT KEY METRICS-------------------------------------
        DepartmentKeyMetrics = await LoadDepartmentKeyMetricByPeriod(SelectedPeriod.Id);

        // ----------SUBMISSIONS-------------------------------------------------
        var allSubmissions = await _keyKpiSubmissionService.FindByPeriodAsync(SelectedPeriod.Id);
        if (!allSubmissions.IsSuccess || allSubmissions.Data == null)
        {
            ModelState.AddModelError(string.Empty, "No submissions found.");
            return Page();
        }
        KeyKpiSubmissions = allSubmissions.Data.Select(s => s.MapToViewModel()).ToList();

        // ----------------------------------------------------------------------
        // ----------ALL KEY ISSUE DEPARTMENT------------------------------------
        // ----------------------------------------------------------------------
        if (KEY_DEPARTMENT__ALL)
        {
            foreach (var keyDpt in KeyIssueDepartmentList)
            {
                var submissionsPerKeyDepartment = allSubmissions.Data
                   .Where(s => s.DepartmentKeyMetric.KeyIssueDepartmentId == keyDpt.Id)
                   .ToList();
                var constraintsPerKeyDepartment = allSubmissionConstraints
                    .Where(c => c.DepartmentKeyMetric.KeyIssueDepartmentId == keyDpt.Id)
                    .ToList();

                // BASED ON CONSTRAINTS DEFINED
                // var keyMetrics = constraintsPerKeyDepartment
                //     .DistinctBy(c => c.DepartmentKeyMetric.KeyMetricId)
                //     .OrderBy(s => s.DepartmentKeyMetric.KeyMetric.MetricTitle)
                //     .Select(c => c.DepartmentKeyMetric.KeyMetric)
                //     .ToList();
                // BASED ON SUBMISSIONS RECEIVED
                var keyMetrics = submissionsPerKeyDepartment
                    .DistinctBy(s => s.DepartmentKeyMetric.KeyMetricId)
                    .OrderBy(s => s.DepartmentKeyMetric.KeyMetric.MetricTitle)
                    .Select(s => s.DepartmentKeyMetric.KeyMetric)
                    .ToList();


                // List<SummaryReport_RowDetail_ViewModel> summaryReportRowDetail = [];
                List<SummaryReportItem__ViewModel> SummaryReportItems = [];

                foreach (var keyMetric in keyMetrics)
                {
                    var submissionsOnKey = submissionsPerKeyDepartment
                        .Where(s => s.DepartmentKeyMetric.KeyMetricId == keyMetric.Id)
                        .ToList();
                    var submissionCountPerKey = submissionsOnKey.Count;
                    var scoreTotalPerKey = submissionsOnKey.Sum(s => s.ScoreValue);

                    var candidateDepartmentCount = submissionsPerKeyDepartment
                        .DistinctBy(s => s.SubmittedBy.DepartmentId).Count();
                    var averageScore = scoreTotalPerKey / candidateDepartmentCount; // scoreTotalPerKey / no. of candidate department with actual score submitted 

                    // List<CandidateDepartmentScoreDetail> candidateDepartmentScoreDetails = [];
                    List<SummaryReport_CandidateDepartmentScore__ViewModel> SummaryReport_CandidateDepartmentScoreList = [];

                    var candidateDepartmentList = submissionsOnKey.DistinctBy(s => s.SubmittedBy.DepartmentId).Select(s => s.SubmittedBy.Department).ToList();
                    foreach (var department in candidateDepartmentList)
                    {
                        // for each candidate department, get submission score total
                        var scoreTotal = submissionsOnKey.Where(s => s.SubmittedBy.DepartmentId == department.Id).Sum(s => s.ScoreValue);
                        // candidateDepartmentScoreDetails.Add(new CandidateDepartmentScoreDetail
                        SummaryReport_CandidateDepartmentScoreList.Add(new SummaryReport_CandidateDepartmentScore__ViewModel
                        {
                            CandidateDepartment = department.MapToViewModel(),
                            CandidateScore = scoreTotal,
                        });
                    }

                    // for each department for each key => 
                    // var row = new SummaryReport_RowDetail_ViewModel
                    var row = new SummaryReportItem__ViewModel
                    {
                        PeriodName = SelectedPeriodName,
                        KeyIssueDepartment = keyDpt,
                        KeyMetric = keyMetric.MapToViewModel(),
                        // CandidateDepartmentScoreDetails = candidateDepartmentScoreDetails,
                        SummaryReport_CandidateDepartmentScoreList = SummaryReport_CandidateDepartmentScoreList,
                        ReceivedSubmissions = submissionCountPerKey, //count
                        ReceivedScore = scoreTotalPerKey, //sum
                        AverageScore = averageScore,
                    };
                    SummaryReportItems.Add(row);
                }

                // department's total key's total score
                // -> find final score
                // total / keys count (where submissions present)
                // SummaryReport_MultiKeyDepartments.Add(new SummaryReport_ViewModel
                // {
                //     SummaryReportRow_Details = summaryReportRowDetail,
                //     FinalScore = 0,
                // });
                SummaryReport_Multiple.Add(new SummaryReport__ViewModel
                {
                    SummaryReportItems = SummaryReportItems,
                    FinalScore = 0,
                });
            }
        }

        // ----------------------------------------------------------------------
        // ----------SINGLE KEY ISSUE DEPARTMENT---------------------------------------
        // ----------------------------------------------------------------------
        else
        {
            if (Guid.TryParse(KeyDepartment, out Guid departmentCode) == false)
            {
                ModelState.AddModelError(string.Empty, "Invalid Department Code.");
                return Page();
            }
            SelectedKeyDepartment = KeyIssueDepartmentList.FirstOrDefault(d => d.DepartmentCode == departmentCode);

            if (SelectedKeyDepartment != null)
            {
                var submissionsPerKeyDepartment = allSubmissions.Data
                    .Where(s => s.DepartmentKeyMetric.KeyIssueDepartment.DepartmentCode == SelectedKeyDepartment.DepartmentCode)
                    .ToList();
                var constraintsPerKeyDepartment = allSubmissionConstraints
                    .Where(c => c.DepartmentKeyMetric.KeyIssueDepartment.DepartmentCode == SelectedKeyDepartment.DepartmentCode)
                    .ToList();

                // BASED ON SUBMISSIONS RECEIVED
                var keyMetrics = submissionsPerKeyDepartment
                    .DistinctBy(s => s.DepartmentKeyMetric.KeyMetricId)
                    .OrderBy(s => s.DepartmentKeyMetric.KeyMetric.MetricTitle)
                    .Select(s => s.DepartmentKeyMetric.KeyMetric)
                    .ToList();

                //List<SummaryReport_RowDetail_ViewModel> summaryReportRowDetail = [];
                List<SummaryReportItem__ViewModel> summaryReportItems = [];
                foreach (var keyMetric in keyMetrics) // keys
                {
                    // score total of each key
                    var submissionsOnKey = submissionsPerKeyDepartment
                        .Where(s => s.DepartmentKeyMetric.KeyMetricId == keyMetric.Id)
                        .ToList();
                    var submissionCountPerKey = submissionsOnKey.Count;
                    var scoreTotalPerKey = submissionsOnKey.Sum(s => s.ScoreValue);

                    var candidateDepartmentCount = submissionsPerKeyDepartment
                        .DistinctBy(s => s.SubmittedBy.DepartmentId).Count();
                    var averageScore = scoreTotalPerKey / candidateDepartmentCount; // scoreTotalPerKey / no. of candidate department with actual score submitted 

                    //  total score details submitted by each candidate department 
                    // sum of candidate department score on key of selected key department

                    // List<CandidateDepartmentScoreDetail> candidateDepartmentScoreDetails = [];
                    List<SummaryReport_CandidateDepartmentScore__ViewModel> SummaryReport_CandidateDepartmentScoreList = [];
                    var candidateDptList = submissionsOnKey.DistinctBy(s => s.SubmittedBy.DepartmentId).Select(s => s.SubmittedBy.Department).ToList();
                    foreach (var dpt in candidateDptList)
                    {
                        // for each candidate department, get submission score total
                        var scoreTotal = submissionsOnKey.Where(s => s.SubmittedBy.DepartmentId == dpt.Id).Sum(s => s.ScoreValue);

                        // candidateDepartmentScoreDetails.Add(new CandidateDepartmentScoreDetail
                        SummaryReport_CandidateDepartmentScoreList.Add(new SummaryReport_CandidateDepartmentScore__ViewModel
                        {
                            CandidateDepartment = dpt.MapToViewModel(),
                            CandidateScore = scoreTotal,
                        });
                    }

                    // for each department for each key => 
                    var row = new SummaryReportItem__ViewModel //SummaryReport_RowDetail_ViewModel
                    {
                        PeriodName = SelectedPeriodName,
                        KeyIssueDepartment = SelectedKeyDepartment,
                        KeyMetric = keyMetric.MapToViewModel(),
                        //CandidateDepartmentScoreDetails = candidateDepartmentScoreDetails,
                        SummaryReport_CandidateDepartmentScoreList = SummaryReport_CandidateDepartmentScoreList,
                        ReceivedSubmissions = submissionCountPerKey, //count
                        ReceivedScore = scoreTotalPerKey, //sum
                        AverageScore = averageScore,
                    };
                    //summaryReportRowDetail.Add(row);
                    summaryReportItems.Add(row);
                }

                // department's total key's total score
                // -> find final score
                // total / keys count (where submissions present)
                // SummaryReport_SingleKeyDepartments = new SummaryReport_ViewModel
                // {
                //     SummaryReportRow_Details = summaryReportRowDetail,
                //     FinalScore = 0,
                // };
                SummaryReport_Single = new SummaryReport__ViewModel
                {
                    SummaryReportItems = summaryReportItems,
                    FinalScore = summaryReportItems.Sum(i => i.AverageScore) / summaryReportItems.Count,
                };
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unknown selected key issue department.");
            }
        }

        return Page();
    }


    // ========== Methods ==================================================
    private async Task<KpiPeriodViewModel?> LoadKpiPeriod(string periodName)
    {
        var kpiPeriod = await _kpiPeriodService
            .FindByKpiPeriodNameAsync(periodName);

        // TODO: kpi period service return result<dto>
        if (kpiPeriod != null)
            return kpiPeriod.MapToDto().MapToViewModel();
        return null;
    }

    private async Task<List<DepartmentViewModel>> LoadKeyIssueDepartmentFromConstraints(long periodId)
    {
        var constraints = await _submissionConstraintService.FindByPeriodAsync(periodId);
        if (constraints.IsSuccess && constraints.Data != null)
        {
            return constraints.Data
                .DistinctBy(c => c.DepartmentKeyMetric.KeyIssueDepartmentId)
                .OrderBy(c => c.DepartmentKeyMetric.KeyIssueDepartment.DepartmentName)
                .Select(c => c.DepartmentKeyMetric.KeyIssueDepartment.MapToViewModel())
                .ToList();
        }
        return [];
    }

    private async Task<List<DepartmentViewModel>> LoadCandidateDepartmentFromConstraints(long periodId)
    {
        var constraints = await _submissionConstraintService.FindByPeriodAsync(periodId);
        if (constraints.IsSuccess && constraints.Data != null)
        {
            return constraints.Data
                .DistinctBy(c => c.CandidateDepartmentId)
                .OrderBy(c => c.CandidateDepartment.DepartmentName)
                .Select(c => c.CandidateDepartment.MapToViewModel())
                .ToList();
        }
        return [];
    }

    private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeyMetricByPeriod(long periodId)
    {
        var dkmResult = await _departmentKeyMetricService.FindByPeriodIdAsync(periodId);

        if (dkmResult.IsSuccess && dkmResult.Data != null)
        {
            return dkmResult.Data.Select(e => e.MapToViewModel()).ToList();
        }
        ModelState.AddModelError(string.Empty, "Department keys does not exist.");
        return [];
    }


    private List<SelectListItem> LoadKeyIssueDepartmentListItems(List<DepartmentViewModel> keyIssueDepartmentList)
    {
        if (keyIssueDepartmentList.Count > 0)
        {
            // add All item before user group items
            var items = new List<SelectListItem>()
            {
                new() { Value = "all", Text = "All" }
            };

            foreach (var department in keyIssueDepartmentList)
            {
                items.Add(new SelectListItem
                {
                    Value = department.DepartmentCode.ToString(),
                    Text = department.DepartmentName,
                });
            }
            return items;
        }
        ModelState.AddModelError(string.Empty, "Key Issue Department does not exist.");
        return [];
    }

    private async Task<List<KeyKpiSubmissionConstraintViewModel>> LoadSubmissionConstraintByPeriod(long periodId)
    {
        var constraints = await _submissionConstraintService.FindByPeriodAsync(periodId);
        if (constraints.IsSuccess && constraints.Data != null)
        {
            return constraints.Data
                .Select(c => c.MapToViewModel())
                .ToList();
        }
        ModelState.AddModelError(string.Empty, "Failed to fetch Department Key Assignments. Please try agian later or contact administrator if problem persist.");
        return [];
    }

    // ========== Binding ======================================================

    // ---Period---
    public string SelectedPeriodName { get; set; } = null!;
    public KpiPeriodViewModel SelectedPeriod { get; set; } = new();

    // ----------SELECTION ITEMS-------------------------------------------------
    // ---KeyIssue Department---
    [BindProperty]
    public List<SelectListItem> KeyIssueDepartmentSelectItems { get; set; } = [];
    [BindProperty(SupportsGet = true)]
    public required string KeyDepartment { get; set; }

    // ---Candidate Department---
    // [BindProperty]
    // public List<SelectListItem> CandidateDepartmentSelectItems { get; set; } = [];
    // [BindProperty(SupportsGet = true)]
    // public required string CandidateDepartment { get; set; }
    // --------------------------------------------------------------------------
    public List<DepartmentViewModel> KeyIssueDepartmentList { get; set; } = [];
    public DepartmentViewModel? SelectedKeyDepartment { get; set; } = null;
    public List<DepartmentViewModel> CandidateDepartmentList { get; set; } = [];
    public List<KeyKpiSubmissionViewModel> KeyKpiSubmissions { get; set; } = [];
    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];
    // --------------------------------------------------------------------------


    // ========== MODELS CLASSES ===============================================
    public SummaryReport__ViewModel SummaryReport_Single { get; set; } = new();
    public List<SummaryReport__ViewModel> SummaryReport_Multiple { get; set; } = [];


    public class SummaryReport__ViewModel
    {
        public List<SummaryReportItem__ViewModel> SummaryReportItems { get; set; } = [];
        public decimal FinalScore { get; set; }
    }

    public class SummaryReportItem__ViewModel
    {
        public string PeriodName { get; set; } = null!;
        public DepartmentViewModel KeyIssueDepartment { get; set; } = null!;
        public KeyMetricViewModel KeyMetric { get; set; } = null!;
        public List<SummaryReport_CandidateDepartmentScore__ViewModel> SummaryReport_CandidateDepartmentScoreList { get; set; } = [];
        public long ReceivedSubmissions { get; set; }
        public decimal ReceivedScore { get; set; } // total score received on each department's key
        public decimal AverageScore { get; set; }

    }
    public class SummaryReport_CandidateDepartmentScore__ViewModel
    {
        public DepartmentViewModel CandidateDepartment { get; set; } = null!;
        public decimal CandidateScore { get; set; }
    }

}
