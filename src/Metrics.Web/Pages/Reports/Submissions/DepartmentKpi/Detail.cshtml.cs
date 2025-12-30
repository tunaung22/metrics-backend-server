using Metrics.Application.Domains;
using Metrics.Application.DTOs.KpiSubmissionDtos;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentKpi;

public class DetailModel(
    IUserService userService,
    IUserTitleService userGroupService,
    IKpiSubmissionPeriodService kpiPeriodService,
    IDepartmentService departmentService,
    IKpiSubmissionService kpiSubmissionService,
    IUserTitleService userTitleService) : PageModel
{
    private readonly IUserService _userService = userService;
    private readonly IUserTitleService _userGroupService = userGroupService;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IDepartmentService _departmentService = departmentService;
    private readonly IKpiSubmissionService _kpiSubmissionService = kpiSubmissionService;
    private readonly IUserTitleService _userTitleService = userTitleService;



    // =============== HANDLERS ================================================

    public async Task<IActionResult> OnGetAsync([FromRoute] string periodName, [FromQuery] string? submitter)
    {
        // ----------PERIOD-----------------------------------------------------
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }
        var period = await LoadKpiPeriods(periodName);
        if (period == null)
        {
            ModelState.AddModelError("", $"Period {periodName} not found.");
            return Page();
        }
        SelectedPeriod = period;
        IsActivePeriod = period.SubmissionEndDate >= DateTimeOffset.UtcNow;
        SelectedPeriodName = period.PeriodName;

        // ----------USER GROUPS------------------------------------------------
        UserGroupList = await LoadUserGroups();
        if (UserGroupList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "User Group is empty");
            return Page();
        }
        UserGroupListItems = LoadUserGroupListItems(UserGroupList);
        if (string.IsNullOrEmpty(Group))
            Group = "all";

        // ----------SHOW SUBMISSIONS-------------------------------------------
        ShowSubmissionModeListItems = LoadShowSubmissionListItems();
        if (string.IsNullOrEmpty(Show))
            Show = ShowSubmissionModeListItems[0].Value.ToLower();

        // ----------DEPARTMENT-------------------------------------------------
        var excludedDepartmentIDs = new List<long>();
        var cca = await _departmentService.FindByDepartmentNameAsync("cca");
        if (cca != null)
            excludedDepartmentIDs.Add(cca.Id);
        var departments = await _departmentService.FindAllAsync(excludedDepartmentIDs);
        if (!departments.IsSuccess || departments.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Department does not exist.");
            return Page();
        }
        DepartmentList = departments.Data.Select(d => d.MapToViewModel()).ToList();

        // ----------SUBMISSIONS-------------------------------------------------
        var submissionQuery = await _kpiSubmissionService
                    .FindByPeriod_Async(SelectedPeriod.Id, true);
        if (!submissionQuery.IsSuccess || submissionQuery.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to load submissions by period.");
            return Page();
        }
        var submissionsByPeriod = submissionQuery.Data;

        // ----------VIEW----------------------------------------------------------
        var GROUP_ALL = Group.Equals("all", StringComparison.OrdinalIgnoreCase);
        UserList = await LoadAllUsers();
        if (UserList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "No users found.");
            return Page();
        }

        // bool showAllSubmissions = Show.Equals("all", StringComparison.OrdinalIgnoreCase);
        bool showAllSubmissions = Show.ToLower() == "all" ? true : false;
        if (GROUP_ALL)
        {
            // ALL + 
            // submission by users of [group]
            // [user] [group] [score] [score] [score],...

            // for detail needs: 
            // submissions -> for submissions to map
            // user list -> for detail scores
            // department list -> for include all departments

            // TODO: **WHICH APPROACH TO USE??
            AllUserGroupReportDetailList = Load_AllUserGroup_DetailList(
                submissionsByPeriod,
                UserList,
                DepartmentList,
                IsActivePeriod,
                showAllSubmissions);
        }
        // ---GROUP: SINGLE
        else
        {
            // ViewMode ~= hod, management, staff,...
            SingleUserGroupReportDetailList = Load_SingleUserGroup_DetailList(
                SelectedPeriod.PeriodName,
                Group,
                submissionsByPeriod,
                UserList,
                DepartmentList,
                IsActivePeriod,
                showAllSubmissions); // department to includes all department
        }

        return Page();
    }

    // public async Task<IActionResult> OnPostExportExcelAsync(string periodName)
    // {
    //     // return Page();
    // }


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
                .Where(g => !g.TitleName.Equals("sysadmin", StringComparison.OrdinalIgnoreCase))
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
            var items = new List<SelectListItem>
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

    private static List<SelectListItem> LoadShowSubmissionListItems()
    {
        return
        [
            new() { Value = "all", Text = "All Submissions" },
            new() { Value = "received", Text = "Received Submissions" }
        ];
    }

    private async Task<List<UserViewModel>> LoadAllUsers()
    {
        var users = await _userService.FindAllAsync(includeLockedUser: true);
        if (users.IsSuccess && users.Data != null)
        {
            return users.Data
                //.Where(user => !user.Department.DepartmentName.Equals("cca", StringComparison.OrdinalIgnoreCase))
                .Select(user => user.MapToViewModel())
                .ToList();
            //         user.Department.DepartmentName.ToLower() != "cca")
            //     // (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow))
        }
        return [];
    }

    private List<AllUserGroupReportDetailViewModel> Load_AllUserGroup_DetailList(
        List<KpiSubmissionDto> submissionsByPeriod,
        List<UserViewModel> userList, // all users
        List<DepartmentViewModel> departmentList,
        bool isActivePeriod,
        bool showAllSubmissions = true)
    {
        // active period -> based on received submissions, get no-locked user
        //               -> all submissions, get no-locked user
        // old    period -> based on received submissions, get locked user
        //               -> **can't fetch all submissions (doing so will also gets submissions for new users added at later)

        // Active Period
        if (isActivePeriod)
        {
            // 1. based on all submissions       + includes only active users
            // 2. based on received submissions  + includes only active users
            var activeUsers = userList.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow).ToList();

            // Active Period:: based on all submissions + includes only active users
            if (showAllSubmissions)
            {
                return activeUsers
                    .Select(user =>
                    {
                        var userSubmissions = submissionsByPeriod
                            .Where(s => s.SubmitterId == user.Id)
                            // **note: sort by DepartmentName is required
                            .OrderBy(s => s.TargetDepartment.DepartmentName)
                            .ToList();

                        // WHY departments?? > to get all department in the result even if there is no submissions for some departments 
                        var departmentScores = departmentList
                            .Select(dpt =>
                            {
                                var submission = userSubmissions.FirstOrDefault(s => s.DepartmentId == dpt.Id);
                                return new DepartmentScoreViewModel
                                {
                                    DepartmentName = dpt.DepartmentName,
                                    Comment = submission?.Comments ?? string.Empty,
                                    ScoreValue = submission?.ScoreValue ?? 0.00M
                                };
                            })
                            .OrderBy(dpt => dpt.DepartmentName)
                            .ToList();

                        return new AllUserGroupReportDetailViewModel
                        {
                            PeriodName = SelectedPeriod.PeriodName,
                            SubmittedBy = user,
                            DepartmentScores = departmentScores
                        };
                    })
                .ToList();
            }
            // Active Period:: based on received submissions + includes only active users
            else
            {
                return activeUsers
                    // users who actual submitted
                    .Where(user => submissionsByPeriod.Any(s => s.SubmitterId == user.Id))
                    .Select(user =>
                    {
                        var userSubmissions = submissionsByPeriod
                            .Where(s => s.SubmitterId == user.Id)
                            // **note: sort by DepartmentName is required
                            .OrderBy(s => s.TargetDepartment.DepartmentName)
                            .ToList() ?? [];

                        // WHY departments?? > to get all department in the result even if there is no submissions for some departments 
                        var departmentScores = departmentList
                            .Select(dpt =>
                            {
                                var submission = userSubmissions.FirstOrDefault(s => s.DepartmentId == dpt.Id);
                                return new DepartmentScoreViewModel
                                {
                                    DepartmentName = dpt.DepartmentName,
                                    Comment = submission?.Comments ?? string.Empty,
                                    ScoreValue = submission?.ScoreValue ?? 0.00M
                                };
                            })
                            .OrderBy(dpt => dpt.DepartmentName)
                            .ToList();

                        return new AllUserGroupReportDetailViewModel
                        {
                            PeriodName = SelectedPeriod.PeriodName,
                            SubmittedBy = user,
                            DepartmentScores = departmentScores
                        };
                    })
                .ToList();
            }
        }

        // Old Period:: based on received submissions + all users
        else
        {
            return userList
                // users who actual submitted
                .Where(user => submissionsByPeriod.Any(s => s.SubmitterId == user.Id))
                .Select(user =>
                {
                    var userSubmissions = submissionsByPeriod
                        .Where(s => s.SubmitterId == user.Id)
                        // **note: sort by DepartmentName is required
                        .OrderBy(s => s.TargetDepartment.DepartmentName)
                        .ToList() ?? [];

                    // WHY departments?? > to get all department in the result even if there is no submissions for some departments 
                    var departmentScores = departmentList
                        .Select(dpt =>
                        {
                            var submission = userSubmissions.FirstOrDefault(s => s.DepartmentId == dpt.Id);
                            return new DepartmentScoreViewModel
                            {
                                DepartmentName = dpt.DepartmentName,
                                Comment = submission?.Comments ?? string.Empty,
                                ScoreValue = submission?.ScoreValue ?? 0.00M
                            };
                        })
                        .OrderBy(dpt => dpt.DepartmentName)
                        .ToList();

                    return new AllUserGroupReportDetailViewModel
                    {
                        PeriodName = SelectedPeriod.PeriodName,
                        SubmittedBy = user,
                        DepartmentScores = departmentScores
                    };
                })
            .ToList();
        }
    }

    private static List<SingleUserGroupReportDetailViewModel> Load_SingleUserGroup_DetailList(
        string periodName,
        string selectedGroupName,
        List<KpiSubmissionDto> submissionsByPeriod,
        List<UserViewModel> userList,
        List<DepartmentViewModel> departmentList,
        bool isActivePeriod,
        bool showAllSubmissions = true)
    {
        // active Period -> baesd on all submissions       + includes only active users
        //               -> based on received submissions  + includes only active users
        // old    Period -> based on received submissions  + all users

        // Active Period
        if (isActivePeriod)
        {
            // 1. based on all submissions       + includes only active users
            // 2. based on received submissions  + includes only active users
            var activeUsers = userList.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow).ToList();

            // Active Period:: based on ALL submissions + includes only ACTIVE users
            if (showAllSubmissions)
            {
                return activeUsers
                    .Where(user => user.UserGroup.GroupName.Equals(selectedGroupName, StringComparison.OrdinalIgnoreCase))
                    .Select(user =>
                    {
                        // submission by [user] of [group]
                        var submissionByUser = submissionsByPeriod
                            .Where(s => s.SubmitterId == user.Id)
                            // .Where(s => s.SubmittedBy.UserTitle.TitleName.Equals(Group, StringComparison.OrdinalIgnoreCase)
                            //     && s.ApplicationUserId == user.Id)
                            // **note: sort by DepartmentName is required
                            .OrderBy(s => s.TargetDepartment.DepartmentName)
                            .ToList();

                        return new SingleUserGroupReportDetailViewModel
                        {
                            PeriodName = periodName,
                            SubmittedBy = user,
                            GroupName = selectedGroupName,
                            DepartmentScores = departmentList.Select(dpt =>
                            {
                                var submissions = submissionByUser.FirstOrDefault(s => s.DepartmentId == dpt.Id);

                                return new DepartmentScoreViewModel
                                {
                                    DepartmentName = dpt.DepartmentName,
                                    Comment = submissions?.Comments ?? string.Empty,
                                    ScoreValue = submissions?.ScoreValue ?? 0.00M
                                };
                            })
                            .OrderBy(dpt => dpt.DepartmentName)
                            .ToList()
                        };
                    }
                ).ToList();
            }
            // Active Period:: based on RECEIVED submissions + includes only ACTIVE users
            else
            {
                return activeUsers
                    .Where(user =>
                        // users who actual submitted
                        submissionsByPeriod.Any(s => s.SubmitterId == user.Id) &&
                        user.UserGroup.GroupName.Equals(selectedGroupName, StringComparison.OrdinalIgnoreCase))
                    .Select(user =>
                    {
                        // submission by [user] of [group]
                        var submissionByUser = submissionsByPeriod
                            .Where(s => s.SubmitterId == user.Id)
                            // .Where(s => s.SubmittedBy.UserTitle.TitleName.Equals(Group, StringComparison.OrdinalIgnoreCase)
                            //     && s.ApplicationUserId == user.Id)
                            // **note: sort by DepartmentName is required
                            .OrderBy(s => s.TargetDepartment.DepartmentName)
                            .ToList();

                        return new SingleUserGroupReportDetailViewModel
                        {
                            PeriodName = periodName,
                            SubmittedBy = user,
                            GroupName = selectedGroupName,
                            DepartmentScores = departmentList.Select(dpt =>
                            {
                                var submissions = submissionByUser
                                    .Where(s => s.DepartmentId == dpt.Id)
                                    .FirstOrDefault();

                                return new DepartmentScoreViewModel
                                {
                                    DepartmentName = dpt.DepartmentName,
                                    Comment = submissions?.Comments ?? string.Empty,
                                    ScoreValue = submissions?.ScoreValue ?? 0.00M
                                };
                            })
                            .OrderBy(dpt => dpt.DepartmentName)
                            .ToList()
                        };
                    }
                ).ToList();
            }
        }
        // Old Period:: based on received submissions  + all users
        else
        {
            return userList
                .Where(user =>
                    // users who actual submitted
                    submissionsByPeriod.Any(s => s.SubmitterId == user.Id) &&
                    user.UserGroup.GroupName.Equals(selectedGroupName, StringComparison.OrdinalIgnoreCase))
                .Select(user =>
                {
                    // submission by [user] of [group]
                    var submissionByUser = submissionsByPeriod
                        .Where(s => s.SubmitterId == user.Id)
                        // .Where(s => s.SubmittedBy.UserTitle.TitleName.Equals(Group, StringComparison.OrdinalIgnoreCase)
                        //     && s.ApplicationUserId == user.Id)
                        // **note: sort by DepartmentName is required
                        .OrderBy(s => s.TargetDepartment.DepartmentName)
                        .ToList();

                    return new SingleUserGroupReportDetailViewModel
                    {
                        PeriodName = periodName,
                        SubmittedBy = user,
                        GroupName = selectedGroupName,
                        DepartmentScores = departmentList.Select(dpt =>
                        {
                            var submissions = submissionByUser
                                .Where(s => s.DepartmentId == dpt.Id)
                                .FirstOrDefault();

                            return new DepartmentScoreViewModel
                            {
                                DepartmentName = dpt.DepartmentName,
                                Comment = submissions?.Comments ?? string.Empty,
                                ScoreValue = submissions?.ScoreValue ?? 0.00M
                            };
                        })
                        .OrderBy(dpt => dpt.DepartmentName)
                        .ToList()
                    };
                })
            .ToList();
        }
    }

    // =============== MODELS ==================================================
    // ----------Collection----------
    public List<UserGroupViewModel> UserGroupList { get; set; } = [];
    public List<DepartmentViewModel> DepartmentList { get; set; } = [];
    public List<UserViewModel> UserList { get; set; } = [];


    // ----------Period----------
    public KpiPeriodViewModel SelectedPeriod { get; set; } = null!;
    public string SelectedPeriodName { get; set; } = null!;
    public bool IsActivePeriod { get; set; }

    // ----------User Group----------
    [BindProperty]
    public List<SelectListItem> UserGroupListItems { get; set; } = []; // for select element

    [BindProperty(SupportsGet = true)]
    public string? Group { get; set; } // selected item (for filter select element)

    // ----------Show Submissions Mode----------
    [BindProperty]
    public List<SelectListItem> ShowSubmissionModeListItems { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Show { get; set; } // selected item (for filter select element)





    public class DepartmentScoreViewModel
    {
        public string? DepartmentName { get; set; }
        public decimal ScoreValue { get; set; }
        public string? Comment { get; set; }
    }


    public class AllUserGroupReportDetailViewModel
    {
        public string? PeriodName { get; set; }
        public UserViewModel SubmittedBy { get; set; } = null!;
        public List<DepartmentScoreViewModel> DepartmentScores { get; set; } = [];
    }
    public List<AllUserGroupReportDetailViewModel> AllUserGroupReportDetailList { get; set; } = [];


    public class SingleUserGroupReportDetailViewModel
    {
        public string? PeriodName { get; set; }
        public UserViewModel SubmittedBy { get; set; } = null!;
        public string? GroupName { get; set; }
        public List<DepartmentScoreViewModel> DepartmentScores { get; set; } = [];
    }
    public List<SingleUserGroupReportDetailViewModel> SingleUserGroupReportDetailList { get; set; } = [];


    // public class ScoreSubmissionDetailReportViewModel
    // {
    //     public string? PeriodName { get; set; }
    //     public string? DepartmentName { get; set; }
    //     public Decimal GivenScore { get; set; }
    //     public string? UserFullName { get; set; }
    //     public string? ApplicationUserId { get; set; }
    //     public string? PositiveAspects { get; set; }
    //     public string? NegativeAspects { get; set; }
    //     public string? Comments { get; set; }
    //     // public string? UserGroupName { get; set; }
    //     // public long TotalUser { get; set; }
    //     // public long TotalSubmissions { get; set; }
    //     // public decimal TotalScore { get; set; }
    //     // public decimal KpiScore { get; set; }
    // }

    // public List<ScoreSubmissionDetailReportViewModel> ScoreSubmissionDetailReports { get; set; } = new();

    // public KpiPeriodViewModel SelectedPeriod { get; set; } = null!;

    // [BindProperty(SupportsGet = true)]
    // public string? Submitter { get; set; } // for filter by user

    // public class ExcelDtoAllUserModel
    // {
    //     [ExcelColumnWidth(15)]
    //     [ExcelColumn(Name = "Period Name")]
    //     public string? PeriodName { get; set; }

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Scoring Department")]
    //     public string? ScoringDepartment { get; set; }

    //     [ExcelColumnWidth(10)]
    //     [ExcelColumn(Name = "Score")]
    //     public decimal GivenScore { get; set; }

    //     [ExcelColumnWidth(15)]
    //     [ExcelColumn(Name = "Submitted By")]
    //     public string? SubmittedBy { get; set; }

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Positive Aspects")]
    //     public string? PositiveAspects { get; set; }

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Negative Aspects")]
    //     public string? NegativeAspects { get; set; }

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Comments")]
    //     public string? Comments { get; set; }
    // }
    // public class ExcelDtoSingleUserModel
    // {
    //     [ExcelColumnWidth(15)]
    //     [ExcelColumn(Name = "Period Name")]
    //     public string? PeriodName { get; set; }

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Scoring Department")]
    //     public string? ScoringDepartment { get; set; }

    //     [ExcelColumnWidth(10)]
    //     [ExcelColumn(Name = "Score")]
    //     public decimal GivenScore { get; set; }

    //     [ExcelColumnWidth(15)]
    //     [ExcelColumn(Name = "Submitted By")]
    //     public string? SubmittedBy { get; set; }

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Positive Aspects")]
    //     public string? PositiveAspects { get; set; }

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Negative Aspects")]
    //     public string? NegativeAspects { get; set; }

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Comments")]
    //     public string? Comments { get; set; }
    // }

}
