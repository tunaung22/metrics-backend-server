using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Metrics.Web.Models.DepartmentKeyMetric;
using Metrics.Web.Models.KeyKpiSubmissionConstraint;
using Metrics.Web.Models.ReportViewModels.KeyKpi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentKeyKpi;

public class DetailModel(
    IKpiSubmissionPeriodService kpiPeriodService,
    IUserService userService,
    IUserTitleService userGroupService,
    // IDepartmentService departmentService,
    IKeyKpiSubmissionService keyKpiSubmissionService,
    IKeyKpiSubmissionConstraintService submissionConstraintService,
    IDepartmentKeyMetricService departmentKeyMetricService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IUserService _userService = userService;
    private readonly IUserTitleService _userGroupService = userGroupService;
    // private readonly IDepartmentService _departmentService = departmentService;
    private readonly IKeyKpiSubmissionService _keyKpiSubmissionService = keyKpiSubmissionService;
    private readonly IKeyKpiSubmissionConstraintService _submissionConstraintService = submissionConstraintService;
    public readonly IDepartmentKeyMetricService _departmentKeyMetricService = departmentKeyMetricService;


    // ========== MODELS =======================================================
    // -----model for Report Detail View for both All user group + single user group----
    public List<KeyKpi_ReportDetail_ViewModel> KeyKpi_DetailReportList { get; set; } = [];
    public class KeyKpi_ReportDetail_ViewModel
    {
        public string PeriodName { get; set; } = null!;
        public UserViewModel SubmittedBy { get; set; } = null!;
        public List<KeyKpi_ReportDetailItem_ViewModel> KeyKpi_DetailReportItems { get; set; } = [];
    }
    public class KeyKpi_ReportDetailItem_ViewModel
    {
        public required DepartmentKeyMetricViewModel DepartmentKeyMetric { get; set; }
        public decimal ScoreValue { get; set; }
        public string? Comments { get; set; } = string.Empty;
    }
    // -------------------------------------------------------------------------

    public List<KeyKpi_AllUserGroup_ReportDetailViewModel> AllUserGroup_DetailList { get; set; } = [];
    public List<KeyKpi_AllUserGroup_ReportSummaryViewModel> AllUserGroup_SummaryList { get; set; } = [];
    public List<KeyKpi_SingleUserGroup_ReportSummaryViewModel> SingleUserGroup_SummaryList { get; set; } = [];
    public List<KeyKpi_SingleUserGroup_ReportDetailViewModel> SingleUserGroup_DetailList { get; set; } = [];

    public List<UserViewModel> SubmitterList { get; set; } = [];
    public List<KeyKpiSubmissionViewModel> KeyKpiSubmissions { get; set; } = [];
    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];
    public List<KeyKpiSubmissionConstraintViewModel> SubmissionConstraints { get; set; } = [];
    public KpiPeriodViewModel SelectedPeriod { get; set; } = new();

    // public List<UserViewModel> EligibleSubmitters { get; set; } = [];
    // public List<DepartmentViewModel> EligibleDepartments { get; set; } = [];

    // ----------Excel Models----------
    // public class KeyKpiSubmissionExportViewModel
    // {
    // }
    // public List<KeyKpiSubmissionExportViewModel>
    public string SelectedPeriodName { get; set; } = null!;

    public class KeyIssueDepartmentViewModel
    {
        public long Id { get; set; }
        public Guid DepartmentCode { get; set; }
        public required string DepartmentName { get; set; }
        public required List<DepartmentKeyMetricViewModel> DepartmentKeys { get; set; } = [];
    }
    public List<KeyIssueDepartmentViewModel> KeyIssueDepartmentList2 { get; set; } = [];
    public List<DepartmentViewModel> KeyIssueDepartmentList { get; set; } = [];
    public List<UserGroupViewModel> UserGroupList { get; set; } = [];

    // ----------Select/Options Data----------
    [BindProperty]
    public List<SelectListItem> UserGroupListItems { get; set; } = []; // for select element
    [BindProperty]
    public List<SelectListItem> SubmitterDepartmentListItems { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public required string Group { get; set; } // selected item (for filter select element)

    [BindProperty(SupportsGet = true)]
    public required string Department { get; set; } // keep to value of selected item

    [BindProperty]
    public List<SelectListItem> ReportViewModeListItems { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? ViewMode { get; set; } // selected item (for filter select element)


    // ========== HANDLERS =====================================================
    public async Task<IActionResult> OnGetAsync(string periodName)
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }

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
        if (string.IsNullOrEmpty(Group)) { Group = UserGroupListItems[0].Value; }

        // ----------DEPARTMENT KEY METRICS-------------------------------------
        DepartmentKeyMetrics = await LoadDepartmentKeyMetrics(SelectedPeriod.Id); // key departments + keys

        // ----------Key Issue Departments List (Dinstinct)---------------------
        KeyIssueDepartmentList = DepartmentKeyMetrics
            .DistinctBy(x => x.KeyIssueDepartmentId)
            .OrderBy(x => x.KeyIssueDepartment.DepartmentName)
            .Select(x => x.KeyIssueDepartment)
            .ToList();



        foreach (var keyDepartment in KeyIssueDepartmentList)
        {
            var keysOfDepartment = DepartmentKeyMetrics
                .Where(dkm => dkm.KeyIssueDepartmentId == keyDepartment.Id)
                .ToList();
            KeyIssueDepartmentList2.Add(new KeyIssueDepartmentViewModel
            {
                Id = keyDepartment.Id,
                DepartmentCode = keyDepartment.DepartmentCode,
                DepartmentName = keyDepartment.DepartmentName,
                DepartmentKeys = keysOfDepartment
            });
        }

        // ----------SUBMISSION CONSTRAINTS-------------------------------------
        var dkmIDs = DepartmentKeyMetrics.Select(x => x.Id).ToList();
        SubmissionConstraints = await LoadSubmissionConstraints(dkmIDs);

        // ----------SUBMITTER DEPARTMENTS--------------------------------------
        var submitterDepartments = SubmissionConstraints
            .DistinctBy(c => c.SubmitterDepartmentId)
            .Select(c => c.CandidateDepartment).ToList();
        if (submitterDepartments.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Submitter department is empty");
            return Page();
        }
        SubmitterDepartmentListItems = LoadSubmitterDepartmentListItems(submitterDepartments);
        if (string.IsNullOrEmpty(Department)) { Department = SubmitterDepartmentListItems[0].Value; }

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


        // ==========FILTERS====================================================
        var GROUP_ALL = Group.Equals("all", StringComparison.OrdinalIgnoreCase);

        // -----FILTER----- by [Period]
        var submissionsResult = await _keyKpiSubmissionService.FindByPeriodAsync(SelectedPeriod.Id);
        if (!submissionsResult.IsSuccess || submissionsResult.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed fetching submissions. Please try again.");
            return Page();
        }
        var submissions = submissionsResult.Data.Select(e => e.MapToViewModel()).ToList();

        // -----FILTER----- by [Submitter Department]
        var ALL_DEPARTMENT = Department.Equals("all", StringComparison.OrdinalIgnoreCase);
        if (!ALL_DEPARTMENT)  // Filter submissions by single submitter department
        {
            submissions = submissions
                .Where(s => s.SubmittedBy.Department.DepartmentName.Equals(Department, StringComparison.OrdinalIgnoreCase))
                .ToList();
            SubmitterList = SubmitterList
                .Where(submitter => submitter.Department.DepartmentName.Equals(Department, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // -----FILTER----- by [Single Group]
        if (!GROUP_ALL)
        {
            submissions = submissions
                .Where(s => s.SubmittedBy.UserGroup.GroupName.Equals(Group, StringComparison.OrdinalIgnoreCase))
                .ToList();
            SubmitterList = SubmitterList
                .Where(submitter => submitter.UserGroup.GroupName.Equals(Group, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }



        // Load Submissions
        KeyKpi_DetailReportList = Load_DetailReportList(
            allSubmissionsByPeriod: submissions,
            submitterList: SubmitterList,
            keyIssueDepartmentList: KeyIssueDepartmentList,
            keyIssueDepartmentList2: KeyIssueDepartmentList2);

        return Page();
    }

    public async Task<IActionResult> OnPostExportExcelAsync(string periodName)
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }

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
        if (string.IsNullOrEmpty(Group)) { Group = UserGroupListItems[0].Value; }

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

        // ----------SUBMITTER DEPARTMENTS--------------------------------------
        var submitterDepartments = SubmissionConstraints
            .DistinctBy(c => c.SubmitterDepartmentId)
            .Select(c => c.CandidateDepartment).ToList();
        if (submitterDepartments.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Submitter department is empty");
            return Page();
        }
        SubmitterDepartmentListItems = LoadSubmitterDepartmentListItems(submitterDepartments);
        if (string.IsNullOrEmpty(Department)) { Department = SubmitterDepartmentListItems[0].Value; }

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


        // ==========FILTERS====================================================
        var GROUP_ALL = Group.Equals("all", StringComparison.OrdinalIgnoreCase);

        // -----FILTER----- by [Period]
        var submissionsResult = await _keyKpiSubmissionService.FindByPeriodAsync(SelectedPeriod.Id);
        if (!submissionsResult.IsSuccess || submissionsResult.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed fetching submissions. Please try again.");
            return Page();
        }
        var submissions = submissionsResult.Data.Select(e => e.MapToViewModel()).ToList();

        // -----FILTER----- by [Submitter Department]
        var ALL_DEPARTMENT = Department.Equals("all", StringComparison.OrdinalIgnoreCase);
        if (!ALL_DEPARTMENT)  // Filter submissions by single submitter department
        {
            submissions = submissions
                .Where(s => s.SubmittedBy.Department.DepartmentName.Equals(Department, StringComparison.OrdinalIgnoreCase))
                .ToList();
            SubmitterList = SubmitterList
                .Where(submitter => submitter.Department.DepartmentName.Equals(Department, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // -----FILTER----- by [Single Group]
        if (!GROUP_ALL)
        {
            submissions = submissions
                .Where(s => s.SubmittedBy.UserGroup.GroupName.Equals(Group, StringComparison.OrdinalIgnoreCase))
                .ToList();
            SubmitterList = SubmitterList
                .Where(submitter => submitter.UserGroup.GroupName.Equals(Group, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Load Submissions
        KeyKpi_DetailReportList = Load_DetailReportList(
            allSubmissionsByPeriod: submissions,
            submitterList: SubmitterList,
            keyIssueDepartmentList: KeyIssueDepartmentList,
            keyIssueDepartmentList2: KeyIssueDepartmentList2);


        // ----------Dynamic Columns----------
        var colPeriod = "kpi_period";
        var colCandidateCode = "candidate_code";
        var colCandidateName = "candidate_name";
        var colCandidateDepartment = "candidate_department";
        var colCandidateGroup = "candidate_group";
        var colKeyDepartment = "key_department";
        var colKeyTitle = "key_title";
        var colScore = "score";
        var colComment = "comment";
        // var keyIssueDepartmentColumns = new List<DynamicExcelColumn>();
        // for (int i = 0; i < KeyIssueDepartmentList2.Count; i++)
        // {
        //     keyIssueDepartmentColumns.Add(new DynamicExcelColumn($"{KeyIssueDepartmentList2[i].Id}_name") { Width = 20, Name = $"{KeyIssueDepartmentList2[i].DepartmentName}" });
        // }
        var dynamicCols = new List<DynamicExcelColumn>
        {
            new(colPeriod) {Width = 20, Name = "KPI Period"},
            new(colCandidateCode) {Width = 20, Name ="Code"},
            new(colCandidateName) {Width = 20, Name ="Name"},
            new(colCandidateDepartment) {Width = 20, Name="Department"},
            new(colCandidateGroup) {Width = 20, Name="Group"},
        }
        .Concat(KeyIssueDepartmentList2.SelectMany(department => new DynamicExcelColumn[]
        {
            // new($"{department.Id}_name") { Width = 20, Name = $"{department.DepartmentName}" },
            // new($"{department.Id}_key") { Width = 20, Name = $"{department.}" },
            // new($"{department.Id}_score") { Width = 20, Name = $"{}" },
            // new($"{department.Id}_comment") { Width = 20, Name = $"{}" },
        }))
        .Concat(new DynamicExcelColumn[]
        {
            new(colKeyDepartment) {Width=20, Name="Key Department"},
            new(colKeyTitle) {Width=20, Name="Key Title"},
            new(colScore) {Width=20, Name="Score"},
            new(colComment) {Width=20, Name="Comments"},

        })
        .ToList();

        // ----------Prepare Data----------
        List<Dictionary<string, object>> excelData = KeyKpi_DetailReportList
            .SelectMany(e => e.KeyKpi_DetailReportItems.Select(d => new Dictionary<string, object>
            {
                [colPeriod] = e.PeriodName,
                [colCandidateCode] = e.SubmittedBy.UserCode,
                [colCandidateName] = e.SubmittedBy.FullName,
                [colCandidateDepartment] = e.SubmittedBy.Department.DepartmentName,
                [colCandidateGroup] = e.SubmittedBy.UserGroup.GroupName,
                [colKeyDepartment] = d.DepartmentKeyMetric.KeyIssueDepartment.DepartmentName,
                [colKeyTitle] = d.DepartmentKeyMetric.KeyMetric.MetricTitle,
                [colScore] = d.ScoreValue,
                [colComment] = d.Comments ?? string.Empty,
            }))
            .ToList();

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
            $"Report_KeyDepartmentKPI_Detail_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"
        );
    }


    // period
    // candidate id
    // candidate name
    // candidate department
    // candidate group
    // List of DepartmentDetail
    //              name
    //              key
    //              score
    //              comment

    public class DetailReport_ViewModel
    {

    }
    public class DetailReport_DepartmentDetail_ViewModel
    {

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
                // filter out "sysadmin" user
                // filter out "staff" user group
                // filter out "management" user group
                .Where(g =>
                    !g.TitleName.Equals("sysadmin", StringComparison.OrdinalIgnoreCase)
                    && !g.TitleName.Equals("staff", StringComparison.OrdinalIgnoreCase))
                // && !g.TitleName.Equals("management", StringComparison.OrdinalIgnoreCase))
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

    /// <summary>
    /// Load user groups for SelectListItem
    /// </summary>
    /// <param name="userGroups"></param>
    /// <returns>List<SelectListItem></returns>
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
                items.Add(new SelectListItem
                {
                    Value = group.GroupName,
                    Text = group.GroupName,
                });
            }

            return items;
        }

        ModelState.AddModelError("", "User group does not exist. Try to add group and continue.");
        return [];
    }

    /// <summary>
    /// Load submitter departments for SelectListItem
    /// </summary>
    /// <param name="submitterDepartments"></param>
    /// <returns>List<SelectListItem></returns>
    private List<SelectListItem> LoadSubmitterDepartmentListItems(List<DepartmentViewModel> submitterDepartments)
    {
        if (submitterDepartments.Count > 0)
        {
            // add All item before user group items
            var items = new List<SelectListItem>()
            {
                new() { Value = "all", Text = "All" }
            };

            foreach (var department in submitterDepartments)
            {
                items.Add(new SelectListItem
                {
                    Value = department.DepartmentName,
                    Text = department.DepartmentName
                });
            }
            return items;
        }
        ModelState.AddModelError("", "Submitter departments does not exist.");
        return [];
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
                .Where(user =>
                    // remove users with user title "Staff" 
                    !user.UserTitle.TitleName.Equals("sysadmin", StringComparison.OrdinalIgnoreCase)
                    && !user.UserTitle.TitleName.Equals("staff", StringComparison.OrdinalIgnoreCase))
                // && !user.UserTitle.TitleName.Equals("management", StringComparison.OrdinalIgnoreCase))
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


    // private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeyMetrics(
    //     string periodName,
    //     Guid departmentCode)
    // {
    //     var dkms = await _departmentKeyMetricService
    //         .FindAllByPeriodAndDepartmentAsync(periodName, departmentCode);

    //     return dkms.Select(dkm => MapEntityToViewModel(dkm)).ToList();
    // }

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

    private List<KeyKpi_AllUserGroup_ReportSummaryViewModel> Load_AllUserGroup_SummaryList(
        List<DepartmentViewModel> DepartmentList,
        List<KeyKpiSubmissionViewModel> allSubmissionsByPeriod)
    {
        var data = DepartmentList.Select(department =>
        {
            // await service.FindByKpiPeriodAndDepartmentAndUserGroupAsync(SelectedPeriodId, departmentId, usertitle);
            // Submission to Department by Period
            var submissionToDepartment = allSubmissionsByPeriod
                .Where(s => s.DepartmentKeyMetric.KeyIssueDepartmentId == department.Id)
                .ToList();

            // User Group Submission Info (group name, count, total score)
            var groupScores = UserGroupList.Select(group =>
            {
                // Submission by User Group to Department by Period
                var submittedByGroup = submissionToDepartment
                    .Where(s => s.SubmittedBy.UserGroup.Id == group.Id)
                    .ToList();

                // var totalKeysSubmitted_ByGroup = submittedByGroup.Select(s =>
                //     s.KeyKpiSubmissionItems.DistinctBy(item =>
                //         item.DepartmentKeyMetricId)).Count();
                var totalKeysSubmitted_ByGroup = -1;
                // var totalSubmission_ByGroup = submittedByGroup
                //     .Select(s => s.KeyKpiSubmissionItems).Count();
                var totalSubmission_ByGroup = -1;
                // var totalScore_ByGroup = submittedByGroup
                //     .SelectMany(s => s.KeyKpiSubmissionItems
                //         .Select(item => item.ScoreValue)).Sum();
                var totalScore_ByGroup = -1;

                return new KeyKpi_UserGroup_SubmissionInfo
                {
                    // summary by user group
                    GroupName = group.GroupName,
                    Keys = totalKeysSubmitted_ByGroup,
                    TotalSubmissions = totalSubmission_ByGroup,
                    TotalScore = totalScore_ByGroup
                };
            }).ToList();

            var totalKeys = -1;
            var totalSubmissions = submissionToDepartment.Count;
            var totalScore = -1;
            // var totalScore = submissionToDepartment
            //     .Select(s => s.KeyKpiSubmissionItems).Count();
            var kpiScore = -1;

            // Return the Result for each Department
            return new KeyKpi_AllUserGroup_ReportSummaryViewModel
            {
                PeriodName = SelectedPeriod.PeriodName,
                DepartmentName = department.DepartmentName,
                UserGroupSubmissionInfo = groupScores,
                TotalKeys = totalKeys,
                TotalSubmissions = totalSubmissions,
                TotalScore = totalScore,
                KpiScore = kpiScore
            };
        }).ToList();

        return data;
    }






    /// <summary>
    /// Load AllUserGroup_DetailList
    /// Takes submitterList to include all submitter (sorting)
    /// Taks keyIssueDepartmentList to include all Key Issue Department (sorting)
    /// </summary>
    /// <param name="allSubmissionsByPeriod"></param>
    /// <param name="submitterList"></param>
    /// <param name="keyIssueDepartmentList"></param>
    /// <returns></returns>
    private List<KeyKpi_ReportDetail_ViewModel> Load_DetailReportList(
        List<KeyKpiSubmissionViewModel> allSubmissionsByPeriod,
        List<UserViewModel> submitterList, // eligible users
        List<DepartmentViewModel> keyIssueDepartmentList, // departments who issue keys
        List<KeyIssueDepartmentViewModel> keyIssueDepartmentList2)
    {
        // NOTE: need to fetch for all submitter (including not submitted user's submission that score will be set to 0.00)
        // 1. need all Submitters
        // 2. need all Keys of Departments

        // // find eligible department, get eligible users
        // var dkmIds = DepartmentKeyMetrics.Select(x => x.Id).ToList();
        // var eligibleDepartments = submissionConstrains.Select(c => c.SubmitterDepartmentId).ToList();
        // var eligibleUsers = submitterList.Where(submitter => eligibleDepartments.Contains(submitter.DepartmentId)).ToList();

        // each Submitter -> List<departments> -> each department -> List<DKM> -> each DKM -> List<MODLE>
        List<KeyKpi_ReportDetail_ViewModel> data = submitterList
            // .Where(submitter => submitter.DepartmentId)
            .Select(submitter =>
        {
            // for each Submitter, get Key Issue Departments
            // for each Key Issue Department, get Keys
            var items = keyIssueDepartmentList.SelectMany(department =>
            {
                var dkms = DepartmentKeyMetrics.OrderBy(dkm => dkm.KeyMetric.MetricTitle).Where(dkm => dkm.KeyIssueDepartmentId == department.Id).ToList();

                return dkms.Select(dkm =>
                {
                    // for each Keys, get Submission
                    var submission = allSubmissionsByPeriod
                        .Where(submission => submission.DepartmentKeyMetricId == dkm.Id
                            && submission.SubmitterId == submitter.Id)
                        .FirstOrDefault();
                    if (submission != null)
                    {
                        return new KeyKpi_ReportDetailItem_ViewModel
                        {
                            DepartmentKeyMetric = dkm, //submission.DepartmentKeyMetric,
                            ScoreValue = submission.ScoreValue,
                            Comments = submission.Comments ?? string.Empty,
                        };
                    }
                    else
                    {
                        return new KeyKpi_ReportDetailItem_ViewModel
                        {
                            DepartmentKeyMetric = dkm,
                            ScoreValue = 0.00M,
                            Comments = string.Empty
                        };
                    }
                }).ToList();
            }).ToList();

            return new KeyKpi_ReportDetail_ViewModel
            {
                PeriodName = SelectedPeriod.PeriodName,
                SubmittedBy = submitter,
                KeyKpi_DetailReportItems = items
            };



            // // Each user's submissions (for single row data)
            // var items = allSubmissionsByPeriod
            //     .OrderBy(submission => submission.DepartmentKeyMetric.KeyIssueDepartment.DepartmentName)
            //     .Select(submission =>
            // {
            //     return new KeyKpi_AllUserGroup_ReportDetailItem_ViewModel2
            //     {
            //         DepartmentKeyMetric = submission.DepartmentKeyMetric,
            //         ScoreValue = submission.ScoreValue,
            //         Comments = submission.Comments
            //     };
            // })
            // .ToList();

            // return new KeyKpi_AllUserGroup_ReportDetail_ViewModel2
            // {
            //     PeriodName = SelectedPeriod.PeriodName,
            //     SubmittedBy = submitter,
            //     KeyKpi_AllUserGroup_ReportDetailItem = items
            // };
        })
        .ToList();

        return data;
    }

    private List<KeyKpi_ReportDetail_ViewModel> Load_ReportDetailList(
        List<KeyKpiSubmissionViewModel> submissions,
        List<UserViewModel> submitterList, // eligible users
        List<DepartmentViewModel> keyIssueDepartmentList) // departments who issue keys
    {
        List<KeyKpi_ReportDetail_ViewModel> data = submitterList.Select(submitter =>
        {
            // for each Submitter, get Key Issue Departments
            // for each Key Issue Department, get Keys
            var items = keyIssueDepartmentList.SelectMany(keyDepartment =>
            {
                // for each Department's Key Metrics -> get Keys
                var departmentKeys = DepartmentKeyMetrics
                    .OrderBy(dkm => dkm.KeyMetric.MetricTitle)
                    .Where(dkm => dkm.KeyIssueDepartmentId == keyDepartment.Id)
                    .ToList();

                return departmentKeys.Select(dkm =>
                {
                    // for each Key, get submitter's Submission data
                    // **submission by Submitter + by DKM 
                    //      => will get unique one submission
                    var submission = submissions
                        .Where(submission => submission.DepartmentKeyMetricId == dkm.Id
                            && submission.SubmitterId == submitter.Id)
                        .FirstOrDefault();
                    if (submission != null)
                    {
                        return new KeyKpi_ReportDetailItem_ViewModel
                        {
                            DepartmentKeyMetric = dkm,
                            ScoreValue = submission.ScoreValue,
                            Comments = submission.Comments
                        };
                    }
                    else
                    {
                        return new KeyKpi_ReportDetailItem_ViewModel
                        {
                            DepartmentKeyMetric = dkm,
                            ScoreValue = 0.00M,
                            Comments = string.Empty
                        };
                    }
                }).ToList();
            }).ToList();

            return new KeyKpi_ReportDetail_ViewModel
            {
                PeriodName = SelectedPeriod.PeriodName,
                SubmittedBy = submitter,
                KeyKpi_DetailReportItems = items
            };
        }).ToList();

        return data;
    }

    // 2.0 for each DEPARTMENT (of a user), 
    // 2.1 -> filter the SUBMISSIONS by USER and by DEPARTMENT
    // TODO: We could just call service method directly as: _keyKpiSubmissionService.FindAsync(SelectedPeriod.Id, user.Id, department.Id);
    // var submissionByPeriodByUserByDepartment = allSubmissionsByPeriod
    //     .Where(s => s.ApplicationUserId == user.Id && s.DepartmentId == department.Id)
    //     .FirstOrDefault();
    // var submissionByPeriodByUserByDepartment = allSubmissionsByPeriod
    //     .Where(s => s.SubmitterId == user.Id && s.DepartmentKeyMetric.KeyIssueDepartmentId == department.Id)
    //     .FirstOrDefault();

    // **အမှတ်မပေးရသေးတဲ့ စာရင်းလည်း ပါချင်တာဆိုတော့ submission ကိုနေယူလို့မရဘူး
    // submission ကနေယူရင် အမှတ်ပေးထားတဲ့စာရင်းပဲ ပါလာမယ်။
    // DepartmentKeyMetric ကနေပြန်ဆွဲထုတ်ရင် အမှတ်မပေးထားတဲ့ key တွေလည်းရမယ်
    // NOTE: SELECT Submissions.Items WHERE Submission.Items.DepartmentKeyMetricId == DepartmentKeyMetric.Id
    // for each DKMs, -> get SUBMISSIONS :: WHERE DKM.Id == SUBMISSION's dkmId
    // filter the DKMs by DEPARTMENT (which contains non-duplicated KEYS)
    // var details = DepartmentKeyMetrics
    //     .Where(dkm => dkm.KeyIssueDepartmentId == department.Id)
    //     .OrderBy(dkm => dkm.KeyIssueDepartment.DepartmentName)
    //     .Select(dkm =>
    //     {

    //         // 3.0 for each DEPARTMENT KEY METRIC list (DKMs)
    //         // 3.1 -> filter SubmissionItem by DKM.Id
    //         // LoadSubmissionItemByDkm(submission, dkm.Id);
    //         var submissionItem = submissionByPeriodByUserByDepartment;
    //         // var submissionItem = submissionByPeriodByUserByDepartment?.KeyKpiSubmissionItems
    //         //     .Where(i => i.DepartmentKeyMetricId == dkm.Id)
    //         //     .FirstOrDefault();

    //         // return DepartmentScoreDetail regardless of submission data
    //         if (submissionItem != null)
    //         {
    //             return new KeyKpi_DepartmentScoreDetail
    //             {
    //                 DepartmentKeyMetric = dkm, //
    //                 DKMId = dkm.Id,
    //                 KeyId = dkm.KeyMetricId,
    //                 KeyIssueDepartmentName = department.DepartmentName, //
    //                 KeyTitle = dkm.KeyMetric.MetricTitle,
    //                 ScoreValue = submissionItem.ScoreValue, //
    //                 Comments = submissionItem.Comments ?? string.Empty, //
    //             };
    //         }
    //         // submissionByPeriodByUserByDepartment == null || 
    //         // submissionByPeriodByUserByDepartment.KeyKpiSubmissionItems == null)
    //         return new KeyKpi_DepartmentScoreDetail
    //         {
    //             DepartmentKeyMetric = null,
    //             KeyIssueDepartmentName = dkm.KeyIssueDepartment.DepartmentName,
    //             KeyTitle = dkm.KeyMetric.MetricTitle,
    //             KeyId = dkm.KeyMetricId,
    //             DKMId = dkm.Id,
    //             ScoreValue = 0,
    //             Comments = string.Empty,
    //         };
    //     }).ToList();
    // return details;
    // })
    // .ToList();




    // TODO: Should we combine Load_AllUserGroup_DetailList and Load_SingleUserGroup_DetailList?? 
    private List<KeyKpi_SingleUserGroup_ReportDetailViewModel> Load_SingleUserGroup_DetailList(
        List<KeyKpiSubmissionViewModel> allSubmissionsByPeriod,
        List<UserViewModel> userList,
        List<DepartmentViewModel> departmentList,
        string groupName)
    {
        // var submissionByGroup = allSubmissionsByPeriod
        //     .Where(s => s.SubmittedBy.UserTitle.TitleName.Equals(groupName, StringComparison.OrdinalIgnoreCase))
        //     .ToList();

        // 0. filter user by group
        // 1. loop users
        var resultDataList = userList
            .Where(user => user.UserGroup.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase))
            .Select(user =>
            {
                // 2. filter submissions by user
                var submisionByUser = allSubmissionsByPeriod
                    .Where(s => s.SubmitterId == user.Id)
                    .OrderBy(s => s.DepartmentKeyMetric.KeyIssueDepartment.DepartmentName)
                    .ToList() ?? [];
                // Loop department list to include non submitted departments
                // 3. loop department list
                var departmentScoreSummary = departmentList
                    .Select(department =>
                    {
                        var submission = submisionByUser
                            .Where(submission => submission.DepartmentKeyMetric.KeyIssueDepartmentId == department.Id)
                            .FirstOrDefault();
                        return new KeyKpi_DepartmentScoreSummary
                        {
                            DepartmentName = department.DepartmentName,
                            // TotalKey = submission != null ? submission.KeyKpiSubmissionItems.Count : 0,
                            // TotalScore = submission != null ? submission.KeyKpiSubmissionItems.Sum(i => i.ScoreValue) : 0
                            TotalKey = -1,
                            TotalScore = -1
                        };
                    })
                    .OrderBy(department => department.DepartmentName)
                    .ToList();

                return new KeyKpi_SingleUserGroup_ReportDetailViewModel
                {
                    PeriodName = SelectedPeriod.PeriodName,
                    SubmittedBy = user,
                    KeyKpi_DepartmentScoreSummary = departmentScoreSummary
                };
            }).ToList();

        return resultDataList;
    }

}