using Metrics.Application.Common.Mappers;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Metrics.Web.Models.DepartmentKeyMetric;
using Metrics.Web.Models.KeyKpiSubmissionConstraint;
using Metrics.Web.Models.KeyMetric;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;

namespace Metrics.Web.Pages.Reports.Submissions.KeyKpi;

public class ViewModel(
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

        // ----------USER GROUPS------------------------------------------------
        UserGroupList = await LoadUserGroups();
        if (UserGroupList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "No Candidate Group.");
            return Page();
        }
        UserGroupSelectItems = LoadUserGroupListItems(UserGroupList);
        if (string.IsNullOrEmpty(Group))
            Group = UserGroupSelectItems[0].Value.ToLower();

        // ---------CANDIDATE DEPARTMENT------------------------------------------------
        CandidateDepartmentList = await LoadCandidateDepartmentFromConstraints(SelectedPeriod.Id);
        if (CandidateDepartmentList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "No Candidate Department.");
            return Page();
        }
        CandidateDepartmentSelectItems = LoadCandidateDepartmentListItems(CandidateDepartmentList);
        if (string.IsNullOrEmpty(CandidateDepartment))
        {
            CandidateDepartment = CandidateDepartmentSelectItems[0].Value;
            SelectedCandidateDepartment = null;
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
            SelectedKeyDepartment = null;
        }
        var KEY_DEPARTMENT__ALL = KeyDepartment.Equals("all", StringComparison.OrdinalIgnoreCase);

        // ----------SUBMISSION CONSTRAINTS-------------------------------------
        // SubmissionConstraints = await LoadSubmissionConstraintByPeriod(SelectedPeriod.Id);
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


        // ----------FILTER BY KEY ISSUE DEPARTMENT------------------------------
        if (KEY_DEPARTMENT__ALL)
        {
            // get submission of each key department 
            foreach (var keyDpt in KeyIssueDepartmentList)
            {
                var submissionsPerKeyDepartment = allSubmissions.Data
                   .Where(s => s.DepartmentKeyMetric.KeyIssueDepartmentId == keyDpt.Id)
                   .ToList();
                var constraintsPerKeyDepartment = allSubmissionConstraints
                    .Where(c => c.DepartmentKeyMetric.KeyIssueDepartmentId == keyDpt.Id)
                    .ToList();

                // BASED ON CONSTRAINTS DEFINED
                var keyMetrics = constraintsPerKeyDepartment
                    .DistinctBy(c => c.DepartmentKeyMetric.KeyMetricId)
                    .OrderBy(s => s.DepartmentKeyMetric.KeyMetric.MetricTitle)
                    .Select(c => c.DepartmentKeyMetric.KeyMetric)
                    .ToList();

                foreach (var keyMetric in keyMetrics)
                {
                    //----------EXPECTED SUBMISSIONS-----------------------------
                    // **Not working
                    /*
                    var constraintsOnKey = constraintsPerKeyDepartment
                        .Where(c => c.DepartmentKeyMetric.KeyMetricId == keyMetric.Id)
                        .ToList();
                    var totalUsersInDepartment = 0L;
                    foreach (var c in constraintsOnKey)
                    {
                        var userCount = 0;
                        var users = await _userService.FindByDepartmentAsync(c.CandidateDepartment.Id);
                        if (users.IsSuccess && users.Data != null)
                        {
                            userCount = users.Data
                                .Where(u => !u.UserGroup.GroupName.Equals("staff", StringComparison.OrdinalIgnoreCase))
                                .Count();
                        }

                        totalUsersInDepartment += userCount; // users to submit key1 of departmentx
                    }
                    var expectedSubmissionOnKey = totalUsersInDepartment;//constraintsOnKey.Count; 
                    // expected submissions = number of user in department expected to submit score  
                    */

                    //------------------------------------------------------------
                    var submissionsOnKey = submissionsPerKeyDepartment
                        .Where(s => s.DepartmentKeyMetric.KeyMetricId == keyMetric.Id)
                        .ToList();
                    var scoreTotalPerKey = submissionsOnKey.Sum(s => s.ScoreValue);
                    var submissionCountPerKey = submissionsOnKey.Count;

                    var summaryReport = new SummaryReport_ViewModel
                    {
                        KpiPeriodName = SelectedPeriodName,
                        KeyIssueDepartment = keyDpt,
                        KeyMetric = keyMetric,
                        //ExpectedSubmission = -1, // expectedSubmissionOnKey, // not working
                        SubmissionReceived = submissionCountPerKey, // submission count on each key
                        ReceivedScore = scoreTotalPerKey,
                    };
                    SummaryReportList.Add(summaryReport);
                }

                // BASED ON SUBMITTED DATA
                /*
                var submissionsPerKeyDepartment = allSubmissions.Data
                    .Where(s => s.DepartmentKeyMetric.KeyIssueDepartmentId == keyDpt.Id)
                    .ToList();
                // total score on key department keys // var totalScorePerKeyDepartment = submissionsPerKeyDepartment.Sum(s=>s.ScoreValue);
                // filter total score on each key (single) -> extract keys from submissionsPerKeyDepartment
                var keyMetrics = submissionsPerKeyDepartment
                    .DistinctBy(s => s.DepartmentKeyMetric.KeyMetricId)
                    .OrderBy(s => s.DepartmentKeyMetric.KeyMetric.MetricTitle)
                    .Select(s => s.DepartmentKeyMetric.KeyMetric)
                    .ToList();
                // get total score on each key
                foreach (var keyMetric in keyMetrics)
                {
                    // score total of each key
                    var scoreTotalPerKey = submissionsPerKeyDepartment
                        .Where(s => s.DepartmentKeyMetric.KeyMetricId == keyMetric.Id)
                        .Sum(s => s.ScoreValue);

                    var dataRow = new SummaryReport_ViewModel
                    {
                        KpiPeriodName = SelectedPeriodName,
                        KeyIssueDepartment = keyDpt,
                        KeyMetric = keyMetric.MapToViewModel(),
                        TotalExpectedSubmission = 0,
                        TotalSubmissionsReceived = 0,
                        TotalReceivedScore = scoreTotalPerKey,
                    };
                    SummaryReportList.Add(dataRow);
                }
                */

            }
        }
        else // SINGLE KEY DEPARTMENT
        {
            // -----Parse String to GUID for Selected Department-----
            if (Guid.TryParse(KeyDepartment, out Guid departmentCode))
                SelectedKeyDepartment = KeyIssueDepartmentList
                    .FirstOrDefault(d => d.DepartmentCode == departmentCode);
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid Department Code.");
                return Page();
            }

            // ------------------------------------------------------
            if (SelectedKeyDepartment != null)
            {
                var submissionsPerKeyDepartment = allSubmissions.Data
                        .Where(s => s.DepartmentKeyMetric.KeyIssueDepartment.DepartmentCode == SelectedKeyDepartment.DepartmentCode)
                        .ToList();
                var constraintsPerKeyDepartment = allSubmissionConstraints
                    .Where(c => c.DepartmentKeyMetric.KeyIssueDepartment.DepartmentCode == SelectedKeyDepartment.DepartmentCode)
                    .ToList();

                // // BASED ON CONSTRAINTS DEFINED
                // var keyMetricsAvaiable = constraintsPerKeyDepartment
                //     .DistinctBy(s => s.DepartmentKeyMetric.KeyMetricId)
                //     .OrderBy(s => s.DepartmentKeyMetric.KeyMetric.MetricTitle)
                //     .Select(s => s.DepartmentKeyMetric.KeyMetric)
                //     .ToList();
                // foreach(var keyMetric in keyMetricsAvaiable)
                // {
                //     var submissionsOnKey = submissionsPerKeyDepartment
                //         .Where(s => s.DepartmentKeyMetric.KeyMetricId == keyMetric.Id)
                //         .ToList();
                //     var scoreTotalPerKey = submissionsOnKey.Sum(s => s.ScoreValue);
                //     var submissionCountPerKey = submissionsOnKey.Count;

                //     var summaryReport = new SummaryReport_ViewModel
                //     {
                //         KpiPeriodName = SelectedPeriodName,
                //         KeyIssueDepartment = SelectedKeyDepartment,
                //         KeyMetric = keyMetric,
                //         //ExpectedSubmission = -1, // expectedSubmissionOnKey, // not working
                //         SubmissionReceived = submissionCountPerKey, // submission count on each key
                //         ReceivedScore = scoreTotalPerKey,
                //     };
                //     SummaryReportList.Add(summaryReport);
                // }

                // BASED ON SUBMISSIONS RECEIVED
                var keyMetrics = submissionsPerKeyDepartment
                    .DistinctBy(s => s.DepartmentKeyMetric.KeyMetricId)
                    .OrderBy(s => s.DepartmentKeyMetric.KeyMetric.MetricTitle)
                    .Select(s => s.DepartmentKeyMetric.KeyMetric)
                    .ToList();
                foreach (var keyMetric in keyMetrics) // keys
                {
                    // score total of each key
                    var submissionsOnKey = submissionsPerKeyDepartment
                        .Where(s => s.DepartmentKeyMetric.KeyMetricId == keyMetric.Id)
                        .ToList();
                    var scoreTotalPerKey = submissionsOnKey.Sum(s => s.ScoreValue);
                    var submissionCountPerKey = submissionsOnKey.Count;

                    var summaryReportRow = new SummaryReport_ViewModel
                    {
                        KpiPeriodName = SelectedPeriodName,
                        KeyIssueDepartment = SelectedKeyDepartment,
                        KeyMetric = keyMetric.MapToViewModel(),
                        //ExpectedSubmission = -1, //count
                        SubmissionReceived = submissionCountPerKey, //count
                        ReceivedScore = scoreTotalPerKey //sum
                    };
                    SummaryReportList.Add(summaryReportRow);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unknown selected key issue department.");
            }
        }







        /*
      
        foreach (var keyDepartment in KeyIssueDepartmentList)
        {
            // for each KeyDepartment -> get its DKMs
            var dkms = await _departmentKeyMetricService.FindByPeriodAndDepartmentAsync(
                SelectedPeriod.Id,
                keyDepartment.Id);
            if (!dkms.IsSuccess || dkms.Data == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to fetch Department keys by key issue department: " + keyDepartment.DepartmentName);
                return Page();
            }
            var keyDepartmentDKMs = dkms.Data
                .OrderBy(dkm => dkm.KeyMetric.MetricTitle)
                .Select(dkm => dkm.MapToViewModel())
                .ToList();

            // get key list
            var keysOfDepartment = keyDepartmentDKMs
                .Select(dkm => dkm.KeyMetric)
                .ToList();
            // for each key -> find submission
            // OR
            // get submissions by DKMs
            var dkmIDs = keyDepartmentDKMs.Select(x => x.Id).ToList();
            var submissionOnKeyDepartment = await _keyKpiSubmissionService.FindByDepartmentKeyMetricsAsync(dkmIDs);
            // get total score from submissions containing dkm of key issue department
            if (!submissionOnKeyDepartment.IsSuccess || submissionOnKeyDepartment.Data == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to fetch submissions.");
                return Page();
            }

            foreach (var key in keysOfDepartment)
            {

                var r = submissionOnKeyDepartment.Data
                    .Where(s => s.DepartmentKeyMetric.KeyMetricId == key.Id)
                    .FirstOrDefault();
                if (r != null)
                {
                    SummaryReportList.Add(new SummaryReport_ViewModel
                    {
                        KpiPeriodName = SelectedPeriodName,
                        KeyIssueDepartment = r.DepartmentKeyMetric.KeyIssueDepartment.MapToViewModel(),
                        KeyMetric = r.DepartmentKeyMetric.KeyIssueDepartment.MapToViewModel(),
                        TotalScore =
                    });

                }

            }
        }

        // Load Submissions
        SummaryReportList = Load_SummaryReport_List(
            allSubmissionsByPeriod: allSubmissions,
            submitterList: SubmitterList,
            keyIssueDepartmentList: KeyIssueDepartmentList,
            keyIssueDepartmentList2: KeyIssueDepartmentList2);
        */
        return Page();
    }



    // private List<> Load_SummaryReport_List(
    //     List<DepartmentViewModel> DepartmentList,
    //     List<KeyKpiSubmissionViewModel> submissionsByPeriod_ByAllUserGroup,
    //     bool IsAllGroup,
    //     string? groupName)
    // {
    //     throw new NotImplementedException();
    // }

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

    // private async Task<List<KeyKpiSubmissionConstraintViewModel>> LoadSubmissionConstraintByKeyIssueDepartment(long id)
    // {
    //     // await _submissionConstraintService.findby
    // }

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
        UserGroupSelectItems = LoadUserGroupListItems(UserGroupList);
        if (string.IsNullOrEmpty(Group)) { Group = UserGroupSelectItems[0].Value; }

        // ----------DEPARTMENT KEY METRICS-------------------------------------
        DepartmentKeyMetrics = await LoadDepartmentKeyMetricByPeriod(SelectedPeriod.Id); // key departments + keys

        // ----------Key Issue Departments List (Dinstinct)---------------------
        KeyIssueDepartmentList = DepartmentKeyMetrics
            .DistinctBy(x => x.KeyIssueDepartmentId)
            .OrderBy(x => x.KeyIssueDepartment.DepartmentName)
            .Select(x => x.KeyIssueDepartment)
            .ToList();

        // ----------SUBMISSION CONSTRAINTS-------------------------------------
        var dkmIDs = DepartmentKeyMetrics.Select(x => x.Id).ToList();
        SubmissionConstraints = await LoadSubmissionConstraintsByDkmIDs(dkmIDs);

        // ----------SUBMITTER DEPARTMENTS--------------------------------------
        var submitterDepartments = SubmissionConstraints
            .DistinctBy(c => c.SubmitterDepartmentId)
            .Select(c => c.CandidateDepartment).ToList();
        if (submitterDepartments.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Submitter department is empty");
            return Page();
        }
        CandidateDepartmentSelectItems = LoadSubmitterDepartmentListItems(submitterDepartments);
        if (string.IsNullOrEmpty(CandidateDepartment)) { CandidateDepartment = CandidateDepartmentSelectItems[0].Value; }

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
        var ALL_DEPARTMENT = CandidateDepartment.Equals("all", StringComparison.OrdinalIgnoreCase);
        if (!ALL_DEPARTMENT)  // Filter submissions by single submitter department
        {
            submissions = submissions
                .Where(s => s.SubmittedBy.Department.DepartmentName.Equals(CandidateDepartment, StringComparison.OrdinalIgnoreCase))
                .ToList();
            SubmitterList = SubmitterList
                .Where(submitter => submitter.Department.DepartmentName.Equals(CandidateDepartment, StringComparison.OrdinalIgnoreCase))
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
        // KeyKpi_DetailReportList = Load_DetailReportList(
        //     allSubmissionsByPeriod: submissions,
        //     submitterList: SubmitterList,
        //     keyIssueDepartmentList: KeyIssueDepartmentList,
        //     keyIssueDepartmentList2: KeyIssueDepartmentList2);


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

    private async Task<List<UserGroupViewModel>> LoadUserGroups()
    {
        // TODO: FindAll_Async method with accepting list of excluded username parameter.
        // var userGroups = await _userGroupService.FindAll_Async(["sysadmin", "staff"]);
        var userGroups = await _userGroupService.FindAll_Async();
        if (userGroups.IsSuccess && userGroups.Data != null)
        {
            return userGroups.Data
                .Where(g =>
                    !g.GroupName.Equals("sysadmin", StringComparison.OrdinalIgnoreCase) &&
                    !g.GroupName.Equals("staff", StringComparison.OrdinalIgnoreCase))
                .OrderBy(g => g.GroupName)
                .Select(g => g.MapToViewModel())
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


    //----------LOADING LIST ITEMS-----------------------------------------------
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

    private List<SelectListItem> LoadCandidateDepartmentListItems(List<DepartmentViewModel> candidateDepartmentList)
    {
        if (candidateDepartmentList.Count > 0)
        {
            // add All item before user group items
            var items = new List<SelectListItem>()
            {
                new() { Value = "all", Text = "All" }
            };

            foreach (var department in candidateDepartmentList)
            {
                items.Add(new SelectListItem
                {
                    Value = department.DepartmentCode.ToString(),
                    Text = department.DepartmentName,
                });
            }
            return items;
        }
        ModelState.AddModelError(string.Empty, "Candidate Department does not exist.");
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
                    Value = group.GroupCode.ToString(),
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
                    && !user.UserTitle.TitleName.Equals("staff", StringComparison.OrdinalIgnoreCase)
                    && !user.UserTitle.TitleName.Equals("management", StringComparison.OrdinalIgnoreCase))
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

    /// <summary>
    /// Load Department Key Metrics by Period
    /// </summary>
    /// <param name="periodId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Load Submission Constraints by Department Keys (Metrics)
    /// </summary>
    /// <param name="dkmIDs"></param>
    /// <returns></returns>
    private async Task<List<KeyKpiSubmissionConstraintViewModel>> LoadSubmissionConstraintsByDkmIDs(List<long> dkmIDs)
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


    // ========== MODELS ========================================================
    // ----------SELECT items----------------------------------------------------
    [BindProperty]
    public List<SelectListItem> KeyIssueDepartmentSelectItems { get; set; } = [];
    [BindProperty]
    public List<SelectListItem> UserGroupSelectItems { get; set; } = [];
    [BindProperty]
    public List<SelectListItem> CandidateDepartmentSelectItems { get; set; } = [];
    // ----------SELECTION VALUE--------------------------
    // **Property name will display in Query String
    [BindProperty(SupportsGet = true)]
    public required string KeyDepartment { get; set; }
    [BindProperty(SupportsGet = true)]
    public required string CandidateDepartment { get; set; }
    [BindProperty(SupportsGet = true)]
    public required string Group { get; set; }
    // --------------------------------------------------------------------------
    public string SelectedPeriodName { get; set; } = null!;
    public KpiPeriodViewModel SelectedPeriod { get; set; } = new();
    // -----------KEY DEPARTMENT & CANDIDATE DEPARTMENT--------------------------
    public List<DepartmentViewModel> KeyIssueDepartmentList { get; set; } = []; // key department list (distinct)
    public DepartmentViewModel? SelectedKeyDepartment { get; set; } = null;
    public List<DepartmentViewModel> CandidateDepartmentList { get; set; } = []; // candidate department list (distinct)
    public DepartmentViewModel? SelectedCandidateDepartment { get; set; } = null;



    public List<DepartmentKeyAssignmentViewModel> DepartmentKeyAssignments { get; set; } = [];
    public List<KeyKpiSubmissionConstraintViewModel> SubmissionConstraints { get; set; } = [];
    public List<SummaryReport_ViewModel> SummaryReportList { get; set; } = [];
    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];



    // [BindProperty]
    // public List<SelectListItem> ReportViewModeListItems { get; set; } = [];

    // [BindProperty(SupportsGet = true)]
    // public string? ViewMode { get; set; } // selected item (for filter select element)
    public List<UserGroupViewModel> UserGroupList { get; set; } = [];

    // -------------------------------------------------------------------------
    public List<KeyIssueDepartmentViewModel> KeyIssueDepartmentList2 { get; set; } = [];
    public List<UserViewModel> SubmitterList { get; set; } = [];
    public List<KeyKpiSubmissionViewModel> KeyKpiSubmissions { get; set; } = [];
    // -----model for Report Detail View for both All user group + single user group----
    public List<KeyKpi_ReportDetail_ViewModel> KeyKpi_DetailReportList { get; set; } = [];

    // ========== MODELS CLASSES ===============================================
    public class DepartmentKeyAssignmentViewModel
    {
        public long Id { get; set; }
        public required string KeyIssueDepartmentName { get; set; }
        public List<DepartmentKeyMetricViewModel> DepartmentKeys { get; set; } = [];
    }

    public class SummaryReport_ViewModel
    {
        public string KpiPeriodName { get; set; } = null!;
        public DepartmentViewModel KeyIssueDepartment { get; set; } = null!;
        public KeyMetricViewModel KeyMetric { get; set; } = null!;
        public long SubmissionReceived { get; set; }
        // public long ExpectedSubmission { get; set; }
        public decimal ReceivedScore { get; set; } // total score received on each department's key
    }

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

    public class KeyIssueDepartmentViewModel
    {
        public long Id { get; set; }
        public Guid DepartmentCode { get; set; }
        public required string DepartmentName { get; set; }
        public required List<DepartmentKeyMetricViewModel> DepartmentKeys { get; set; } = [];
    }

}