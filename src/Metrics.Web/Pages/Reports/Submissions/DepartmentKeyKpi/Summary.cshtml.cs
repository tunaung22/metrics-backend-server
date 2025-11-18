using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Metrics.Web.Models.DepartmentKeyMetric;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentKeyKpi;

public class SummaryModel(
    IKpiSubmissionPeriodService kpiPeriodService,
    IUserService userService,
    IUserTitleService userGroupService,
    IDepartmentService departmentService,
    IKeyKpiSubmissionService keyKpiSubmissionService,
    IKeyKpiSubmissionConstraintService submissionConstraintService,
    IDepartmentKeyMetricService departmentKeyMetricService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IUserService _userService = userService;
    private readonly IUserTitleService _userGroupService = userGroupService;
    private readonly IDepartmentService _departmentService = departmentService;
    private readonly IKeyKpiSubmissionService _keyKpiSubmissionService = keyKpiSubmissionService;
    private readonly IKeyKpiSubmissionConstraintService _submissionConstraintService = submissionConstraintService;
    public readonly IDepartmentKeyMetricService _departmentKeyMetricService = departmentKeyMetricService;


    // ========== MODELS =======================================================
    // public class AllUserGroup_SummaryReport_ViewModel
    // {
    //     public string KpiPeriodName { get; set; } = null!;
    //     public DepartmentViewModel KeyIssueDepartment { get; set; } = null!;
    //     public int TotalSubmissions { get; set; }
    //     public int TotalKeys { get; set; }
    //     public decimal TotalScore { get; set; }
    //     public decimal KpiScore { get; set; }
    // }

    public class SummaryReport_ViewModel
    {
        public string KpiPeriodName { get; set; } = null!;
        public DepartmentViewModel KeyIssueDepartment { get; set; } = null!;
        public int TotalSubmissions { get; set; }
        public int TotalKeys { get; set; }
        public decimal TotalScore { get; set; }
        public decimal NetScore { get; set; }
        public decimal KpiScore { get; set; }
        public List<SummaryReport_UserGroupDetail_ViewModel> SummaryReport_UserGroupDetails { get; set; } = [];
    }
    public List<SummaryReport_ViewModel> SummaryReportList { get; set; } = [];

    public class SummaryReport_UserGroupDetail_ViewModel
    {
        public required string UserGroupName { get; set; }
        public int TotalSubmissions { get; set; }
        public decimal TotalScore { get; set; }
    }

    // ----------Excel Models----------
    public class SummaryReport_ExcelExport_ViewModel
    {
        [ExcelColumn(Name = "Key Departments")]
        public string? KeyDepartment { get; set; }

        [ExcelColumn(Name = "Total Keys")]
        public int TotalKeys { get; set; }

        [ExcelColumn(Name = "Total Submissions")]
        public int TotalSubmissions { get; set; }

        [ExcelColumn(Name = "Total Score")]
        [DisplayFormat(DataFormatString = "0.00")]
        public decimal TotalScore { get; set; }

        [ExcelColumn(Name = "KPI Score")]
        [DisplayFormat(DataFormatString = "0.00")]
        public decimal KpiScore { get; set; }
    }
    public List<SummaryReport_ExcelExport_ViewModel> SummaryReportExcelData { get; set; } = [];


    // ----------Select/Options Data----------
    [BindProperty]
    public List<SelectListItem> UserGroupListItems { get; set; } = []; // for select element

    [BindProperty(SupportsGet = true)]
    public required string Group { get; set; } // selected item (for filter select element)

    public KpiPeriodViewModel SelectedPeriod { get; set; } = new();
    public List<KeyKpiSubmissionConstraintViewModel> SubmissionConstraints { get; set; } = [];

    public string SelectedPeriodName { get; set; } = null!;
    public List<UserViewModel> SubmitterList { get; set; } = [];
    public List<KeyKpiSubmissionViewModel> KeyKpiSubmissions { get; set; } = [];
    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];
    public List<UserGroupViewModel> UserGroupList { get; set; } = [];
    public List<DepartmentViewModel> KeyIssueDepartmentList { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(string periodName)
    {
        // ----------PERIOD-----------------------------------------------------
        var period = await LoadKpiPeriod(periodName);
        if (period == null)
        {
            ModelState.AddModelError("", $"Period {periodName} not found.");
            return Page();
        }
        SelectedPeriod = period;
        SelectedPeriodName = period.PeriodName;
        // ----------USER GROUPS------------------------------------------------
        UserGroupList = await LoadUserGroups();
        if (UserGroupList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "User Group is empty");
            return Page();
        }
        UserGroupListItems = LoadUserGroupListItems(UserGroupList);

        if (string.IsNullOrEmpty(Group)) { Group = UserGroupListItems[0].Value.ToLower(); }
        // ----------DEPARTMENT KEY METRICS-------------------------------------
        DepartmentKeyMetrics = await LoadDepartmentKeyMetrics(SelectedPeriod.Id); // key departments + keys

        // ----------Key Issue Departments List (Dinstinct)---------------------
        KeyIssueDepartmentList = DepartmentKeyMetrics
            .DistinctBy(x => x.KeyIssueDepartmentId)
            .OrderBy(x => x.KeyIssueDepartment.DepartmentName)
            .Select(x => x.KeyIssueDepartment)
            .ToList();

        // ----------SUBMISSION CONSTRAINTS-------------------------------------
        var dkmIDs = DepartmentKeyMetrics.Select(x => x.Id).ToList();
        SubmissionConstraints = await LoadSubmissionConstraints(dkmIDs);

        // ----------SUBMITTERS-------------------------------------------------
        // Eligible Submitters
        // Users in Departments eligible to Score
        var submitters = await LoadUserList(roleName: "staff");
        var submitterDepartmentIDs = SubmissionConstraints.Select(c => c.SubmitterDepartmentId).ToList(); // Eligible Departments 
        SubmitterList = submitters.Where(submitter => submitterDepartmentIDs.Contains(submitter.DepartmentId)).ToList();

        // Eligible Key Issue Departments
        // var dkms = SubmissionConstraints.Select(e=> e.DepartmentKeyMetricId).ToList();
        // KeyIssueDepartmentList = KeyIssueDepartmentList.Where(e => keyIssueDepartmentIDs.Contains(e.Id)).ToList();
        // var keyIssueDepartmentIDs = DepartmentKeyMetrics.Select(e => e.KeyIssueDepartmentId).ToList();

        // var MODE_SUMMARY = ViewMode.Equals("summary", StringComparison.OrdinalIgnoreCase);
        var GROUP_ALL = Group.Equals("all", StringComparison.OrdinalIgnoreCase);

        // load existing submissions
        var submissions = await _keyKpiSubmissionService.FindByPeriodAsync(SelectedPeriod.Id);
        if (!submissions.IsSuccess || submissions.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed fetching submissions. Please try again.");
            return Page();
        }

        if (GROUP_ALL)
        {
            var submissionsByPeriod_ByAllUserGroup = submissions.Data
                .Select(s => s.MapToViewModel())
                .ToList();

            SummaryReportList = Load_SummaryReportList(
                KeyIssueDepartmentList,
                submissionsByPeriod_ByAllUserGroup,
                true);
        }
        else // SINGLE GROUP
        {
            var submissionsByPeriod_BySingleUserGroup = submissions.Data
                .Where(s => s.SubmittedBy.UserGroup.GroupName.Equals(Group, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.MapToViewModel())
                .ToList();

            SummaryReportList = Load_SummaryReportList(
                KeyIssueDepartmentList,
                submissionsByPeriod_BySingleUserGroup,
                false);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostExportExcelAsync(string periodName)
    {
        // ----------PERIOD-----------------------------------------------------
        var period = await LoadKpiPeriod(periodName);
        if (period == null)
        {
            ModelState.AddModelError("", $"Period {periodName} not found.");
            return Page();
        }
        SelectedPeriod = period;
        SelectedPeriodName = period.PeriodName;
        // ----------USER GROUPS------------------------------------------------
        UserGroupList = await LoadUserGroups();
        if (UserGroupList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "User Group is empty");
            return Page();
        }
        UserGroupListItems = LoadUserGroupListItems(UserGroupList);

        if (string.IsNullOrEmpty(Group)) { Group = UserGroupListItems[0].Value.ToLower(); }
        // ----------DEPARTMENT KEY METRICS-------------------------------------
        DepartmentKeyMetrics = await LoadDepartmentKeyMetrics(SelectedPeriod.Id); // key departments + keys

        // ----------Key Issue Departments List (Dinstinct)---------------------
        KeyIssueDepartmentList = DepartmentKeyMetrics
            .DistinctBy(x => x.KeyIssueDepartmentId)
            .OrderBy(x => x.KeyIssueDepartment.DepartmentName)
            .Select(x => x.KeyIssueDepartment)
            .ToList();

        // ----------SUBMISSION CONSTRAINTS-------------------------------------
        var dkmIDs = DepartmentKeyMetrics.Select(x => x.Id).ToList();
        SubmissionConstraints = await LoadSubmissionConstraints(dkmIDs);

        // ----------SUBMITTERS-------------------------------------------------
        // Eligible Submitters
        // Users in Departments eligible to Score
        var submitters = await LoadUserList(roleName: "staff");
        var submitterDepartmentIDs = SubmissionConstraints.Select(c => c.SubmitterDepartmentId).ToList(); // Eligible Departments 
        SubmitterList = submitters.Where(submitter => submitterDepartmentIDs.Contains(submitter.DepartmentId)).ToList();

        // ==========FILTERS====================================================
        var GROUP_ALL = Group.Equals("all", StringComparison.OrdinalIgnoreCase);

        // load existing submissions
        var submissions = await _keyKpiSubmissionService.FindByPeriodAsync(SelectedPeriod.Id);
        if (!submissions.IsSuccess || submissions.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed fetching submissions. Please try again.");
            return Page();
        }

        if (GROUP_ALL)
        {
            var submissionsByPeriod_ByAllUserGroup = submissions.Data
                .Select(s => s.MapToViewModel())
                .ToList();

            SummaryReportList = Load_SummaryReportList(
                KeyIssueDepartmentList,
                submissionsByPeriod_ByAllUserGroup,
                true);

            // SummaryReportExcelData = SummaryReportList
            //     .Select(e => new SummaryReport_ExcelExport_ViewModel
            //     {
            //         KeyDepartment = e.KeyIssueDepartment.DepartmentName,
            //         TotalKeys = e.TotalKeys,
            //         TotalSubmissions = e.TotalSubmissions,
            //         TotalScore = e.TotalScore,
            //         KpiScore = e.KpiScore,
            //     }).ToList();
            var colPeriod = "KPI Period";
            var colKeyDepartment = "Key Departments";
            var colTotalKey = "Total Keys";
            var colTotalSubmission = "Total Submissions";
            var colTotalScore = "Total Score";
            var colNetScore = "Score / Submitter";
            var colKpiScore = "KPI Score";

            // ----------Dynamic Columns----------
            var dynamicCols = new List<DynamicExcelColumn>
            {
                new(colPeriod) { Width = 20 },
                new(colKeyDepartment) { Width = 25 },
                new(colTotalKey) { Width = 20 },
            }
            .Concat(UserGroupList.SelectMany(group => new DynamicExcelColumn[]
            {
                new($"{group.GroupName}_Name") { Width= 20, Name = $"Group" },
                new($"{group.GroupName}_Submission") { Width= 18, Name = $"Submissions" },
                new($"{group.GroupName}_Score") { Width= 18, Name = "Score" },
            }))
            .Concat(new DynamicExcelColumn[]
            {
                new(colTotalSubmission) { Width = 20 },
                new(colTotalScore) { Width = 20 },
                new(colNetScore) { Width = 20 },
                new(colKpiScore) { Width = 20 },
            })
            .ToList();

            // ----------Prepare Data----------
            List<Dictionary<string, object>> excelData = SummaryReportList
                .Select(e =>
                {
                    var dict = new Dictionary<string, object>
                    {
                        [colPeriod] = e.KpiPeriodName,
                        [colKeyDepartment] = e.KeyIssueDepartment.DepartmentName,
                        [colTotalKey] = e.TotalKeys,
                    };

                    foreach (var group in UserGroupList)
                    {
                        var detail = e.SummaryReport_UserGroupDetails
                            .Where(d => d.UserGroupName.Equals(group.GroupName, StringComparison.OrdinalIgnoreCase))
                            .FirstOrDefault();
                        if (detail != null)
                        {
                            dict[$"{group.GroupName}_Name"] = group.GroupName;
                            dict[$"{group.GroupName}_Submission"] = detail.TotalSubmissions;
                            dict[$"{group.GroupName}_Score"] = detail.TotalScore;
                        }
                        else
                        {
                            dict[$"{group.GroupName}_Name"] = group.GroupName;
                            dict[$"{group.GroupName}_Submission"] = "---";
                            dict[$"{group.GroupName}_Score"] = "---";
                        }
                    }

                    dict[colTotalSubmission] = e.TotalSubmissions;
                    dict[colTotalScore] = Convert.ToDecimal(e.TotalScore.ToString("0.00"));
                    dict[colNetScore] = Convert.ToDecimal(e.NetScore.ToString("0.00"));
                    dict[colKpiScore] = Convert.ToDecimal(e.KpiScore.ToString("0.00"));

                    return dict;
                }
            ).ToList();




            var memoryStream = new MemoryStream();
            MiniExcel.SaveAs(
                stream: memoryStream,
                value: excelData,
                configuration: new OpenXmlConfiguration
                {
                    DynamicColumns = dynamicCols.ToArray(),
                }
            );
            memoryStream.Position = 0;

            return File(
                memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Report_KeyDepartmentKPI_AllUserGroup_Summary_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"
            );
        }
        else // SINGLE GROUP
        {
            var submissionsByPeriod_BySingleUserGroup = submissions.Data
                .Where(s => s.SubmittedBy.UserGroup.GroupName.Equals(Group, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.MapToViewModel())
                .ToList();

            SummaryReportList = Load_SummaryReportList(
                KeyIssueDepartmentList,
                submissionsByPeriod_BySingleUserGroup,
                false);

            var colPeriod = "KPI Period";
            var colKeyDepartment = "Key Departments";
            var colTotalKey = "Total Keys";
            var colTotalSubmission = "Total Submissions";
            var colTotalScore = "Total Score";
            var colKpiScore = "KPI Score";
            var colGroup = "Group";

            // ----------Dynamic Columns----------
            var dynamicCols = new List<DynamicExcelColumn>
            {
                new(colKeyDepartment) { Width = 25 },
                new(colTotalKey) { Width = 20 },
                new(colTotalSubmission) { Width = 20 },
                new(colTotalScore) { Width = 20 },
                new(colKpiScore) { Width = 20 },
                new(colGroup) {Width = 14 },
            };

            // ----------Prepare Data----------
            List<Dictionary<string, object>> excelData = SummaryReportList
                .Select(e =>
                {
                    var dict = new Dictionary<string, object>
                    {
                        [colPeriod] = e.KpiPeriodName,
                        [colKeyDepartment] = e.KeyIssueDepartment.DepartmentName,
                        [colTotalKey] = e.TotalKeys,
                    };
                    // add group name
                    dict[colGroup] = Group.ToUpper();
                    dict[colTotalSubmission] = e.TotalSubmissions;
                    dict[colTotalScore] = Convert.ToDecimal(e.TotalScore.ToString("0.00"));
                    dict[colKpiScore] = Convert.ToDecimal(e.KpiScore.ToString("0.00"));
                    return dict;
                }).ToList();


            var memoryStream = new MemoryStream();
            MiniExcel.SaveAs(
                stream: memoryStream,
                value: excelData,
                configuration: new OpenXmlConfiguration
                {
                    DynamicColumns = dynamicCols.ToArray(),
                }
            );
            memoryStream.Position = 0;

            return File(
                memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Report_KeyDepartmentKPI_{Group}_Summary_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"
            );
        }
    }


    // ========== Methods ==================================================
    private async Task<KpiPeriodViewModel?> LoadKpiPeriod(string periodName)
    {
        var kpiPeriod = await _kpiPeriodService
            .FindByKpiPeriodNameAsync(periodName);

        if (kpiPeriod != null)
        {
            return new KpiPeriodViewModel()
            {
                Id = kpiPeriod.Id,
                PeriodName = kpiPeriod.PeriodName,
                SubmissionStartDate = kpiPeriod.SubmissionStartDate,
                SubmissionEndDate = kpiPeriod.SubmissionEndDate
            };
        }

        return null;
    }

    private async Task<List<UserGroupViewModel>> LoadUserGroups()
    {
        var userGroups = (await _userGroupService.FindAllAsync())
            // sort by name
            .OrderBy(g => g.TitleName);


        if (userGroups.Any())
        {
            return userGroups
                // filter user without staff
                .Where(g => !g.TitleName.Equals("staff", StringComparison.OrdinalIgnoreCase)
                    && !g.TitleName.Equals("sysadmin", StringComparison.OrdinalIgnoreCase))
                .Select(g => new UserGroupViewModel
                {
                    Id = g.Id,
                    GroupCode = g.TitleCode,
                    GroupName = g.TitleName,
                    Description = g.Description
                }).ToList();
        }

        return [];
    }

    private List<SelectListItem> LoadUserGroupListItems(List<UserGroupViewModel> userGroups)
    {
        if (userGroups.Count > 0)
        {
            // add All item before user group items
            var items = new List<SelectListItem>()
            {
                new() { Value = "all", Text = "All" }
            };

            foreach (var group in userGroups)
            {
                items.Add(new()
                {
                    Value = group.GroupName.ToLower(),
                    Text = group.GroupName,
                });
            }

            return items;
        }

        ModelState.AddModelError("", "User group does not exist. Try to add group and continue.");
        return [];
    }

    /// <summary>
    /// Load Department Key Metrics by Period
    /// </summary>
    /// <param name="periodId"></param>
    /// <returns></returns>
    private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeyMetrics(long periodId)
    {
        var dkmResult = await _departmentKeyMetricService.FindByPeriodIdAsync(periodId);

        if (dkmResult.IsSuccess && dkmResult.Data != null)
        {
            return dkmResult.Data.Select(e => e.MapToViewModel()).ToList();
        }
        ModelState.AddModelError(string.Empty, "Department keys does not exist.");
        return [];
    }
    /// <summary>
    /// Load Submission Constraints by Department Keys (Metrics)
    /// </summary>
    /// <param name="dkmIDs"></param>
    /// <returns></returns>
    private async Task<List<KeyKpiSubmissionConstraintViewModel>> LoadSubmissionConstraints(List<long> dkmIDs)
    {
        var constraintsByDKMResult = await _submissionConstraintService.FindByDepartmentKeyMetricsAsync(dkmIDs);
        if (constraintsByDKMResult.IsSuccess && constraintsByDKMResult.Data != null)
        {
            return constraintsByDKMResult.Data
                .Select(c => c.MapToViewModel()).ToList();
        }
        ModelState.AddModelError(string.Empty, "Failed to load constriants. Please try again.");
        return [];
    }

    private List<SummaryReport_ViewModel> Load_SummaryReportList(
        List<DepartmentViewModel> DepartmentList,
        List<KeyKpiSubmissionViewModel> submissionsByPeriod_ByAllUserGroup,
        bool IsAllGroup)
    {
        //
        var data = DepartmentList.Select(department =>
        {
            var keysCount = DepartmentKeyMetrics.Where(dkm => dkm.KeyIssueDepartmentId == department.Id).Count();

            var submissionFilteredByDepartment = submissionsByPeriod_ByAllUserGroup.Where(s => s.DepartmentKeyMetric.KeyIssueDepartmentId == department.Id).ToList();

            var submissionFilteredByUser = submissionFilteredByDepartment
                .DistinctBy(s => s.SubmitterId).Count();


            var submissionCount = submissionFilteredByDepartment.Count();
            decimal totalScore = submissionFilteredByDepartment.Sum(s => s.ScoreValue);
            // decimal totalKPI = totalScore > 0 ? (totalScore / submissionCount) : 0;
            decimal netKPI = totalScore > 0 ? (totalScore / submissionFilteredByUser) : 0;
            decimal totalKPI = netKPI > 0 ? (netKPI / keysCount) : 0;

            // if All Group
            if (IsAllGroup)
            {
                // get data for each user group details: SummaryReport_UserGroupDetails
                // SummaryReport_UserGroupDetail_ViewModel
                var groupDetails = UserGroupList
                    .Select(group =>
                    {
                        var submissionsByGroup = submissionFilteredByDepartment
                            .Where(s => s.SubmittedBy.UserGroup.Id == group.Id)
                            .ToList();
                        var submissionCount = submissionsByGroup.Count();
                        var totalScore = submissionsByGroup.Sum(s => s.ScoreValue);
                        return new SummaryReport_UserGroupDetail_ViewModel
                        {
                            UserGroupName = group.GroupName,
                            TotalSubmissions = submissionCount,
                            TotalScore = totalScore,
                        };
                    })
                    .ToList();

                var departmentInfo = new SummaryReport_ViewModel
                {
                    KpiPeriodName = SelectedPeriodName,
                    KeyIssueDepartment = department,
                    TotalKeys = keysCount,
                    TotalSubmissions = submissionCount,
                    TotalScore = totalScore,
                    NetScore = netKPI,
                    KpiScore = totalKPI,
                    SummaryReport_UserGroupDetails = groupDetails
                };
                return departmentInfo;
            }
            else
            {
                var departmentInfo = new SummaryReport_ViewModel
                {
                    KpiPeriodName = SelectedPeriodName,
                    KeyIssueDepartment = department,
                    TotalKeys = keysCount,
                    TotalSubmissions = submissionCount,
                    TotalScore = totalScore,
                    KpiScore = totalKPI
                };
                return departmentInfo;
            }


        }).ToList();

        return data;
    }

    /// <summary>
    /// Load Users by Role name
    /// </summary>
    /// <param name="roleName"></param>
    /// <returns></returns>
    private async Task<List<UserViewModel>> LoadUserList(string roleName)
    {
        var users = await _userService.FindAllActiveAsync(roleName);

        if (users.Any())
        {
            return users
                .Where(user => !user.UserTitle.TitleName // remove users with user title "Staff" 
                    .Equals("staff", StringComparison.OrdinalIgnoreCase))
                .Select(user => new UserViewModel
                {
                    Id = user.Id,
                    UserCode = user.UserCode,
                    UserName = user.UserName ?? "unknown username",
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    ContactAddress = user.ContactAddress,
                    DepartmentId = user.DepartmentId,
                    Department = new DepartmentViewModel
                    {
                        Id = user.Department.Id,
                        DepartmentCode = user.Department.DepartmentCode,
                        DepartmentName = user.Department.DepartmentName
                    },
                    UserGroup = new UserGroupViewModel
                    {
                        Id = user.UserTitle.Id,
                        GroupCode = user.UserTitle.TitleCode,
                        GroupName = user.UserTitle.TitleName,
                        Description = user.UserTitle.Description
                    }
                }).ToList();
        }

        return [];
    }



}
