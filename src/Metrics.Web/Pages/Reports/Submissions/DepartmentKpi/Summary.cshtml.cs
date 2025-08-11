using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentKpi;

[Authorize(Policy = "CanAccessAdminFeaturePolicy")]
public class SummaryModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserService _userService;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionService _kpiSubmissionService;
    private readonly IUserTitleService _userTitleService;

    public SummaryModel(
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
    public class UserGroupScoreSubmissionsReportViewModel
    {
        public string? PeriodName { get; set; }
        public string? DepartmentName { get; set; }
        public string? UserGroupName { get; set; }
        // public long TotalUser { get; set; }
        public long TotalSubmissions { get; set; }
        public decimal TotalScore { get; set; }
        public decimal KpiScore { get; set; }
    }

    public class AllUserScoreSubmissionsReportViewModel
    {
        public string? PeriodName { get; set; }
        public string? DepartmentName { get; set; }
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
    public KpiPeriodViewModel SelectedPeriod { get; set; } = null!;
    public List<UserGroupScoreSubmissionsReportViewModel> UserGroupScoreSubmissionsReports { get; set; } = [];
    [BindProperty]
    public List<AllUserScoreSubmissionsReportViewModel> AllUserScoreSubmissionsReports { get; set; } = [];
    public List<UserTitle> UserGroups { get; set; } = [];
    public List<DepartmentViewModel> DepartmentList { get; set; } = [];

    [BindProperty]
    public List<SelectListItem> UserGroupListItems { get; set; } = []; // for select element

    [BindProperty(SupportsGet = true)]
    public string? Group { get; set; } // for filter select element


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(string periodName)
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }

        // ** Clear the list on each request.
        // Reason: Select option changes will cause this OnGet to execute multiple time.
        AllUserScoreSubmissionsReports.Clear();
        UserGroupScoreSubmissionsReports.Clear();
        HttpContext.Session.Remove("ReportDataByAllGroup");
        HttpContext.Session.Remove("ReportDataBySelectedGroup");


        // CHECK periodName exist in submissions
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
        var users = await _userService.FindAllActiveAsync(roleName: "Staff");
        // ---------- Load User Group Items for Select elemeent -----
        UserGroupListItems = await LoadUserGroupListItems();

        // ---------- Load Departments for Table --------------------
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
                var submissionsBySelectedPeriod = await _kpiSubmissionService
                    .FindByKpiPeriodAndDepartmentAsync(
                        SelectedPeriod.Id,
                        department.Id);

                if (string.IsNullOrEmpty(Group) || Group == "All")
                {
                    // ---------- Fetch ALL ----------------------------------------
                    // Total Submissions of Department for Period
                    if (submissionsBySelectedPeriod.Any() && UserGroups.Any())
                    {
                        List<UserGroupSubmission> userGroupSubmissions = [];
                        // ========== FILTER: Submission by User Group ====================
                        foreach (var group in UserGroups)
                        {
                            // ========== Submissions by (Period + Department + Group) =======================================
                            var submissions = submissionsBySelectedPeriod
                                .Where(s => s.SubmittedBy.UserTitleId == group.Id)
                                .ToList();

                            userGroupSubmissions.Add(new UserGroupSubmission
                            {
                                GroupName = group.TitleName,
                                TotalSubmissions = submissions.Count,
                                TotalScore = submissions.Sum(s => s.ScoreValue)
                            });
                        }

                        var totalSubmissions = submissionsBySelectedPeriod.Count;
                        var totalScore = submissionsBySelectedPeriod.Sum(e => e.ScoreValue);
                        var kpiScore = (totalSubmissions > 0)
                            ? (totalScore / totalSubmissions)
                            : 0M;

                        AllUserScoreSubmissionsReports.Add(new AllUserScoreSubmissionsReportViewModel
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

                        // PREPARE DATA FOR EXPORT
                        // HttpContext.Session.SetString("ReportDataByAllGroup",
                        //    JsonSerializer.Serialize(AllUserScoreSubmissionsReports));
                        // TempData["ReportDataByAllGroup"] = JsonSerializer.Serialize(AllUserScoreSubmissionsReports, new JsonSerializerOptions
                        // {
                        //     PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        //     WriteIndented = false // Keep it compact for TempData
                        // });
                    }
                } // END OF FETCH ALL
                else
                {
                    // ---------- **Filter by USER GROUP** -----------------------------
                    if (submissionsBySelectedPeriod.Any() && UserGroups.Any())
                    {
                        var selectedUserGroupSubmissions = submissionsBySelectedPeriod
                            .Where(s => s.SubmittedBy.UserTitle.TitleName == Group)
                            .ToList();

                        if (selectedUserGroupSubmissions.Any())
                        {
                            var totalSubmission = selectedUserGroupSubmissions.Count;
                            var totalScore = selectedUserGroupSubmissions.Sum(e => e.ScoreValue);

                            // KpiReportList.Add(singleReportForPeriodAndDepartment);
                            UserGroupScoreSubmissionsReports.Add(new UserGroupScoreSubmissionsReportViewModel
                            {
                                PeriodName = SelectedPeriod.PeriodName,
                                DepartmentName = department.DepartmentName,
                                UserGroupName = Group ?? "All",
                                TotalSubmissions = totalSubmission,
                                TotalScore = totalScore,
                                KpiScore = totalSubmission > 0 ? totalScore / totalSubmission : 0M
                            });
                        }
                    }
                } // END OF FETCH BY GROUP
            }
        }
        else
        {
            ModelState.AddModelError("", "No department found. Try to add department and continue.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostExportExcelAsync(string periodName)
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }

        // ** Clear the list on each request.
        // Reason: Select option changes will cause this OnGet to execute multiple time.
        AllUserScoreSubmissionsReports.Clear();
        UserGroupScoreSubmissionsReports.Clear();


        // CHECK periodName exist in submissions
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
        var users = await _userService.FindAllActiveAsync(roleName: "Staff");
        // ---------- Load User Group Items for Select elemeent -----
        UserGroupListItems = await LoadUserGroupListItems();

        // ---------- Load Departments for Table --------------------
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
                var submissionsBySelectedPeriod = await _kpiSubmissionService
                    .FindByKpiPeriodAndDepartmentAsync(
                        SelectedPeriod.Id,
                        department.Id);

                if (string.IsNullOrEmpty(Group) || Group == "All")
                {
                    // ---------- Fetch ALL ----------------------------------------
                    // Total Submissions of Department for Period
                    if (submissionsBySelectedPeriod.Any() && UserGroups.Any())
                    {
                        List<UserGroupSubmission> userGroupSubmissions = [];
                        // ========== FILTER: Submission by User Group ====================
                        foreach (var group in UserGroups)
                        {
                            // ========== Submissions by (Period + Department + Group) =======================================
                            var submissions = submissionsBySelectedPeriod
                                .Where(s => s.SubmittedBy.UserTitleId == group.Id)
                                .ToList();

                            userGroupSubmissions.Add(new UserGroupSubmission
                            {
                                GroupName = group.TitleName,
                                TotalSubmissions = submissions.Count,
                                TotalScore = submissions.Sum(s => s.ScoreValue)
                            });
                        }

                        var totalSubmissions = submissionsBySelectedPeriod.Count;
                        var totalScore = submissionsBySelectedPeriod.Sum(e => e.ScoreValue);
                        var kpiScore = (totalSubmissions > 0)
                            ? (totalScore / totalSubmissions)
                            : 0M;

                        AllUserScoreSubmissionsReports.Add(new AllUserScoreSubmissionsReportViewModel
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

                        // PREPARE DATA FOR ALL GROUP
                    }
                } // end of FETCH ALL
                else
                {
                    // ---------- **Filter by USER GROUP** -----------------------------
                    if (submissionsBySelectedPeriod.Any() && UserGroups.Any())
                    {
                        var selectedUserGroupSubmissions = submissionsBySelectedPeriod
                            .Where(s => s.SubmittedBy.UserTitle.TitleName == Group)
                            .ToList();

                        if (selectedUserGroupSubmissions.Any())
                        {
                            var totalSubmission = selectedUserGroupSubmissions.Count;
                            var totalScore = selectedUserGroupSubmissions.Sum(e => e.ScoreValue);

                            // KpiReportList.Add(singleReportForPeriodAndDepartment);
                            UserGroupScoreSubmissionsReports.Add(new UserGroupScoreSubmissionsReportViewModel
                            {
                                PeriodName = SelectedPeriod.PeriodName,
                                DepartmentName = department.DepartmentName,
                                UserGroupName = Group ?? "All",
                                TotalSubmissions = totalSubmission,
                                TotalScore = totalScore,
                                KpiScore = totalSubmission > 0 ? totalScore / totalSubmission : 0M
                            });
                        }

                        // PREPARE DATA FOR SELECTED GROUP

                    }
                }
            }

            if ((Group == null || Group?.ToLower() == "all") && AllUserScoreSubmissionsReports.Any())
            {
                // FOR ALL
                var data = new List<Dictionary<string, object>>();
                foreach (var report in AllUserScoreSubmissionsReports)
                {
                    var dict = new Dictionary<string, object>();


                    dict["Department Name"] = report.DepartmentName ?? "Undefined Department";
                    // Flatten UserGroupSubmissionInfos
                    // int groupIndex = 1;
                    if (report.UserGroupSubmissions != null)
                    {
                        foreach (var groupInfo in report.UserGroupSubmissions)
                        {
                            dict[$"{groupInfo.GroupName} Submissions"] = Convert.ToDecimal(groupInfo.TotalSubmissions.ToString("0"));
                            dict[$"{groupInfo.GroupName} Score"] = Convert.ToDecimal(groupInfo.TotalScore.ToString("0.00"));
                            // dict[$"GroupName {groupIndex}"] = groupInfo.GroupName;
                            // dict[$"Submissions {groupIndex}"] = groupInfo.TotalSubmissions;
                            // dict[$"Score {groupIndex}"] = groupInfo.TotalScore;
                            // groupIndex++;
                        }
                    }

                    dict["Total Submissions"] = Convert.ToDecimal(report.TotalSubmissions.ToString("0.00"));
                    dict["Total Score"] = Convert.ToDecimal(report.TotalScore.ToString("0.00"));
                    dict["KPI Score"] = Convert.ToDecimal(report.KpiScore.ToString("0.00"));

                    data.Add(dict);
                }
                // How to flatten the AllUserScoreSubmissionsReport object???


                var memoryStream = new MemoryStream();
                MiniExcel.SaveAs(
                    stream: memoryStream,
                    value: data
                );
                memoryStream.Position = 0; // Reset stream position

                return File(
                    memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report_DepartmentKPI_All_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
                );
            }
            else if (Group?.ToLower() != "all" && UserGroupScoreSubmissionsReports.Any())
            {
                var memoryStream = new MemoryStream();
                MiniExcel.SaveAs(stream: memoryStream, value: UserGroupScoreSubmissionsReports);
                memoryStream.Position = 0; // Reset stream position

                return File(
                    memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report_DepartmentKPI_{Group}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
                );
            }
        }
        else
        {
            ModelState.AddModelError("", "No department found. Try to add department and continue.");
        }

        return RedirectToPage();
    }




    // public async Task<IActionResult> OnPostExportExcelAsync()
    // {

    //     if (AllUserScoreSubmissionsReports.Any() && Group.ToLower() == "all")
    //     {
    //         // FOR ALL

    //         var data = new List<Dictionary<string, object>>();
    //         foreach (var report in AllUserScoreSubmissionsReports)
    //         {
    //             var dict = new Dictionary<string, object>();

    //             dict["Department Name"] = report.DepartmentName;
    //             dict["Total Submissions"] = report.TotalSubmissions;
    //             dict["Total Score"] = report.TotalScore;
    //             dict["Kpi Score"] = report.KpiScore;

    //             // Flatten UserGroupSubmissionInfos
    //             int groupIndex = 1;
    //             if (report.UserGroupSubmissions != null)
    //             {
    //                 foreach (var groupInfo in report.UserGroupSubmissions)
    //                 {
    //                     dict[$"GroupName{groupIndex}"] = groupInfo.GroupName;
    //                     dict[$"Submissions{groupIndex}"] = groupInfo.TotalSubmissions;
    //                     dict[$"Score{groupIndex}"] = groupInfo.TotalScore;
    //                     groupIndex++;
    //                 }
    //             }

    //             data.Add(dict);
    //         }
    //         // How to flatten the AllUserScoreSubmissionsReport object???


    //         var memoryStream = new MemoryStream();
    //         MiniExcel.SaveAs(
    //             stream: memoryStream,
    //             value: data
    //         );
    //         memoryStream.Position = 0; // Reset stream position

    //         return File(
    //             memoryStream,
    //             "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    //             $"Report-All-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
    //         );
    //     }
    //     else
    //     {
    //         // FOR GROUP
    //         if (UserGroupScoreSubmissionsReports.Any())
    //         {

    //         }
    //     }

    //     return RedirectToPage();
    // }



    // public async Task<IActionResult> OnGetExportExcelAsync()
    // {
    //     if (string.IsNullOrEmpty(Group))
    //         return Page();

    //     if (Group == "all")
    //     {
    //         // AllUserScoreSubmissionsReports
    //         var memoryStream = new MemoryStream();
    //         MiniExcel.SaveAs(stream: memoryStream, value: AllUserScoreSubmissionsReports);
    //         memoryStream.Position = 0; // Reset stream position


    //         return File(
    //             memoryStream,
    //             "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    //             $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
    //         );

    //     }
    //     else
    //     {
    //         // UserGroupScoreSubmissionsReports
    //         // AllUserScoreSubmissionsReports
    //         var memoryStream = new MemoryStream();
    //         MiniExcel.SaveAs(stream: memoryStream, value: UserGroupScoreSubmissionsReports);
    //         memoryStream.Position = 0; // Reset stream position


    //         return File(
    //             memoryStream,
    //             "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    //             $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
    //         );
    //     }


    //     return Page();
    //     // ...
    //     // {
    //     //     periodName = SelectedPeriod.PeriodName,
    //     //     group = Group // This will preserve the Group in the URL
    //     // });
    // }


    // ========== Methods ==================================================
    private async Task<List<SelectListItem>> LoadUserGroupListItems()
    {
        var userGroups = await _userTitleService.FindAllAsync();
        if (userGroups.Any())
        {
            return userGroups.Select(e => new SelectListItem
            {
                Value = e.TitleName, //e.Id.ToString(),
                Text = e.TitleName,
            }).ToList();
        }

        ModelState.AddModelError("", "User group not exist. Try to add group and continue.");
        return [];
    }
}
