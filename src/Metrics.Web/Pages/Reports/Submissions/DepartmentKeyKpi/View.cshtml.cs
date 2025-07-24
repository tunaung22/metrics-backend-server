using Metrics.Application.DTOs.KpiSubmissionDtos;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Metrics.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentKeyKpi;

public class ViewModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IUserService _userService;
    private readonly IUserTitleService _userGroupService;
    private readonly IDepartmentService _departmentService;
    private readonly IKeyKpiSubmissionService _keyKpiSubmissionService;

    public ViewModel(
        IKpiSubmissionPeriodService kpiSubmissionPeriodService,
        IUserService userService,
        IUserTitleService userGroupService,
        IDepartmentService departmentService,
        IKeyKpiSubmissionService keyKpiSubmissionService)
    {
        _kpiPeriodService = kpiSubmissionPeriodService;
        _userService = userService;
        _userGroupService = userGroupService;
        _departmentService = departmentService;
        _keyKpiSubmissionService = keyKpiSubmissionService;
    }

    // =========================================================================
    // ========== MODELS =======================================================
    // =========================================================================
    public class AllUserGroupReportSummaryViewModel
    {
        public string? PeriodName { get; set; }
        public string? DepartmentName { get; set; }
        public List<UserGroupSubmissionInfoViewModel> UserGroupSubmissionInfo { get; set; } = [];
        public long TotalSubmissions { get; set; }
        public decimal TotalScore { get; set; }
        public decimal KpiScore { get; set; }
    }
    public List<AllUserGroupReportSummaryViewModel> AllUserGroupReportSummaryList { get; set; } = [];

    public class AllUserGroupReportDetailViewModel
    {
        public string? PeriodName { get; set; }
        public string? DepartmentName { get; set; }
        public UserViewModel SubmittedBy { get; set; } = null!;
        public DateTimeOffset SubmittedAt { get; set; }
        // flatten KeyKPISubmisionItems: key, issuer, score, comments
        public DepartmentKeyMetricViewModel DepartmentKeyMetric { get; set; } = null!;
        public decimal ScoreValue { get; set; }
        public string? Comments { get; set; } = string.Empty;
    }
    public List<AllUserGroupReportDetailViewModel> AllUserGroupReportDetailList { get; set; } = [];

    public class SingleUserGroupReportSummaryViewModel { }

    public class SingleUserGroupReportDetailViewModel { }

    public class UserGroupSubmissionInfoViewModel // a single user group info
    {
        public string? GroupName { get; set; }
        public int TotalSubmissions { get; set; }
        public decimal TotalScore { get; set; }
    }

    public List<KeyKpiSubmissionViewModel> KeyKpiSubmissions { get; set; } = [];

    public KpiPeriodViewModel SelectedPeriod { get; set; } = new();

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

        // TODO: Does retrieving all items impact performance??
        var allSubmissionsByPeriod = await _keyKpiSubmissionService
            .FindByKpiPeriodAsync(SelectedPeriod.Id);

        // ==========VIEW MODELS FOR DISPLAY========================
        // ALL    + SUMMARY = AllUserGroupReportSummaryList :: AllUserGroupReportSummaryViewModel
        // ALL    + DETAIL  = AllUserGroupReportDetailList :: AllUserGroupReportDetailViewModel
        // SINGLE + SUMMARY = SingleUserGroupReportSummaryViewModel
        // SINGLE + DETAIL  = SingleUserGroupReportDetailViewModel
        // =========================================================

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

                foreach (var department in DepartmentList)
                {
                    // deapartment | [group1] | [group1-totalSubmission] | [group1-totalScore] | [group2] | [group2-totalSubmission] | [group2-totalScore] | ...
                    // TODO: entity to viewmodel
                    var filteredByDepartment = allSubmissionsByPeriod
                        .Where(s => s.DepartmentId == department.Id)
                        // .Where(s => s.SubmittedBy.UserTitleId == group.Id
                        //     && s.DepartmentId == department.Id)
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
                                UserName = s.SubmittedBy.UserName!,
                                FullName = s.SubmittedBy.FullName,
                                PhoneNumber = s.SubmittedBy.PhoneNumber,
                                UserGroup = new UserGroupViewModel
                                {
                                    Id = s.SubmittedBy.UserTitle.Id,
                                    GroupCode = s.SubmittedBy.UserTitle.TitleCode,
                                    GroupName = s.SubmittedBy.UserTitle.TitleName,
                                    Description = s.SubmittedBy.UserTitle.Description
                                },
                                Department = new DepartmentViewModel
                                {
                                    Id = s.SubmittedBy.Department.Id,
                                    DepartmentCode = s.SubmittedBy.Department.DepartmentCode,
                                    DepartmentName = s.SubmittedBy.Department.DepartmentName
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

                    List<UserGroupSubmissionInfoViewModel> userGroupSubmissions = [];
                    userGroupSubmissions = UserGroupList
                        .Select(group =>
                        {
                            var items = filteredByDepartment
                                .Where(s => s.SubmittedBy.UserGroup.Id == group.Id)
                                .SelectMany(s => s.KeyKpiSubmissionItems)
                                .ToList();
                            return new UserGroupSubmissionInfoViewModel
                            {
                                GroupName = group.GroupName,
                                TotalSubmissions = items.Count,
                                TotalScore = items.Sum(i => i.ScoreValue)
                            };
                        })
                        .ToList();

                    // foreach (var group in UserGroupList)
                    // {
                    //     // filter by each group
                    //     // to get total count, scores by each group
                    //     // get child items by group
                    //     var childItemsByGroup = filteredByDepartment
                    //         .Where(s => s.SubmittedBy.UserGroup.Id == group.Id)
                    //         .SelectMany(s => s.KeyKpiSubmissionItems)
                    //         .ToList();

                    //     userGroupSubmissions.Add(new UserGroupSubmissionInfoViewModel
                    //     {
                    //         GroupName = group.GroupName,
                    //         TotalSubmissions = childItemsByGroup.Count,
                    //         TotalScore = childItemsByGroup.Sum(i => i.ScoreValue)
                    //     });
                    // }

                    // CHECK
                    // if (UserGroupList.Count != userGroupSubmissions.Count)
                    // {
                    //     throw new InvalidOperationException("User group count mismatch");
                    // }

                    // CALCULATE for EACH GROUP
                    // TODO: Calculate Score
                    // TODO: how calculation works for Key KPI?
                    var totalSubmissions = filteredByDepartment.Count;
                    var totalScore = filteredByDepartment.SelectMany(s => s.KeyKpiSubmissionItems).Sum(i => i.ScoreValue);
                    var kpiScore = totalSubmissions > 0 ? totalScore / totalSubmissions : 0M;

                    AllUserGroupReportSummaryList.Add(new AllUserGroupReportSummaryViewModel
                    {
                        PeriodName = SelectedPeriodName ?? SelectedPeriod.PeriodName,
                        DepartmentName = department.DepartmentName,
                        UserGroupSubmissionInfo = userGroupSubmissions,
                        TotalSubmissions = totalSubmissions,
                        TotalScore = totalScore,
                        KpiScore = kpiScore
                    });
                } // end of Department loop
            }

            // ----------MODE: DETAIL-------------------------------------------
            // ALL + DETAIL :: (all group detail + with kpi)
            // view:    department | scores | submitter | comments
            // excel:   period | department | key | score | submitter | group
            else if (ViewMode.Equals("detail", StringComparison.OrdinalIgnoreCase))
            {
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

                // prepare for Detail Table (all group) 
                // -> all group details + without kpi
                // all + detail

                // // ---------- Load Users ---------------------------------------
                // var users = await _userService.FindAllActiveAsync();
                // KeyKpiSubmissionViewModel to AllUserGroupReportDetailViewModel 
                // AllUserGroupReportDetailList = keyKpiSubmissions
                //     .Select(s => new AllUserGroupReportDetailViewModel
                //     {
                //         PeriodName = s.TargetKpiPeriod.PeriodName,
                //         DepartmentName = s.TargetDepartment.DepartmentName,
                //         SubmittedBy = s.SubmittedBy,
                //         SubmittedAt = s.SubmittedAt,
                //         DepartmentKeyMetric = s.KeyKpiSubmissionItems
                //             .SelectMany(i=> new DepartmentKeyMetricViewModel
                //             {
                //                 Id = i.DepartmentKeyMetric.Id
                //             })
                //     }).ToList();
                AllUserGroupReportDetailList = keyKpiSubmissions
                    .SelectMany(
                        s => s.KeyKpiSubmissionItems,
                        (s, item) => new AllUserGroupReportDetailViewModel
                        {
                            PeriodName = s.TargetKpiPeriod.PeriodName,
                            DepartmentName = s.TargetDepartment.DepartmentName,
                            SubmittedBy = s.SubmittedBy, // same type
                            SubmittedAt = s.SubmittedAt,
                            // DepartmentKeyMetric = new DepartmentKeyMetricViewModel
                            // {
                            //     Id = item.DepartmentKeyMetric.Id,
                            //     LookupId = item.DepartmentKeyMetric.LookupId,
                            //     KpiSubmissionPeriodId = item.DepartmentKeyMetric.KpiSubmissionPeriodId,
                            //     DepartmentId = item.DepartmentKeyMetric.DepartmentId,
                            //     // KeyIssuer = new DepartmentViewModel
                            //     // {
                            //     //     Id = item.DepartmentKeyMetric.KeyIssuer.Id,
                            //     //     DepartmentCode = item.DepartmentKeyMetric.KeyIssuer.DepartmentCode,
                            //     //     DepartmentName = item.DepartmentKeyMetric.KeyIssuer.DepartmentName
                            //     // },
                            //     KeyMetricId = item.DepartmentKeyMetric.KeyMetricId,
                            //     KeyMetric = new KeyMetricViewModel
                            //     {
                            //         Id = item.DepartmentKeyMetric.KeyMetric.Id,
                            //         LookupId = item.DepartmentKeyMetric.KeyMetric.LookupId,
                            //         MetricTitle = item.DepartmentKeyMetric.KeyMetric.MetricTitle,
                            //         Description = item.DepartmentKeyMetric.KeyMetric.Description,
                            //         IsDeleted = item.DepartmentKeyMetric.KeyMetric.IsDeleted,
                            //     },
                            //     IsDeleted = item.DepartmentKeyMetric.IsDeleted
                            // },
                            DepartmentKeyMetric = item.DepartmentKeyMetric,
                            ScoreValue = item.ScoreValue,
                            Comments = item.Comments
                        })
                        .ToList();
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

    // private List<allSubmissionsByPeriod> ToViewModel(List<KpiSubmissionDto> dto)
    // {

    // }
}
