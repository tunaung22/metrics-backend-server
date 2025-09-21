using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Metrics.Web.Models.ReportViewModels.KeyKpi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentKeyKpi;

public class ViewModel(
    IKpiSubmissionPeriodService kpiPeriodService,
    IUserService userService,
    IUserTitleService userGroupService,
    IDepartmentService departmentService,
    IKeyKpiSubmissionService keyKpiSubmissionService,
    IDepartmentKeyMetricService departmentKeyMetricService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IUserService _userService = userService;
    private readonly IUserTitleService _userGroupService = userGroupService;
    private readonly IDepartmentService _departmentService = departmentService;
    private readonly IKeyKpiSubmissionService _keyKpiSubmissionService = keyKpiSubmissionService;
    public readonly IDepartmentKeyMetricService _departmentKeyMetricService = departmentKeyMetricService;

    // =========================================================================
    // ========== MODELS =======================================================
    // =========================================================================
    // ----------SUMMARY + ALL -------------------------------------------------
    public List<KeyKpi_AllUserGroup_ReportSummaryViewModel> AllUserGroup_SummaryList { get; set; } = [];
    // ----------DETAIL + ALL---------------------------------------------------
    public List<KeyKpi_AllUserGroup_ReportDetailViewModel> AllUserGroup_DetailList { get; set; } = [];
    // ----------SUMMARY + SINGLE-----------------------------------------------
    public List<KeyKpi_SingleUserGroup_ReportSummaryViewModel> SingleUserGroup_SummaryList { get; set; } = [];
    // ----------DETAIL + SINGLE------------------------------------------------
    public List<KeyKpi_SingleUserGroup_ReportDetailViewModel> SingleUserGroup_DetailList { get; set; } = [];

    public List<UserViewModel> UserList { get; set; } = [];

    public List<KeyKpiSubmissionViewModel> KeyKpiSubmissions { get; set; } = [];

    public KpiPeriodViewModel SelectedPeriod { get; set; } = new();

    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];
    // ----------Excel Models----------
    // public class KeyKpiSubmissionExportViewModel
    // {
    // }

    // public List<KeyKpiSubmissionExportViewModel>


    public string SelectedPeriodName { get; set; } = null!;

    // public string? Submitter { get; set; }
    public List<DepartmentViewModel> DepartmentList { get; set; } = [];
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

        // ----------PERIOD----------
        var period = await LoadKpiPeriods(periodName);
        if (period != null)
        {
            SelectedPeriod = period;
            SelectedPeriodName = period.PeriodName;
        }
        else
        {
            ModelState.AddModelError("", $"Period {periodName} not found.");
            return Page();
        }

        // ----------VIEW MODE----------
        ReportViewModeListItems = new List<SelectListItem>
        {
            new() { Value = "summary", Text = "Summary" },
            new() { Value = "detail", Text = "Detail" }
        };
        if (string.IsNullOrEmpty(ViewMode))
        {
            ViewMode = ReportViewModeListItems[0].Value.ToLower();
        }

        // ----------USER GROUPS----------
        UserGroupList = await LoadUserGroups();
        if (UserGroupList.Count != 0)
        {
            UserGroupListItems = LoadUserGroupListItems(UserGroupList);
            if (string.IsNullOrEmpty(Group))
            {
                Group = "all";
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, "User Group is empty");
            return Page();
        }


        // View Mode (depends on selected group)
        // all      -> summary
        //          -> detail
        // [group]  -> summary
        //          -> detail
        // 1. Summary
        //      period | department | submissions by [group] | scores by [group] | total submissions | total score | final kpi
        // 2. Details
        //      period | submitter | date | department | key | score | comments

        // ----------DEPARTMENT----------
        var departments = await _departmentService.FindAllAsync();
        if (!departments.Any())
        {
            ModelState.AddModelError(string.Empty, "Department does not exist.");
            return Page();
        }

        DepartmentList = departments.Select(d => new DepartmentViewModel
        {
            Id = d.Id,
            DepartmentName = d.DepartmentName,
            DepartmentCode = d.DepartmentCode
        }).ToList();


        DepartmentKeyMetrics = await LoadDepartmentKeyMetrics(SelectedPeriodName);



        // TODO: Does retrieving all items impact performance??
        var allSubmissionsByPeriod = await _keyKpiSubmissionService
            .FindByKpiPeriodAsync(SelectedPeriod.Id);

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
            if (GROUP_ALL) // SUMMARY ALL GROUP
            {
                AllUserGroup_SummaryList = Load_AllUserGroup_SummaryList(
                    DepartmentList,
                    allSubmissionsByPeriod);
            }
            else // SUMMARY SINGLE GROUP
            {
                AllUserGroup_SummaryList = [];
                // ...
            }
        }
        else if (MODE_DETAIL) // or else
        {
            UserList = await LoadUserList(roleName: "staff");
            if (UserList.Count <= 0)
            {
                ModelState.AddModelError(string.Empty, "No users found.");
                return Page();
            }



            if (GROUP_ALL) // DETAIL ALL GROUP
            {
                AllUserGroup_DetailList = Load_AllUserGroup_DetailList(
                    allSubmissionsByPeriod: allSubmissionsByPeriod,
                    userList: UserList,
                    departmentList: DepartmentList);
            }
            else // DETAIL SINGLE GROUP
            {
                SingleUserGroup_DetailList = Load_SingleUserGroup_DetailList(
                    allSubmissionsByPeriod: allSubmissionsByPeriod,
                    userList: UserList,
                    departmentList: DepartmentList,
                    groupName: Group
                );
            }
        }



        // ----------GROUP: ALL-------------------------------------------------
        if (Group.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            // ----------MODE: SUMMARY------------------------------------------
            // ALL + SUMMARY :: (all group summary + with kpi)
            // view:    department | key | submissions | scores | KPI Score
            // excel:   period | department | submissions by [group] | scores by [group] | total submissions | total score | final kpi
            /*
                > Loop Department
                    > Loop Group
                        > Filter by Group
                            > Get Total Submissions, Scores for EACH Group
                    > Add to List (for rendering table)
            */
            if (ViewMode.Equals("summary", StringComparison.OrdinalIgnoreCase))
            {


            }

            // ----------MODE: DETAIL-------------------------------------------
            // ALL + DETAIL :: (all group detail + with kpi)
            // view:    department | scores | submitter | comments
            // excel:   period | department | key | score | submitter | group
            else if (ViewMode.Equals("detail", StringComparison.OrdinalIgnoreCase))
            {

            }
        }

        // ----------GROUP: SINGLE----------------------------------------------
        else
        {
            // ----------MODE: SUMMARY------------------------------------------
            if (ViewMode.Equals("summary", StringComparison.OrdinalIgnoreCase))
            {
                // view: department | submissions | scores | KPI Score
                // excel: period | department | submissions by [group] | scores by [group] | total submissions | total score | final kpi

                // prepare for Summary Table (all group)
                // single + summary

            }

            // ----------MODE: DETAIL-------------------------------------------
            else if (ViewMode.Equals("detail", StringComparison.OrdinalIgnoreCase))
            {
                // view: submitter | department | key | score | comments
                // excel: period | submitter | department | key | score | comments

                // prepare for Detail Table (all group)
                // single + detail

                // ---------- Load Users ------------------------------------
                // var users = await _userService.FindAllActiveAsync();
            }
        }


        // try
        // {
        //     var keyKpiSubmissions = await _keyKpiSubmissionService
        //         .FindByKpiPeriodAsync(SelectedPeriod.Id);
        //     if (keyKpiSubmissions.Count > 0)
        //     {
        //         // entity to viewModel
        //         // CaseFeedbackSubmissions = ToViewModels(keyKpiSubmissions);
        //     }
        // }
        // catch (Exception)
        // {
        //     ModelState.AddModelError(string.Empty, "Error fetching department case feedbacks.");
        // }

        return Page();
    }



    // ========== Methods ==================================================
    private async Task<KpiPeriodViewModel?> LoadKpiPeriods(string periodName)
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


    private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeyMetrics(
        string periodName,
        Guid departmentCode)
    {
        var dkms = await _departmentKeyMetricService
            .FindAllByPeriodAndDepartmentAsync(periodName, departmentCode);

        return dkms.Select(dkm => MapEntityToViewModel(dkm)).ToList();
    }

    private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeyMetrics(string periodName)
    {
        var dkms = await _departmentKeyMetricService
            .FindAllByPeriodNameAsync(periodName);

        return dkms.Select(dkm => MapEntityToViewModel(dkm)).ToList();
    }



    private List<KeyKpi_AllUserGroup_ReportSummaryViewModel> Load_AllUserGroup_SummaryList(
        List<DepartmentViewModel> DepartmentList,
        List<KeyKpiSubmission> allSubmissionsByPeriod)
    {
        var data = DepartmentList.Select(department =>
        {
            // await service.FindByKpiPeriodAndDepartmentAndUserGroupAsync(SelectedPeriodId, departmentId, usertitle);
            // Submission to Department by Period
            var submissionToDepartment = allSubmissionsByPeriod
                .Where(s => s.DepartmentId == department.Id)
                .ToList();

            // User Group Submission Info (group name, count, total score)
            var groupScores = UserGroupList.Select(group =>
            {
                // Submission by User Group to Department by Period
                var submittedByGroup = submissionToDepartment
                    .Where(s => s.SubmittedBy.UserTitleId == group.Id)
                    .ToList();

                var totalKeysSubmitted_ByGroup = submittedByGroup.Select(s =>
                    s.KeyKpiSubmissionItems.DistinctBy(item =>
                        item.DepartmentKeyMetricId)).Count();
                var totalSubmission_ByGroup = submittedByGroup
                    .Select(s => s.KeyKpiSubmissionItems).Count();
                var totalScore_ByGroup = submittedByGroup
                    .SelectMany(s => s.KeyKpiSubmissionItems
                        .Select(item => item.ScoreValue)).Sum();

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
    /// Takes userList to include all users
    /// Taks departmentList to include all departments
    /// </summary>
    /// <param name="allSubmissionsByPeriod"></param>
    /// <param name="userList"></param>
    /// <param name="departmentList"></param>
    /// <returns></returns>
    private List<KeyKpi_AllUserGroup_ReportDetailViewModel> Load_AllUserGroup_DetailList(
        List<KeyKpiSubmission> allSubmissionsByPeriod,
        List<UserViewModel> userList,
        List<DepartmentViewModel> departmentList)
    {
        // Steps:
        // 1.0 for each USER,
        // 1.1      -> loop DEPARTMENT list
        // 2.0 for each DEPARTMENT (of a USER), 
        // 2.1      -> filter the SUBMISSIONS by USER and by DEPARTMENT
        // 3.0 for each DEPARTMENT KEY METRIC list (DKMs), filter: [WHERE DKM.Id == SUBMISSION's dkmId]
        // 3.1      -> filter SubmissionItem by DKM.Id

        List<KeyKpi_AllUserGroup_ReportDetailViewModel> allUserGroupDetailList = userList.Select(user =>
        {
            // 1.0 for each user,
            // 1.1 -> loop DEPARTMENT list
            List<KeyKpi_DepartmentScoreDetail> departmentScoreDetailList = departmentList
                .SelectMany(department =>
                {
                    // 2.0 for each DEPARTMENT (of a user), 
                    // 2.1 -> filter the SUBMISSIONS by USER and by DEPARTMENT
                    // TODO: We could just call service method directly as: _keyKpiSubmissionService.FindAsync(SelectedPeriod.Id, user.Id, department.Id);
                    // var submissionByPeriodByUserByDepartment = allSubmissionsByPeriod
                    //     .Where(s => s.ApplicationUserId == user.Id && s.DepartmentId == department.Id)
                    //     .FirstOrDefault();
                    var submissionByPeriodByUserByDepartment = allSubmissionsByPeriod
                        .Where(s => s.ApplicationUserId == user.Id && s.DepartmentId == department.Id)
                        .FirstOrDefault();

                    // **အမှတ်မပေးရသေးတဲ့ စာရင်းလည်း ပါချင်တာဆိုတော့ submission ကိုနေယူလို့မရဘူး
                    // submission ကနေယူရင် အမှတ်ပေးထားတဲ့စာရင်းပဲ ပါလာမယ်။
                    // DepartmentKeyMetric ကနေပြန်ဆွဲထုတ်ရင် အမှတ်မပေးထားတဲ့ key တွေလည်းရမယ်
                    // NOTE: SELECT Submissions.Items WHERE Submission.Items.DepartmentKeyMetricId == DepartmentKeyMetric.Id
                    // for each DKMs, -> get SUBMISSIONS :: WHERE DKM.Id == SUBMISSION's dkmId
                    // filter the DKMs by DEPARTMENT (which contains non-duplicated KEYS)
                    var details = DepartmentKeyMetrics
                        .Where(dkm => dkm.DepartmentId == department.Id)
                        .OrderBy(dkm => dkm.KeyIssuer.DepartmentName)
                        .Select(dkm =>
                        {

                            // 3.0 for each DEPARTMENT KEY METRIC list (DKMs)
                            // 3.1 -> filter SubmissionItem by DKM.Id
                            // LoadSubmissionItemByDkm(submission, dkm.Id);
                            var submissionItem = submissionByPeriodByUserByDepartment?.KeyKpiSubmissionItems
                                .Where(i => i.DepartmentKeyMetricId == dkm.Id)
                                .FirstOrDefault();

                            // return DepartmentScoreDetail regardless of submission data
                            if (submissionItem != null)
                            {
                                return new KeyKpi_DepartmentScoreDetail
                                {
                                    DepartmentKeyMetric = dkm, //
                                    DKMId = dkm.Id,
                                    KeyId = dkm.KeyMetricId,
                                    DepartmentName = department.DepartmentName, //
                                    KeyTitle = dkm.KeyMetric.MetricTitle,
                                    ScoreValue = submissionItem.ScoreValue, //
                                    Comments = submissionItem.Comments ?? string.Empty, //
                                };
                            }
                            // submissionByPeriodByUserByDepartment == null || 
                            // submissionByPeriodByUserByDepartment.KeyKpiSubmissionItems == null)
                            return new KeyKpi_DepartmentScoreDetail
                            {
                                DepartmentKeyMetric = null,
                                DepartmentName = dkm.KeyIssuer.DepartmentName,
                                KeyTitle = dkm.KeyMetric.MetricTitle,
                                KeyId = dkm.KeyMetricId,
                                DKMId = dkm.Id,
                                ScoreValue = 0,
                                Comments = string.Empty,
                            };
                        }).ToList();
                    return details;
                })
                .ToList();

            return new KeyKpi_AllUserGroup_ReportDetailViewModel
            {
                PeriodName = SelectedPeriod.PeriodName,
                SubmittedBy = user,
                KeyKpi_DepartmentScoreDetails = departmentScoreDetailList
            };
        }).ToList();

        return allUserGroupDetailList;
    }


    // TODO: Should we combine Load_AllUserGroup_DetailList and Load_SingleUserGroup_DetailList?? 
    private List<KeyKpi_SingleUserGroup_ReportDetailViewModel> Load_SingleUserGroup_DetailList(
        List<KeyKpiSubmission> allSubmissionsByPeriod,
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
                    .Where(s => s.ApplicationUserId == user.Id)
                    .OrderBy(s => s.TargetDepartment.DepartmentName)
                    .ToList() ?? [];
                // Loop department list to include non submitted departments
                // 3. loop department list
                var departmentScoreSummary = departmentList
                    .Select(department =>
                    {
                        var submission = submisionByUser
                            .Where(submission => submission.DepartmentId == department.Id)
                            .FirstOrDefault();
                        return new KeyKpi_DepartmentScoreSummary
                        {
                            DepartmentName = department.DepartmentName,
                            TotalKey = submission != null ? submission.KeyKpiSubmissionItems.Count : 0,
                            TotalScore = submission != null ? submission.KeyKpiSubmissionItems.Sum(i => i.ScoreValue) : 0
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

    // KeyKpiSubmissionItem to KeyKpiSubmissionItemViewModel
    private static KeyKpiSubmissionItemViewModel MapEntityToViewModel(KeyKpiSubmissionItem e)
    {
        return new KeyKpiSubmissionItemViewModel
        {
            Id = e.Id,
            KeyKpiSubmissionId = e.KeyKpiSubmissionId,
            DepartmentKeyMetricId = e.DepartmentKeyMetricId,
            DepartmentKeyMetric = new DepartmentKeyMetricViewModel
            {
                Id = e.DepartmentKeyMetric.Id,
                LookupId = e.DepartmentKeyMetric.DepartmentKeyMetricCode,
                KpiSubmissionPeriodId = e.DepartmentKeyMetric.KpiSubmissionPeriodId,
                DepartmentId = e.DepartmentKeyMetric.DepartmentId,
                KeyIssuer = new DepartmentViewModel
                {
                    Id = e.DepartmentKeyMetric.KeyIssueDepartment.Id,
                    DepartmentCode = e.DepartmentKeyMetric.KeyIssueDepartment.DepartmentCode,
                    DepartmentName = e.DepartmentKeyMetric.KeyIssueDepartment.DepartmentName
                },
                KeyMetricId = e.DepartmentKeyMetric.KeyMetricId,
                KeyMetric = new KeyMetricViewModel
                {
                    Id = e.DepartmentKeyMetric.KeyMetric.Id,
                    LookupId = e.DepartmentKeyMetric.KeyMetric.MetricCode,
                    MetricTitle = e.DepartmentKeyMetric.KeyMetric.MetricTitle,
                    Description = e.DepartmentKeyMetric.KeyMetric.Description,
                    IsDeleted = e.DepartmentKeyMetric.KeyMetric.IsDeleted
                },
                IsDeleted = e.DepartmentKeyMetric.IsDeleted
            },
            ScoreValue = e.ScoreValue,
            Comments = e.Comments
        };
    }

    private static DepartmentKeyMetricViewModel MapEntityToViewModel(DepartmentKeyMetric e)
    {
        return new DepartmentKeyMetricViewModel
        {
            Id = e.Id,
            LookupId = e.DepartmentKeyMetricCode,
            KpiSubmissionPeriodId = e.KpiSubmissionPeriodId,
            DepartmentId = e.DepartmentId,
            KeyIssuer = new DepartmentViewModel
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