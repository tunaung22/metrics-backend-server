using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Web.Pages.Reports.Submissions.Departments.Kpi;

[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class ViewModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserService _userService;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionService _kpiSubmissionService;
    private readonly IUserTitleService _userTitleService;

    public ViewModel(
        UserManager<ApplicationUser> userManager,
        IUserService userService,
        IKpiSubmissionPeriodService kpiPeriodService,
        IDepartmentService departmentService,
        IKpiSubmissionService kpiSubmissionService,
        IUserTitleService userTitleService)
    {
        _userManager = userManager;
        _userService = userService;
        _kpiPeriodService = kpiPeriodService;
        _departmentService = departmentService;
        _kpiSubmissionService = kpiSubmissionService;
        _userTitleService = userTitleService;
    }

    // =============== MODELS ==================================================
    // public class KpiSubmissionReportMasterViewModel
    // {
    //     public string? DepartmentName { get; set; }
    //     public List<KpiSubmissionReportViewModel> ReportDetails { get; set; } = [];
    // }
    public class ExpReportViewModel
    {
        public string? PeriodName { get; set; }
        public string? DepartmentName { get; set; }
        // public List<Dictionary<string, int>> TotalSubmissionsByGroup { get; set; } = [];
        // public List<Dictionary<string, decimal>> TotalScoreByGroup { get; set; } = [];
        public List<UserGroupSubmission> UserGroupSubmissions { get; set; } = [];
        public long TotalSubmissions { get; set; }
        public decimal TotalScore { get; set; }
        public decimal KpiScore { get; set; }
    }

    public class UserGroupSubmission // a single user group info
    {
        public string? GroupName { get; set; }
        public int TotalSubmissions { get; set; }
        public decimal TotalScore { get; set; }
    }

    public List<ExpReportViewModel> ExpReportViewModels { get; set; } = [];

    public class KpiSubmissionReportViewModel
    {
        public string? PeriodName { get; set; }
        public string? DepartmentName { get; set; }
        public string? UserGroupName { get; set; }
        // public long TotalUser { get; set; }
        public long TotalSubmissions { get; set; }
        public decimal TotalScore { get; set; }
        public decimal KpiScore { get; set; }
    }

    public List<KpiSubmissionReportViewModel> KpiSubmissionReportList { get; set; } = [];
    // public List<KpiSubmissionReportMasterViewModel> MasterReportList { get; set; } = [];
    public List<DepartmentViewModel> DepartmentList { get; set; } = [];
    public List<KpiPeriodViewModel> KpiPeriodlist { get; set; } = [];
    public KpiPeriodViewModel SelectedPeriod { get; set; } = new KpiPeriodViewModel()
    {
        PeriodName = DateTime.UtcNow.Date.Year.ToString() + DateTime.UtcNow.Date.Month.ToString("MM")
    };

    [BindProperty]
    public List<SelectListItem> UserTitleListItems { get; set; } = []; // for select element
    public List<UserTitle> UserGroups { get; set; } = [];
    public List<ApplicationUser> Users { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? SelectedUserGroupName { get; set; } // for filter select element

    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(string periodName)
    {
        // ** Clear the list on each request.
        // Reason: Select option changes will cause this OnGet to execute multiple time.
        ExpReportViewModels.Clear();
        KpiSubmissionReportList.Clear();

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
        {
            ModelState.AddModelError("", $"Period {periodName} not found.");
            return Page();
        }

        // ---------- Load User Groups ------------------------------
        var userGroups = await _userTitleService.FindAllAsync();
        if (userGroups.Any())
            UserGroups = userGroups.ToList();
        // ---------- Load Users ------------------------------------
        var users = await _userService.FindAllActiveAsync();
        // ---------- Load User Group Items for Select elemeent -----
        UserTitleListItems = await LoadUserTitleListItems();
        // ---------- Load Departments ------------------------------
        var departments = await _departmentService.FindAllAsync();

        if (departments.Any())
        {
            DepartmentList = departments.Select(d => new DepartmentViewModel()
            {
                Id = d.Id,
                DepartmentName = d.DepartmentName,
                DepartmentCode = d.DepartmentCode
            }).ToList();


            foreach (var department in DepartmentList)
            {
                var departmentSubmissionsByPeriod = await _kpiSubmissionService
                    .FindByKpiPeriodAndDepartmentAsync(SelectedPeriod.Id, department.Id);

                if (!string.IsNullOrEmpty(SelectedUserGroupName)) // Filter the result
                {   // ---------- Filter by User Group -----------------------------
                    if (departmentSubmissionsByPeriod.Any() && UserGroups.Any())
                    {
                        var submissionsByUserGroup = departmentSubmissionsByPeriod
                            .Where(s => s.SubmittedBy.UserTitle.TitleName == SelectedUserGroupName)
                            .ToList();

                        if (submissionsByUserGroup.Any())
                        {
                            var totalSubmission = submissionsByUserGroup.Count;
                            var totalScore = submissionsByUserGroup.Sum(e => e.ScoreValue);

                            // KpiReportList.Add(singleReportForPeriodAndDepartment);
                            KpiSubmissionReportList.Add(new KpiSubmissionReportViewModel
                            {
                                PeriodName = SelectedPeriod.PeriodName,
                                DepartmentName = department.DepartmentName,
                                UserGroupName = SelectedUserGroupName ?? "All",
                                TotalSubmissions = totalSubmission,
                                TotalScore = totalScore,
                                KpiScore = totalSubmission > 0 ? totalScore / totalSubmission : 0M
                            });
                        }
                    }
                }
                else
                {
                    // ---------- Fetch ALL ----------------------------------------
                    // Total Submissions of Department for Period
                    if (departmentSubmissionsByPeriod.Any() && UserGroups.Any())
                    {



                        List<UserGroupSubmission> userGroupSubmissions = [];
                        // ========== FILTER: Submission by User Group ====================
                        foreach (var group in UserGroups)
                        {
                            // ========== Submissions by (Period + Department + Group) =======================================
                            var submissions = departmentSubmissionsByPeriod
                                .Where(s => s.SubmittedBy.UserTitleId == group.Id)
                                .ToList();

                            // ========== find total user in department by user group ========================================
                            // ========== totalDepartmentUserByGroup = users.Where(u => u.UserTitleId == group.Id); ==========

                            // ========== Total Count by (Period + Department + Group)
                            // ========== find total submissions to department by user group ==================================
                            // totalSubmissionsByGroup = submissions.Count;

                            // ========== find total score of submissions to department by user group =========================
                            // ========== Total Score by (Period + Department + Group) ========================================
                            // totalScoreByGroup = submissions.Sum(s => s.ScoreValue);

                            userGroupSubmissions.Add(new UserGroupSubmission
                            {
                                GroupName = group.TitleName,
                                TotalSubmissions = submissions.Count,
                                TotalScore = submissions.Sum(s => s.ScoreValue)
                            });
                        }


                        var totalSubmissions = departmentSubmissionsByPeriod.Count;
                        var totalScore = departmentSubmissionsByPeriod.Sum(e => e.ScoreValue);
                        var kpiScore = (totalSubmissions > 0)
                            ? (totalScore / totalSubmissions)
                            : 0M;

                        ExpReportViewModels.Add(new ExpReportViewModel
                        {
                            // Main data
                            PeriodName = SelectedPeriod.PeriodName,
                            DepartmentName = department.DepartmentName,
                            TotalSubmissions = totalSubmissions,
                            TotalScore = totalScore,
                            KpiScore = kpiScore,
                            // List of Detail for Groups
                            UserGroupSubmissions = userGroupSubmissions,
                        });
                    }

                    /*
                    // ---------- Fetch ALL ----------------------------------------
                    // Total Submissions of Department for Period
                    if (submissionsByPeriodAndDepartment.Any() && UserGroups.Any())
                    {
                        // var totalSubmission = submissionsByPeriodAndDepartment.Count;
                        // var totalScore = submissionsByPeriodAndDepartment.Sum(e => e.ScoreValue);
                        var rowData = new List<KpiSubmissionReportViewModel>();
                        foreach (var group in UserGroups) // user groups of a department
                        {
                            // find total user in department by user group
                            var totalDepartmentUserByGroup = users
                                .Where(u => u.UserTitleId == group.Id);

                            // find total submissions to department by user group
                            var totalDepartmentSubmissionByUserGroup = submissionsByPeriodAndDepartment
                                .Where(s => s.SubmittedBy.UserTitleId == group.Id)
                                .Count();

                            // find total score of submissions to department by user group
                            var totalDepartmentScoreByUserGroup = submissionsByPeriodAndDepartment
                                .Where(s => s.SubmittedBy.UserTitleId == group.Id)
                                .Sum(s => s.ScoreValue);

                            var totalKpiScoreByUserGroup = (totalDepartmentScoreByUserGroup > 0) ?
                                totalDepartmentScoreByUserGroup / totalDepartmentSubmissionByUserGroup :
                                0M;

                            rowData.Add(new KpiSubmissionReportViewModel
                            {
                                PeriodName = SelectedPeriod.PeriodName,
                                DepartmentName = department.DepartmentName,
                                UserGroupName = group.TitleName ?? "UNKNOWN",
                                TotalSubmissions = totalDepartmentSubmissionByUserGroup, // submissions by group
                                TotalScore = totalDepartmentScoreByUserGroup, // score by group
                                KpiScore = totalDepartmentSubmissionByUserGroup > 0 ?
                                    totalDepartmentScoreByUserGroup / totalDepartmentSubmissionByUserGroup :
                                    0M,
                            });
                        }

                        MasterReportList.Add(new KpiSubmissionReportMasterViewModel
                        {
                            DepartmentName = department.DepartmentName,
                            ReportDetails = rowData
                        });
                    }
                    */
                }
            }


            // var singleReportForPeriodAndDepartment = new KpiReportModel()
            // {
            //     KpiPeriodName = SelectedPeriod.PeriodName,
            //     DepartmentName = department.DepartmentName,
            //     TotalReceivedSubmissions = totalSubmission,
            //     TotalReceivedScore = totalScore,
            //     CalculatedKpiScore = totalScore / totalSubmission
            // };

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

        }
        else
        {
            ModelState.AddModelError("", "No department found. Try to add department and continue.");
        }

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
