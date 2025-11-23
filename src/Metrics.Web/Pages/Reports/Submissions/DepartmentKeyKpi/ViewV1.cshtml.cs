using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Metrics.Web.Models.DepartmentKeyMetric;
using Metrics.Web.Models.KeyKpiSubmissionConstraint;
using Metrics.Web.Models.ReportViewModels.KeyKpi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentKeyKpi;

public class ViewV1Model(
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
    public List<KeyKpi_AllUserGroup_ReportSummaryViewModel> AllUserGroup_SummaryList { get; set; } = [];
    public List<KeyKpi_AllUserGroup_ReportDetailViewModel> AllUserGroup_DetailList { get; set; } = [];
    public List<KeyKpi_SingleUserGroup_ReportSummaryViewModel> SingleUserGroup_SummaryList { get; set; } = [];
    public List<KeyKpi_SingleUserGroup_ReportDetailViewModel> SingleUserGroup_DetailList { get; set; } = [];

    public List<UserViewModel> SubmitterList { get; set; } = [];
    public List<KeyKpiSubmissionViewModel> KeyKpiSubmissions { get; set; } = [];
    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];
    public List<KeyKpiSubmissionConstraintViewModel> SubmissionConstraints { get; set; } = [];
    public KpiPeriodViewModel SelectedPeriod { get; set; } = new();

    public List<UserViewModel> EligibleSubmitters { get; set; } = [];
    public List<DepartmentViewModel> EligibleDepartments { get; set; } = [];




    // ----------Excel Models----------
    // public class KeyKpiSubmissionExportViewModel
    // {
    // }

    // public List<KeyKpiSubmissionExportViewModel>


    public string SelectedPeriodName { get; set; } = null!;

    // public string? Submitter { get; set; }

    public class DepartmentViewModel2
    {
        public long Id { get; set; }
        public Guid DepartmentCode { get; set; }
        public required string DepartmentName { get; set; }
        public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];
    }
    public List<DepartmentViewModel2> KeyIssueDepartmentList2 { get; set; } = []; // TODO: Experimental
    public List<DepartmentViewModel> KeyIssueDepartmentList { get; set; } = [];
    public List<UserGroupViewModel> UserGroupList { get; set; } = [];

    // ----------Select/Options Data----------
    [BindProperty]
    public List<SelectListItem> UserGroupListItems { get; set; } = []; // for select element

    [BindProperty(SupportsGet = true)]
    public string? Group { get; set; } // selected item (for filter select element)

    [BindProperty]
    public List<SelectListItem> ReportViewModeListItems { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? ViewMode { get; set; } // selected item (for filter select element)

    // =========================================================================
    // ========== HANDLERS =====================================================
    // =========================================================================
    // TODO: remove redundent string for maintainbility
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

        // ----------VIEW MODE--------------------------------------------------
        ReportViewModeListItems = new List<SelectListItem>
        {
            // new() { Value = "summary", Text = "Summary" },
            new() { Value = "detail", Text = "Detail" }
        };
        // Select First View Mode
        if (string.IsNullOrEmpty(ViewMode)) { ViewMode = ReportViewModeListItems[0].Value.ToLower(); }

        // ----------USER GROUPS------------------------------------------------
        UserGroupList = await LoadUserGroups();
        if (UserGroupList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "User Group is empty");
            return Page();
        }
        UserGroupListItems = LoadUserGroupListItems(UserGroupList);
        // Select All group
        // if (string.IsNullOrEmpty(Group)) { Group = "all"; }
        // Select First Group
        if (string.IsNullOrEmpty(Group)) { Group = UserGroupListItems[0].Value.ToLower(); }

        /* Option 1: All Departmants
        // var departments = await _departmentService.FindAllAsync();
        // if (!departments.Any())
        // {
        //     ModelState.AddModelError(string.Empty, "Department does not exist.");
        //     return Page();
        // }
        // DepartmentList = departments.Select(d => new DepartmentViewModel
        // {
        //     Id = d.Id,
        //     DepartmentName = d.DepartmentName,
        //     DepartmentCode = d.DepartmentCode
        // }).ToList();
        */

        // Option 2: Department having Key
        // ...

        // ----------DEPARTMENT KEY METRICS-------------------------------------
        DepartmentKeyMetrics = await LoadDepartmentKeyMetrics(SelectedPeriod.Id);
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
        // all users
        var submitters = await LoadUserList(roleName: "staff"); // all staff
        // users in Departments eligible to score
        var submitterDepartmentIDs = SubmissionConstraints.Select(c => c.SubmitterDepartmentId).ToList();
        EligibleSubmitters = submitters
            .Where(submitter => submitterDepartmentIDs.Contains(submitter.DepartmentId))
            .ToList();
        SubmitterList = EligibleSubmitters;
        // SubmitterList = submitters;


        // find eligible department, get eligible users
        // Eligible Key Issue Departments
        var keyIssueDepartmentIDs = DepartmentKeyMetrics.Select(e => e.KeyIssueDepartmentId).ToList();
        KeyIssueDepartmentList = KeyIssueDepartmentList.Where(e => keyIssueDepartmentIDs.Contains(e.Id)).ToList();
        // EligibleDepartments = SubmissionConstraints
        //     .Select(c => c.SubmitterDepartment)
        //     .ToList();


        // ==========VIEW MODELS FOR DISPLAY========================
        // ALL    + SUMMARY = AllUserGroupReportSummaryList :: AllUserGroupReportSummaryViewModel
        // ALL    + DETAIL  = AllUserGroupReportDetailList :: AllUserGroupReportDetailViewModel
        // SINGLE + SUMMARY = SingleUserGroupReportSummaryViewModel
        // SINGLE + DETAIL  = SingleUserGroupReportDetailViewModel
        // =========================================================
        var MODE_SUMMARY = ViewMode.Equals("summary", StringComparison.OrdinalIgnoreCase);
        var MODE_DETAIL = ViewMode.Equals("detail", StringComparison.OrdinalIgnoreCase);
        var GROUP_ALL = Group.Equals("all", StringComparison.OrdinalIgnoreCase);

        if (MODE_SUMMARY)
        {

            if (GROUP_ALL) // SUMMARY, ALL GROUP
            {
                // load existing submissions
                // AllUserGroup_SummaryList = Load_AllUserGroup_SummaryList(KeyIssueDepartmentList, submissions);
            }
            else // SUMMARY SINGLE GROUP
            {
                // load existing submissions

                AllUserGroup_SummaryList = [];
                // ...
            }
        }
        else if (MODE_DETAIL) // or else
        {

            if (GROUP_ALL) // DETAIL ALL GROUP
            {
                // load existing submissions
                var submissions = await _keyKpiSubmissionService.FindByPeriodAsync(SelectedPeriod.Id);
                if (!submissions.IsSuccess || submissions.Data == null)
                {
                    ModelState.AddModelError(string.Empty, "Failed fetching submissions. Please try again.");
                    return Page();
                }
                var submissionsByPeriod = submissions.Data.Select(s => s.MapToViewModel()).ToList();

                KeyKpi_AllUserGroup_ReportDetails = Load_AllUserGroup_DetailList(
                    allSubmissionsByPeriod: submissionsByPeriod,
                    submitterList: SubmitterList,
                    keyIssueDepartmentList: KeyIssueDepartmentList);
            }
            else // DETAIL SINGLE GROUP
            {
                var submissions = await _keyKpiSubmissionService.FindByPeriodAsync(SelectedPeriod.Id);
                if (!submissions.IsSuccess || submissions.Data == null)
                {
                    ModelState.AddModelError(string.Empty, "Failed fetching submissions. Please try again.");
                    return Page();
                }
                var submissionsByPeriod = submissions.Data.Select(s => s.MapToViewModel()).ToList();


                // TODO: fix method parameters
                SingleUserGroup_DetailList = Load_SingleUserGroup_DetailList(
                    allSubmissionsByPeriod: submissionsByPeriod,
                    userList: SubmitterList,
                    departmentList: KeyIssueDepartmentList,
                    groupName: Group
                );
            }
        }

        return Page();
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
                .Where(g => !g.TitleName.Equals("staff", StringComparison.OrdinalIgnoreCase))
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


    // private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeyMetrics(
    //     string periodName,
    //     Guid departmentCode)
    // {
    //     var dkms = await _departmentKeyMetricService
    //         .FindAllByPeriodAndDepartmentAsync(periodName, departmentCode);

    //     return dkms.Select(dkm => MapEntityToViewModel(dkm)).ToList();
    // }

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




    public List<KeyKpi_AllUserGroup_ReportDetail_ViewModel2> KeyKpi_AllUserGroup_ReportDetails { get; set; } = [];
    public class KeyKpi_AllUserGroup_ReportDetail_ViewModel2
    {
        public string? PeriodName { get; set; }
        public UserViewModel SubmittedBy { get; set; } = null!;
        public List<KeyKpi_AllUserGroup_ReportDetailItem_ViewModel2> KeyKpi_AllUserGroup_ReportDetailItem { get; set; } = [];
    }

    public class KeyKpi_AllUserGroup_ReportDetailItem_ViewModel2
    {
        public DepartmentKeyMetricViewModel? DepartmentKeyMetric { get; set; }
        public decimal ScoreValue { get; set; }
        public string? Comments { get; set; } = string.Empty;
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
    private List<KeyKpi_AllUserGroup_ReportDetail_ViewModel2> Load_AllUserGroup_DetailList(
        List<KeyKpiSubmissionViewModel> allSubmissionsByPeriod,
        List<UserViewModel> submitterList, // eligible users
        List<DepartmentViewModel> keyIssueDepartmentList) // departments who issue keys
    {
        // NOTE: need to fetch for all submitter (including not submitted user's submission that score will be set to 0.00)
        // 1. need all Submitters
        // 2. need all Keys of Departments

        // // find eligible department, get eligible users
        // var dkmIds = DepartmentKeyMetrics.Select(x => x.Id).ToList();
        // var eligibleDepartments = submissionConstrains.Select(c => c.SubmitterDepartmentId).ToList();
        // var eligibleUsers = submitterList.Where(submitter => eligibleDepartments.Contains(submitter.DepartmentId)).ToList();

        // each Submitter -> List<departments> -> each department -> List<DKM> -> each DKM -> List<MODLE>
        List<KeyKpi_AllUserGroup_ReportDetail_ViewModel2> data = submitterList
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
                        return new KeyKpi_AllUserGroup_ReportDetailItem_ViewModel2
                        {
                            DepartmentKeyMetric = submission.DepartmentKeyMetric,
                            ScoreValue = submission.ScoreValue,
                            Comments = submission.Comments
                        };
                    }
                    else
                    {
                        return new KeyKpi_AllUserGroup_ReportDetailItem_ViewModel2
                        {
                            DepartmentKeyMetric = null,
                            ScoreValue = 0.00M,
                            Comments = string.Empty
                        };
                    }
                }).ToList();
            }).ToList();

            return new KeyKpi_AllUserGroup_ReportDetail_ViewModel2
            {
                PeriodName = SelectedPeriod.PeriodName,
                SubmittedBy = submitter,
                KeyKpi_AllUserGroup_ReportDetailItem = items
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

    private static DepartmentKeyMetricViewModel MapEntityToViewModel(DepartmentKeyMetric e)
    {
        return new DepartmentKeyMetricViewModel
        {
            Id = e.Id,
            LookupId = e.DepartmentKeyMetricCode,
            SubmissionPeriodId = e.KpiSubmissionPeriodId,
            KeyIssueDepartmentId = e.DepartmentId,
            KeyIssueDepartment = new DepartmentViewModel
            {
                Id = e.KeyIssueDepartment.Id,
                DepartmentCode = e.KeyIssueDepartment.DepartmentCode,
                DepartmentName = e.KeyIssueDepartment.DepartmentName
            },
            KeyMetricId = e.KeyMetricId,
            KeyMetric = new KeyMetricViewModel
            {
                Id = e.KeyMetric.Id,
                LookupId = e.KeyMetric.MetricCode,
                MetricTitle = e.KeyMetric.MetricTitle,
                Description = e.KeyMetric.Description,
                IsDeleted = e.KeyMetric.IsDeleted
            },
            IsDeleted = e.IsDeleted
        };
    }


}


