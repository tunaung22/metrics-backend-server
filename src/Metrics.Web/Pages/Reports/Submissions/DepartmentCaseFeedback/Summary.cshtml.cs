using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentCaseFeedback;

public class SummaryModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IUserService _userService;
    private readonly IUserTitleService _userTitleService;
    private readonly IDepartmentService _departmentService;
    private readonly ICaseFeedbackSubmissionService _caseFeedbackService;

    public SummaryModel(
        IKpiSubmissionPeriodService kpiPeriodService,
        IUserService userService,
        IUserTitleService userTitleService,
        IDepartmentService departmentService,
        ICaseFeedbackSubmissionService caseFeedbackService)
    {
        _kpiPeriodService = kpiPeriodService;
        _userService = userService;
        _userTitleService = userTitleService;
        _departmentService = departmentService;
        _caseFeedbackService = caseFeedbackService;
    }

    // ========== MODELS =======================================================

    // Report Models
    public class SingleUserGroupSubmissionExcelViewModel
    {
        [ExcelColumnWidth(14)]
        [ExcelColumn(Name = "Period Name")]
        public string? PeriodName { get; set; }

        [ExcelColumnWidth(25)]
        [ExcelColumn(Name = "Case Department")]
        public string? CaseDepartmentName { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "User Group")]
        public string? UserGroupName { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Total Submissions")]
        public long TotalSubmissions { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Total Score")]
        public decimal TotalScore { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Kpi Score")]
        public decimal KpiScore { get; set; }
    }
    public List<SingleUserGroupSubmissionExcelViewModel> SingleUserGroupSubmissionsReports { get; set; } = [];

    public class AllUserGroupSubmissionExcelViewModel
    {
        public string? PeriodName { get; set; }
        public string? CaseDepartmentName { get; set; }
        public List<UserGroupSubmission> UserGroupSubmissions { get; set; } = [];
        public long TotalSubmissions { get; set; }
        public decimal TotalScore { get; set; }
        public decimal KpiScore { get; set; }
    }
    public List<AllUserGroupSubmissionExcelViewModel> AllUserGroupSubmissionsReports { get; set; } = [];


    public class UserGroupSubmission // a single user group info
    {
        public string? GroupName { get; set; }
        public int TotalSubmissions { get; set; }
        public decimal TotalScore { get; set; }
    }

    public class CaseFeedbackSubmissionViewModel
    {
        public long Id { get; set; }
        public Guid LookupId { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
        // public string SubmitterId { get; set; } = null!;
        // name, department
        public UserViewModel SubmittedBy { get; set; } = null!;

        // case department
        // public long CaseDepartmentId { get; set; }
        public DepartmentViewModel CaseDepartment { get; set; } = null!;
        public decimal NegativeScoreValue { get; set; }

        // Case Info
        public DateTimeOffset IncidentAt { get; set; }
        public string WardName { get; set; } = null!;
        public string CPINumber { get; set; } = null!;
        public string PatientName { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        // Case Info > Details
        public string? Description { get; set; } = string.Empty;
        public string? Comments { get; set; } = string.Empty;
    }
    public List<CaseFeedbackSubmissionViewModel> CaseFeedbackSubmissions { get; set; } = [];

    public KpiPeriodViewModel SelectedPeriod { get; set; } = null!;


    public List<DepartmentViewModel> DepartmentList { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Group { get; set; } // for filter select element

    public List<UserGroupViewModel> UserGroups { get; set; } = [];

    [BindProperty]
    public List<SelectListItem> UserGroupListItems { get; set; } = []; // for select element


    // ========== HANDLERS =======================================================
    public async Task<IActionResult> OnGetAsync([FromRoute] string periodName)
    {
        // 1. Clear ViewModels
        // 2. Load Period
        // 3. Load UserGroup **note: remove group "staff"
        // 4. Load Departments

        // ** Clear the list on each request.
        // Reason: Select option changes will cause this OnGet to execute multiple time.
        AllUserGroupSubmissionsReports.Clear();
        SingleUserGroupSubmissionsReports.Clear();

        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
            return Page();
        }
        // ----------PERIOD----------
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

        // ----------USER GROUP-------------------------------------------------
        var userGroups = await _userTitleService.FindAllAsync();
        if (userGroups.Any())
            UserGroups = userGroups
                .Where(g => g.TitleName != "Staff")
                .Select(g => new UserGroupViewModel
                {
                    Id = g.Id,
                    GroupCode = g.TitleCode,
                    GroupName = g.TitleName,
                    Description = g.Description
                }).ToList();
        // ---------- Load User Group Items for Select elemeent -----
        UserGroupListItems = LoadUserGroupSelectListItems(UserGroups);

        // ---------- Load Departments for Table --------------------
        var departments = await _departmentService.FindAllAsync();
        if (!departments.Any())
        {
            ModelState.AddModelError(string.Empty, "No dpeartments found.");
            return Page();
        }

        DepartmentList = departments.Select(d => new DepartmentViewModel()
        {
            Id = d.Id,
            DepartmentName = d.DepartmentName,
            DepartmentCode = d.DepartmentCode
        }).ToList();

        // ---------- Load Submissions by Period -------------------------------
        var caseFeedbackSubmissions = await _caseFeedbackService
            .FindByKpiPeriodAsync(SelectedPeriod.Id);

        // Fore each department, get total score filterd by user group
        foreach (var department in DepartmentList)
        {
            // Case Department == department
            var submissionsByDepartment = caseFeedbackSubmissions
                .Where(s => s.CaseDepartmentId == department.Id)
                .ToList();

            // ----------BY ALL GROUP-------------------------------------------
            if (string.IsNullOrEmpty(Group)
                || Group.Trim().Equals("all", StringComparison.CurrentCultureIgnoreCase))
            {
                var submission = LoadAllUserGroupSubmissions(
                    submissionsByDepartment,
                    department);

                AllUserGroupSubmissionsReports.Add(submission);
            }
            // ----------BY EACH GROUP----------------------------------------
            else
            {
                if (UserGroups.Where(g => g.GroupName
                    .Contains(Group, StringComparison.CurrentCultureIgnoreCase)).Count() == 0)
                {
                    ModelState.AddModelError(string.Empty, $"Group {Group} not found.");
                    return Page();
                }

                var submissions = LoadSingleUserGroupSubmissions(
                    submissionsByDepartment,
                    department,
                    Group);

                SingleUserGroupSubmissionsReports.Add(submissions);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostExportExcelAsync(string periodName)
    {
        // 1. Clear ViewModels
        // 2. Load Period
        // 3. Load UserGroup **note: remove group "staff"
        // 4. Load Departments

        // ** Clear the list on each request.
        // Reason: Select option changes will cause this OnGet to execute multiple time.
        AllUserGroupSubmissionsReports.Clear();
        SingleUserGroupSubmissionsReports.Clear();

        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }
        // ----------PERIOD----------
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
        // ----------USER GROUP-------------------------------------------------
        var userGroups = await _userTitleService.FindAllAsync();
        if (userGroups.Any())
            UserGroups = userGroups
                .Where(g => g.TitleName != "Staff")
                .Select(g => new UserGroupViewModel
                {
                    Id = g.Id,
                    GroupCode = g.TitleCode,
                    GroupName = g.TitleName,
                    Description = g.Description
                }).ToList();
        // ---------- Load Users ------------------------------------
        var users = await _userService.FindAllActiveAsync(roleName: "Staff");
        // ---------- Load User Group Items for Select elemeent -----
        UserGroupListItems = LoadUserGroupSelectListItems(UserGroups);

        // ---------- Load Departments for Table --------------------
        var departments = await _departmentService.FindAllAsync();
        if (!departments.Any())
        {
            ModelState.AddModelError(string.Empty, "No dpeartments found.");
            return Page();
        }

        DepartmentList = departments.Select(d => new DepartmentViewModel()
        {
            Id = d.Id,
            DepartmentName = d.DepartmentName,
            DepartmentCode = d.DepartmentCode
        }).ToList();

        // ---------- Load Submissions by Period -------------------------------
        var caseFeedbackSubmissions = await _caseFeedbackService
            .FindByKpiPeriodAsync(SelectedPeriod.Id);

        // Fore each department, get total score filterd by user group
        foreach (var department in DepartmentList)
        {
            // Case Department == department
            var submissionsByDepartment = caseFeedbackSubmissions
                .Where(s => s.CaseDepartmentId == department.Id)
                .ToList();

            // ----------BY ALL GROUP-------------------------------------------
            if (string.IsNullOrEmpty(Group)
                || Group.Trim().Equals("all", StringComparison.CurrentCultureIgnoreCase))
            {

                var submission = LoadAllUserGroupSubmissions(
                                    submissionsByDepartment,
                                    department);

                AllUserGroupSubmissionsReports.Add(submission);
            }
            // ----------BY EACH GROUP------------------------------------------
            else
            {
                if (UserGroups.Where(g => g.GroupName
                    .Contains(Group, StringComparison.CurrentCultureIgnoreCase)).Count() == 0)
                {
                    ModelState.AddModelError(string.Empty, $"Group {Group} not found.");
                    return Page();
                }

                var submissions = LoadSingleUserGroupSubmissions(
                    submissionsByDepartment,
                    department,
                    Group);

                SingleUserGroupSubmissionsReports.Add(submissions);
            }
        } // end of foreach department

        // Export Excel
        // ---------- Prepare for Excel Export ---------------------------------
        // ----------BY ALL GROUP-----------------------------------------------
        if (AllUserGroupSubmissionsReports.Count > 0)
        {
            var excelData = new List<Dictionary<string, object>>();

            // excel column names
            var colPeriod = "Period";
            var colCaseDepartment = "Case Department";
            var colTotalSubmissions = "Total Submissions";
            var colTotalScore = "Total Score";
            var colKpiScore = "KPI Score";
            var colUserGroup = new List<string>();

            foreach (var r in AllUserGroupSubmissionsReports)
            {
                var dict = new Dictionary<string, object>();

                dict[colPeriod] = r.PeriodName ?? "Undefined Period";
                dict[colCaseDepartment] = r.CaseDepartmentName ?? "Undefined Department";
                // Flatten UserGroupSubmissions list
                if (r.UserGroupSubmissions != null)
                {
                    foreach (var group in r.UserGroupSubmissions)
                    {
                        var colSubmissionsByGroupName = $"Submissions by {group.GroupName}";
                        var colScoresByGroupName = $"Scores by {group.GroupName}";
                        colUserGroup.Add(colSubmissionsByGroupName);
                        colUserGroup.Add(colScoresByGroupName);

                        dict[colSubmissionsByGroupName] = Convert
                            .ToDecimal(group.TotalSubmissions.ToString("0"));
                        dict[colScoresByGroupName] = Convert
                            .ToDecimal(group.TotalScore.ToString("0.00"));
                    }
                }

                dict["Total Submissions"] = Convert.ToDecimal(r.TotalSubmissions.ToString("0.00"));
                dict["Total Score"] = Convert.ToDecimal(r.TotalScore.ToString("0.00"));
                dict["KPI Score"] = Convert.ToDecimal(r.KpiScore.ToString("0.00"));

                excelData.Add(dict);
            }

            // 
            /* ===Using Array===
            var dynamicCols1 = new DynamicExcelColumn[] {
                new(colPeriod) { Width = 20 },
                new(colCaseDepartment) { Width = 30 },
                new(colTotalSubmissions) { Width = 25 },
                new(colTotalScore) { Width = 16 },
                new(colKpiScore) { Width = 16 }
            }.Union(colUserGroup
                .Select(c => new DynamicExcelColumn(c) { Width = 30 }))
                .ToArray();

            var config1 = new OpenXmlConfiguration
            {
                DynamicColumns = dynamicCols1
            }; */

            // ===Using List===
            var dynamicCols = new List<DynamicExcelColumn> {
                new(colPeriod) { Width = 20 },
                new(colCaseDepartment) { Width = 30 },
                new(colTotalSubmissions) { Width = 25 },
                new(colTotalScore) { Width = 16 },
                new(colKpiScore) { Width = 16 }
            };
            foreach (var col in colUserGroup.Distinct())
                dynamicCols.Add(new DynamicExcelColumn(col) { Width = 30 });

            var memoryStream = new MemoryStream();
            MiniExcel.SaveAs(
                stream: memoryStream,
                value: excelData,
                configuration: new OpenXmlConfiguration
                {
                    DynamicColumns = dynamicCols.ToArray()
                }
            );
            memoryStream.Position = 0; // Reset stream position

            // file: Report_CaseFeedback_All_2025-01_Summary_20250101_153025.xlsx 
            return File(
                memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Report_CaseFeedback_All_{SelectedPeriod}_Summary_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx" // Added .xlsx extension
            );
        }
        // Export Excel
        // ---------- Prepare for Excel Export ---------------------------------
        // ----------BY EACH GROUP----------------------------------------------
        if (SingleUserGroupSubmissionsReports.Count > 0)
        {
            var memoryStream = new MemoryStream();
            MiniExcel.SaveAs(
                stream: memoryStream,
                value: SingleUserGroupSubmissionsReports);
            memoryStream.Position = 0; // Reset stream position

            return File(
                    memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report_CaseFeedback_{SelectedPeriod}_{Group}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx" // Added .xlsx extension
                );
        }



        return RedirectToPage();
    }

    // ========== Methods ==================================================
    private List<SelectListItem> LoadUserGroupSelectListItems(List<UserGroupViewModel> userGroups)
    {
        // var userGroups = await _userTitleService.FindAllAsync();
        if (userGroups.Count != 0)
        {
            return userGroups.Select(e => new SelectListItem
            {
                // Value = e.TitleName, //e.Id.ToString(),
                // Text = e.TitleName,
                Value = e.GroupName,
                Text = e.GroupName
            }).ToList();
        }

        ModelState.AddModelError("", "User group not exist. Try to add group and continue.");
        return [];
    }

    private AllUserGroupSubmissionExcelViewModel LoadAllUserGroupSubmissions(
        List<CaseFeedbackSubmission> submissionsByDepartment,
        DepartmentViewModel department)
    {
        List<UserGroupSubmission> userGroupSubmissions = [];

        // **need to group scores by each user group 
        foreach (var group in UserGroups)
        {
            // If submissionsByDepartment is an empty list, 
            // the code will still work correctly without errors.
            var submissionsByGroup = submissionsByDepartment
                .Where(s =>
                    s.SubmittedBy.UserTitle.Id == group.Id)
                .ToList();

            userGroupSubmissions.Add(new UserGroupSubmission
            {
                GroupName = group.GroupName,
                TotalSubmissions = submissionsByGroup.Count,
                TotalScore = submissionsByGroup.Sum(s => s.NegativeScoreValue)
            });
        }

        // Calculate Submissions, Score, KPI Score 
        var totalSubmissions = submissionsByDepartment.Count;
        var totalScore = submissionsByDepartment.Sum(e => e.NegativeScoreValue);
        var kpiScore = (totalSubmissions > 0)
            ? (totalScore / totalSubmissions)
            : 0M;

        return new AllUserGroupSubmissionExcelViewModel
        {
            PeriodName = SelectedPeriod.PeriodName,
            CaseDepartmentName = department.DepartmentName,
            UserGroupSubmissions = userGroupSubmissions, // submissions for each group
            TotalSubmissions = totalSubmissions, // total submission of all group
            TotalScore = totalScore, // total score of all group
            KpiScore = kpiScore // final kpi score
        };
    }

    private SingleUserGroupSubmissionExcelViewModel LoadSingleUserGroupSubmissions(
        List<CaseFeedbackSubmission> submissionsByDepartment,
        DepartmentViewModel department,
        string groupName)
    {

        // If filteredByDepartment is an empty list, 
        // the code will still work correctly without errors.
        var submissions = submissionsByDepartment
            .Where(s => s.SubmittedBy.UserTitle.TitleName == Group)
            .ToList();

        var totalSubmission = submissions.Count;
        var totalScore = submissions.Sum(e => e.NegativeScoreValue);
        var kpiScore = totalSubmission > 0 ? totalScore / totalSubmission : 0M;

        return new SingleUserGroupSubmissionExcelViewModel
        {
            PeriodName = SelectedPeriod.PeriodName,
            CaseDepartmentName = department.DepartmentName,
            UserGroupName = Group,
            TotalSubmissions = totalSubmission,
            TotalScore = totalScore,
            KpiScore = kpiScore
        };
    }
}
