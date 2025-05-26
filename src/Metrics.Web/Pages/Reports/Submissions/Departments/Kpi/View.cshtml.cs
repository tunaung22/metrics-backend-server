using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Reports.Submissions.Departments.Kpi;

[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class ViewModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionService _kpiSubmissionService;
    private readonly IUserTitleService _userTitleService;

    public ViewModel(
        IKpiSubmissionPeriodService kpiPeriodService,
        IDepartmentService departmentService,
        IKpiSubmissionService kpiSubmissionService,
        IUserTitleService userTitleService)
    {
        _kpiPeriodService = kpiPeriodService;
        _departmentService = departmentService;
        _kpiSubmissionService = kpiSubmissionService;
        _userTitleService = userTitleService;
    }

    // =============== MODELS ==================================================
    public class KpiReportModel
    {
        public required string KpiPeriodName { get; set; }
        public required string DepartmentName { get; set; }
        public required int TotalReceivedSubmissions { get; set; } = 0;
        public required decimal TotalScoreReceived { get; set; } = 0M;
        public required decimal FinalKpiScore { get; set; } = 0M;
    }

    public List<DepartmentViewModel> DepartmentList { get; set; } = [];
    public List<KpiPeriodViewModel> KpiPeriodlist { get; set; } = [];
    public List<KpiReportModel> KpiReportList { get; set; } = [];
    public KpiPeriodViewModel SelectedPeriod { get; set; } = new KpiPeriodViewModel()
    {
        PeriodName = DateTime.UtcNow.Date.Year.ToString() + DateTime.UtcNow.Date.Month.ToString("MM")
    };

    [BindProperty]
    public List<SelectListItem> UserTitleListItems { get; set; } = []; // for select element

    [BindProperty(SupportsGet = true)]
    public string? UserGroup { get; set; }

    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(string periodName)
    {
        // ** Clear the list on each request.
        // Reason: Select option changes will cause this OnGet to execute multiple time.
        KpiReportList.Clear();

        // if (!string.IsNullOrEmpty(groupName))
        //     UserGroup = groupName;

        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }
        // CHECK periodName exist in submissions
        // GET id, SET id
        var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(periodName);
        if (kpiPeriod != null)
            SelectedPeriod = new KpiPeriodViewModel() // ---- do we need entire KPI Period object??
            {
                Id = kpiPeriod.Id,
                PeriodName = kpiPeriod.PeriodName,
                SubmissionStartDate = kpiPeriod.SubmissionStartDate,
                SubmissionEndDate = kpiPeriod.SubmissionEndDate
            };
        else
            ModelState.AddModelError("", $"Period {periodName} not found.");

        UserTitleListItems = await LoadUserTitleListItems();

        var departments = await _departmentService.FindAllAsync();
        if (departments.Any())
        {
            DepartmentList = departments.Select(d => new DepartmentViewModel()
            {
                Id = d.Id,
                DepartmentName = d.DepartmentName,
                DepartmentCode = d.DepartmentCode
            }).ToList();


            if (!string.IsNullOrEmpty(UserGroup)) // Filter the result
            {
                // ---------- Filter by User Group -----------------------------
                foreach (var department in DepartmentList)
                {
                    var submissionListByPeriodAndDepartment = await _kpiSubmissionService
                        .FindByKpiPeriodAndDepartmentAsync(SelectedPeriod.Id, department.Id);
                    var filteredSubmissions = submissionListByPeriodAndDepartment
                        .Where(s => s.SubmittedBy.UserTitle.TitleName == UserGroup)
                        .ToList();

                    if (filteredSubmissions.Any())
                    {
                        var totalSubmission = filteredSubmissions.Count;
                        var totalScore = filteredSubmissions.Sum(e => e.ScoreValue);

                        var singleReportForPeriodAndDepartment = new KpiReportModel()
                        {
                            KpiPeriodName = SelectedPeriod.PeriodName,
                            DepartmentName = department.DepartmentName,
                            TotalReceivedSubmissions = totalSubmission,
                            TotalScoreReceived = totalScore,
                            FinalKpiScore = totalScore / totalSubmission
                        };

                        KpiReportList.Add(singleReportForPeriodAndDepartment);
                    }
                }
            }
            else
            {
                // ---------- Fetch ALL ----------------------------------------
                foreach (var department in DepartmentList)
                {
                    var submissionListByPeriodAndDepartment = await _kpiSubmissionService
                        .FindByKpiPeriodAndDepartmentAsync(SelectedPeriod.Id, department.Id);

                    if (submissionListByPeriodAndDepartment.Any())
                    {
                        var totalSubmission = submissionListByPeriodAndDepartment.Count;
                        var totalScore = submissionListByPeriodAndDepartment.Sum(e => e.ScoreValue);

                        var singleReportForPeriodAndDepartment = new KpiReportModel()
                        {
                            KpiPeriodName = SelectedPeriod.PeriodName,
                            DepartmentName = department.DepartmentName,
                            TotalReceivedSubmissions = totalSubmission,
                            TotalScoreReceived = totalScore,
                            FinalKpiScore = totalScore / totalSubmission
                        };

                        KpiReportList.Add(singleReportForPeriodAndDepartment);
                    }
                }
            }
        }

        // ----- List Submission for **ALL KPI Periods ----------
        // all kpi periods
        // var kpiPeriods = await _kpiPeriodService.FindAllAsync();
        // KpiPeriodlist = kpiPeriods.Select(p => new KpiPeriodViewModel()
        // {
        //     Id = p.Id,
        //     PeriodName = p.PeriodName,
        //     SubmissionStartDate = p.SubmissionStartDate,
        //     SubmissionEndDate = p.SubmissionEndDate
        // }).ToList();

        // foreach (var period in KpiPeriodlist)
        // {
        //     foreach (var department in DepartmentList)
        //     {
        //         var submissionListByPeriodAndDepartment = await _kpiSubmissionService
        //             .FindByKpiPeriodAndDepartmentAsync(period.Id, department.Id);

        //         if (submissionListByPeriodAndDepartment.Any())
        //         {
        //             var totalSubmission = submissionListByPeriodAndDepartment.Count;
        //             var totalScore = submissionListByPeriodAndDepartment.Sum(e => e.KpiScore);

        //             var singleReportForPeriodAndDepartment = new KpiReportModel()
        //             {
        //                 KpiPeriodName = period.PeriodName,
        //                 DepartmentName = department.DepartmentName,
        //                 TotalReceivedSubmissions = totalSubmission,
        //                 TotalScoreReceived = totalScore,
        //                 FinalKpiScore = totalSubmission / totalScore
        //             };

        //             KpiReportList.Add(singleReportForPeriodAndDepartment);
        //         }
        //     }
        // }

        return Page();
    }

    // ========== Methods ==================================================
    private async Task<List<SelectListItem>> LoadUserTitleListItems()
    {
        var userTitles = await _userTitleService.FindAllAsync();
        if (userTitles.Any())
        {
            return userTitles.Select(e => new SelectListItem
            {
                Value = e.TitleName, //e.Id.ToString(),
                Text = e.TitleName
            }).ToList();
        }

        ModelState.AddModelError("", "User group not exist. Try to add group and continue.");
        return [];
    }
}