// DepartmentKeyMetric = new DepartmentKeyMetricViewModel
// {
//     Id = item.DepartmentKeyMetric.Id,
//     LookupId = item.DepartmentKeyMetric.DepartmentKeyMetricCode,
//     // KpiSubmissionPeriodId = item.DepartmentKeyMetric.KpiSubmissionPeriodId,
//     DepartmentId = item.DepartmentKeyMetric.DepartmentId,
//     KeyIssuer = new DepartmentViewModel
//     {
//         Id = item.DepartmentKeyMetric.KeyIssueDepartment.Id,
//         DepartmentCode = item.DepartmentKeyMetric.KeyIssueDepartment.DepartmentCode,
//         DepartmentName = item.DepartmentKeyMetric.KeyIssueDepartment.DepartmentName
//     },
//     KeyMetric = new KeyMetricViewModel
//     {
//         Id = item.DepartmentKeyMetric.KeyMetric.Id,
//         LookupId = item.DepartmentKeyMetric.KeyMetric.MetricCode,
//         MetricTitle = item.DepartmentKeyMetric.KeyMetric.MetricTitle,
//         Description = item.DepartmentKeyMetric.KeyMetric.Description,
//     },
//     KeyMetricId = item.DepartmentKeyMetric.KeyMetricId,
// },

/*
 // TODO: entity to viewmodel
var keyKpiSubmissions = allSubmissionsByPeriod
        .Select(s => new KeyKpiSubmissionViewModel
        {
            Id = s.Id,
            SubmittedAt = s.SubmittedAt,
            KpiPeriodId = s.ScoreSubmissionPeriodId,
            TargetKpiPeriod = new KpiPeriodViewModel
            {
                Id = s.TargetPeriod.Id,
                PeriodName = s.TargetPeriod.PeriodName,
                SubmissionStartDate = s.TargetPeriod.SubmissionStartDate,
                SubmissionEndDate = s.TargetPeriod.SubmissionEndDate
            },
            DepartmentId = s.DepartmentId,
            TargetDepartment = new DepartmentViewModel
            {
                Id = s.TargetDepartment.Id,
                DepartmentCode = s.TargetDepartment.DepartmentCode,
                DepartmentName = s.TargetDepartment.DepartmentName
            },
            SubmitterId = s.ApplicationUserId,
            SubmittedBy = new UserViewModel
            {
                Id = s.SubmittedBy.Id,
                UserCode = s.SubmittedBy.UserCode,
                UserName = s.SubmittedBy.UserName!,
                FullName = s.SubmittedBy.FullName,
                PhoneNumber = s.SubmittedBy.PhoneNumber,
                UserGroup = new UserGroupViewModel
                {
                    Id = s.SubmittedBy.UserTitle.Id,
                    GroupCode = s.SubmittedBy.UserTitle.TitleCode,
                    GroupName = s.SubmittedBy.UserTitle.TitleName,
                    Description = s.SubmittedBy.UserTitle.Description
                }
            },
            KeyKpiSubmissionItems = s.KeyKpiSubmissionItems
                    .Select(i => new KeyKpiSubmissionItemViewModel
                    {
                        Id = i.Id,
                        KeyKpiSubmissionId = i.KeyKpiSubmissionId,
                        DepartmentKeyMetricId = i.DepartmentKeyMetricId,
                        DepartmentKeyMetric = new DepartmentKeyMetricViewModel
                        {
                            Id = i.DepartmentKeyMetric.Id,
                            LookupId = i.DepartmentKeyMetric.DepartmentKeyMetricCode,
                            KpiSubmissionPeriodId = i.DepartmentKeyMetric.KpiSubmissionPeriodId,
                            DepartmentId = i.DepartmentKeyMetric.DepartmentId,
                            KeyIssuer = new DepartmentViewModel
                            {
                                Id = i.DepartmentKeyMetric.TargetDepartment.Id,
                                DepartmentCode = i.DepartmentKeyMetric.TargetDepartment.DepartmentCode,
                                DepartmentName = i.DepartmentKeyMetric.TargetDepartment.DepartmentName
                            },
                            KeyMetricId = i.DepartmentKeyMetric.KeyMetricId,
                            KeyMetric = new KeyMetricViewModel
                            {
                                Id = i.DepartmentKeyMetric.KeyMetric.Id,
                                LookupId = i.DepartmentKeyMetric.KeyMetric.MetricCode,
                                MetricTitle = i.DepartmentKeyMetric.KeyMetric.MetricTitle,
                                Description = i.DepartmentKeyMetric.KeyMetric.Description,
                                IsDeleted = i.DepartmentKeyMetric.KeyMetric.IsDeleted
                            },
                            IsDeleted = i.DepartmentKeyMetric.IsDeleted
                        },
                        ScoreValue = i.ScoreValue,
                        Comments = i.Comments
                    }).ToList()
        }).ToList();
*/