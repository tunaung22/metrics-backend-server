using Metrics.Application.Domains;
using Metrics.Application.DTOs.KpiSubmissionDtos;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Services;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Metrics.Web.Models.ReportViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentKpi;

public class ViewModel(
        IKpiSubmissionPeriodService kpiSubmissionPeriodService,
        IUserService userService,
        IUserTitleService userGroupService,
        IDepartmentService departmentService,
        IKpiSubmissionService kpiSubmissionService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiSubmissionPeriodService;
    private readonly IUserService _userService = userService;
    private readonly IUserTitleService _userGroupService = userGroupService;
    private readonly IDepartmentService _departmentService = departmentService;
    private readonly IKpiSubmissionService _kpiSubmissionService = kpiSubmissionService;

    // ========== HANDLERS =====================================================
    public async Task<IActionResult> OnGetAsync(string periodName)
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
        SelectedPeriodName = period.PeriodName;

        // ----------VIEW MODE--------------------------------------------------
        ViewModeListItems = InitViewModeListItems();
        if (string.IsNullOrEmpty(ViewMode))
            ViewMode = ViewModeListItems[0].Value.ToLower();

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


        // ---------------------------------------------------------------------
        // ==========VIEW MODELS FOR DISPLAY====================================
        // ALL    + SUMMARY = AllUserGroupReportSummaryList :: AllUserGroupReportSummaryViewModel
        // ALL    + DETAIL  = AllUserGroupReportDetailList :: AllUserGroupReportDetailViewModel
        // SINGLE + SUMMARY = SingleUserGroupReportSummaryViewModel
        // SINGLE + DETAIL  = SingleUserGroupReportDetailViewModel
        // summary + all
        // summary + single
        // detail + all
        // detail + single
        // =====================================================================

        var submissionQuery = await _kpiSubmissionService
            .FindByPeriod_Async(SelectedPeriod.Id, true);
        if (!submissionQuery.IsSuccess || submissionQuery.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to load submissions by period.");
            return Page();
        }
        var submissionsByPeriod = submissionQuery.Data;


        // ==========VIEW==============================================================================
        var MODE_SUMMARY = ViewMode.Equals("summary", StringComparison.OrdinalIgnoreCase);
        var MODE_DETAIL = ViewMode.Equals("detail", StringComparison.OrdinalIgnoreCase);
        var GROUP_ALL = Group.Equals("all", StringComparison.OrdinalIgnoreCase);

        if (MODE_SUMMARY)
        {
            // SUMMARY ALL
            if (GROUP_ALL)
            {
                AllUserGroupReportSummaryList = DepartmentList.Select(department =>
                {
                    var submissionToDepartment = submissionsByPeriod
                        .Where(s => s.DepartmentId == department.Id)
                        .ToList();

                    // 2. loop Group to get each Group's total submissions/score
                    var groupScores = UserGroupList.Select(group =>
                    {
                        // submissions to [Department] by [Group] -> eg: hod, 34, 272
                        var submissions = submissionToDepartment
                            .Where(s => s.SubmittedBy.UserGroup.Id == group.Id)
                            .ToList();

                        return new UserGroupSubmissionInfoViewModel
                        {
                            GroupName = group.GroupName,
                            TotalSubmissions = submissions.Count,
                            TotalScore = submissions.Sum(e => e.ScoreValue)
                        };
                    }).ToList();

                    // CALCULATE TOTAL SCORE (summarized score) 
                    // get from groupScores or submissionToDepartment 
                    // -> var totalSubmissions = groupScores.Sum(g => g.TotalSubmissions); // var totalScore = groupScores.Sum(g => g.TotalScore);
                    var totalSubmissions = submissionToDepartment.Count;
                    var totalScore = submissionToDepartment.Sum(g => g.ScoreValue);
                    var kpiScore = (totalSubmissions > 0)
                            ? (totalScore / totalSubmissions) : 0M;

                    return new AllUserGroupReportSummaryViewModel
                    {
                        PeriodName = SelectedPeriod.PeriodName,
                        DepartmentName = department.DepartmentName,
                        UserGroupSubmissions = groupScores,
                        TotalSubmissions = totalSubmissions,
                        TotalScore = totalScore,
                        KpiScore = kpiScore
                    };
                }).ToList();
            }
            // SUMMARY SINGLE
            else
            {
                // SINGLE + SUMMARY
                // ViewMode ~= hod, management, staff,...
                SingleUserGroupReportSummaryList = Load_SingleUserGroup_SummaryList(
                                    SelectedPeriod.PeriodName, //
                                    Group, // group to show
                                    submissionsByPeriod, // source
                                    DepartmentList); // dpeartments to show
            }
        }
        // VIEWMODE: DETAIL
        else if (MODE_DETAIL)
        {
            UserList = await LoadUserList("Staff");
            if (UserList.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No users found.");
                return Page();
            }

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
                    DepartmentList);
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
                    DepartmentList); // department to includes all department
            }
        }



        /*
        // // ----------GROUP: ALL-------------------------------------------------
        // if (Group.Equals("all", StringComparison.OrdinalIgnoreCase))
        // {
        //     // ----------MODE: SUMMARY------------------------------------------
        //     // ALL + SUMMARY
        //     // 1. loop Department
        //     // 2. loop Group to get each Group's total submissions/score
        //     if (ViewMode.Equals("summary", StringComparison.OrdinalIgnoreCase))
        //     {
        //         // 1. loop Department
        //         AllUserGroupReportSummaryList = DepartmentList.Select(department =>
        //         {
        //             var submissionToDepartment = allSubmissionsByPeriod
        //                 .Where(s => s.DepartmentId == department.Id)
        //                 .ToList();

        //             // 2. loop Group to get each Group's total submissions/score
        //             var groupScores = UserGroupList.Select(group =>
        //             {
        //                 // submissions to [Department] by [Group] -> eg: hod, 34, 272
        //                 var submissions = submissionToDepartment
        //                     .Where(s => s.SubmittedBy.UserTitleId == group.Id)
        //                     .ToList();

        //                 return new UserGroupSubmissionInfoViewModel
        //                 {
        //                     GroupName = group.GroupName,
        //                     TotalSubmissions = submissions.Count,
        //                     TotalScore = submissions.Sum(e => e.ScoreValue)
        //                 };
        //             }).ToList();

        //             // CALCULATE TOTAL SCORE (summarized score) 
        //             // get from groupScores or submissionToDepartment // -> var totalSubmissions = groupScores.Sum(g => g.TotalSubmissions); // var totalScore = groupScores.Sum(g => g.TotalScore);
        //             var totalSubmissions = submissionToDepartment.Count;
        //             var totalScore = submissionToDepartment.Sum(g => g.ScoreValue);
        //             var kpiScore = (totalSubmissions > 0)
        //                     ? (totalScore / totalSubmissions) : 0M;

        //             return new AllUserGroupReportSummaryViewModel
        //             {
        //                 PeriodName = SelectedPeriod.PeriodName,
        //                 DepartmentName = department.DepartmentName,
        //                 UserGroupSubmissions = groupScores,
        //                 TotalSubmissions = totalSubmissions,
        //                 TotalScore = totalScore,
        //                 KpiScore = kpiScore
        //             };
        //         }).ToList();
        //     }
        //     // -----------------------------------------------------------------
        //     // ----------MODE: DETAIL-------------------------------------------
        //     else if (ViewMode.Equals("detail", StringComparison.OrdinalIgnoreCase))
        //     {
        //         UserList = await LoadUserList();
        //         if (UserList.Count == 0)
        //         {
        //             ModelState.AddModelError(string.Empty, "No users found.");
        //             return Page();
        //         }

        //         AllUserGroupReportDetailList = UserList.Select(user =>
        //         {
        //             var userSubmissions = allSubmissionsByPeriod
        //                 .Where(s => s.ApplicationUserId == user.Id)
        //                 // **note: sort by DepartmentName is required
        //                 .OrderBy(s => s.TargetDepartment.DepartmentName)
        //                 .ToList() ?? [];

        //             return new AllUserGroupReportDetailViewModel
        //             {
        //                 PeriodName = SelectedPeriod.PeriodName,
        //                 SubmittedBy = user,
        //                 DepartmentScores = userSubmissions.Select(s => new DepartmentScoreViewModel
        //                 {
        //                     DepartmentName = s.TargetDepartment.DepartmentName,
        //                     ScoreValue = s.ScoreValue
        //                 }).ToList()
        //             };
        //         }).ToList();
        //     }
        // }
        // // ---------------------------------------------------------------------
        // // ----------GROUP: SINGLE----------------------------------------------
        // else
        // {
        //     // ----------MODE: SUMMARY------------------------------------------
        //     if (ViewMode.Equals("summary", StringComparison.OrdinalIgnoreCase))
        //     { }

        //     // ----------MODE: DETAIL-------------------------------------------
        //     else if (ViewMode.Equals("detail", StringComparison.OrdinalIgnoreCase))
        //     { }
        // }
        */

        return Page();
    }

    public async Task<IActionResult> OnPostExportExcelAsync(string periodName)
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
        SelectedPeriodName = period.PeriodName;

        // ----------VIEW MODE--------------------------------------------------
        ViewModeListItems = InitViewModeListItems();
        if (string.IsNullOrEmpty(ViewMode))
            ViewMode = ViewModeListItems[0].Value.ToLower();

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


        var submissionQuery = await _kpiSubmissionService
            .FindByPeriod_Async(SelectedPeriod.Id, true);
        if (!submissionQuery.IsSuccess || submissionQuery.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to load submissions by period.");
            return Page();
        }
        var submissionsByPeriod = submissionQuery.Data;


        // ==========VIEW==============================================================================
        var MODE_SUMMARY = ViewMode.Equals("summary", StringComparison.OrdinalIgnoreCase);
        var MODE_DETAIL = ViewMode.Equals("detail", StringComparison.OrdinalIgnoreCase);
        var GROUP_ALL = Group.Equals("all", StringComparison.OrdinalIgnoreCase);
        // var GROUP_SINGLE = !Group.Equals("all", StringComparison.OrdinalIgnoreCase);

        if (MODE_SUMMARY)
        {
            if (GROUP_ALL)
            {
                // ALL + SUMMARY
                AllUserGroupReportSummaryList = LoadAllUserGroupSummaryList(
                submissionsByPeriod,
                DepartmentList);

                // PREPARE FOR EXCEL FILE
                var colPeriod = "Period";
                var colDepartment = "Department";
                var colTotalSubmissions = "Total Submissions";
                var colTotalScore = "Total Score";
                var colKpiScore = "KPI Score";


                var excelData = new List<Dictionary<string, object>>();
                excelData = AllUserGroupReportSummaryList.Select(submission =>
                {
                    var data = new Dictionary<string, object>()
                    {
                        [colPeriod] = submission.PeriodName ?? "[undefined period]",
                        [colDepartment] = submission.DepartmentName ?? "[undefined department]",
                    };

                    foreach (var item in submission.UserGroupSubmissions)
                    {
                        data[$"{item.GroupName} Submissions"] = Convert.ToDecimal(item.TotalSubmissions.ToString("0"));
                        data[$"{item.GroupName} Scores"] = Convert.ToDecimal(item.TotalScore.ToString("0.00"));
                    }

                    data["Total Submissions"] = Convert.ToDecimal(submission.TotalSubmissions.ToString("0.00"));
                    data["Total Score"] = Convert.ToDecimal(submission.TotalScore.ToString("0.00"));
                    data["KPI Score"] = Convert.ToDecimal(submission.KpiScore.ToString("0.00"));

                    return data;
                }).ToList();

                // Dynamic Columns
                var dynamicCols = new List<DynamicExcelColumn>
                    {
                        new(colPeriod) { Width = 10 },
                        new(colDepartment) { Width = 30 },
                        new(colTotalSubmissions) { Width = 16 },
                        new(colTotalScore) { Width = 12 },
                        new(colKpiScore) { Width = 12 }
                    };
                AddMissingDynamicColumns(excelData, dynamicCols);


                // Write Excel file
                var memoryStream = new MemoryStream();
                MiniExcel.SaveAs(
                    stream: memoryStream,
                    value: excelData,
                    configuration: new OpenXmlConfiguration
                    {
                        DynamicColumns = dynamicCols.ToArray(),
                    }
                );
                memoryStream.Position = 0; // Reset stream position

                return File(
                    memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report_DepartmentKPI_Summary_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
                );
            }
            // SUMMARY
            else
            {
                // SINGLE + SUMMARY
                SingleUserGroupReportSummaryList = Load_SingleUserGroup_SummaryList(
                    SelectedPeriod.PeriodName, //
                    Group, // group to show
                    submissionsByPeriod, // source
                    DepartmentList); // dpeartments to show

                // PREPARE FOR EXCEL FILE
                var colPeriod = "Period";
                var colGroupName = "Group";
                var colDepartment = "Department";
                var colTotalSubmissions = "Total Submissions";
                var colTotalScore = "Total Score";
                var colKpiScore = "KPI Score";

                var excelData = new List<Dictionary<string, object>>();
                excelData = SingleUserGroupReportSummaryList.Select(submission =>
                {
                    var data = new Dictionary<string, object>()
                    {
                        [colPeriod] = submission.PeriodName ?? "[undefined period]",
                        [colGroupName] = Group.ToUpper(),
                        [colDepartment] = submission.DepartmentName ?? "[undefined department]",
                        [colTotalSubmissions] = Convert.ToDecimal(submission.TotalSubmissions.ToString("0.00")),
                        [colTotalScore] = Convert.ToDecimal(submission.TotalScore.ToString("0.00")),
                        [colKpiScore] = Convert.ToDecimal(submission.KpiScore.ToString("0.00"))
                    };

                    // foreach (var item in submission.UserGroupSubmissions)
                    // {
                    //     data[$"{item.GroupName} Submissions"] = Convert.ToDecimal(item.TotalSubmissions.ToString("0"));
                    //     data[$"{item.GroupName} Scores"] = Convert.ToDecimal(item.TotalScore.ToString("0.00"));
                    // }

                    return data;
                }).ToList();

                // Dynamic Columns
                var dynamicCols = new List<DynamicExcelColumn>
                {
                    new(colPeriod) { Width = 10 },
                    new(colGroupName) { Width = 16 },
                    new(colDepartment) { Width = 25 },
                    new(colTotalSubmissions) { Width = 16 },
                    new(colTotalScore) { Width = 16 },
                    new(colKpiScore) { Width = 16 }
                };
                // AddMissingDynamicColumns(excelData, dynamicCols);

                // Write Excel file
                var memoryStream = new MemoryStream();
                MiniExcel.SaveAs(
                    stream: memoryStream,
                    value: excelData,
                    configuration: new OpenXmlConfiguration
                    {
                        DynamicColumns = dynamicCols.ToArray(),
                    }
                );
                memoryStream.Position = 0; // Reset stream position

                return File(
                    memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report_DepartmentKPI_{Group}_Detail_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
                );
            }
        }
        else if (MODE_DETAIL)
        {
            UserList = await LoadUserList("Staff");
            if (UserList.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No users found.");
                return Page();
            }

            // DETAIL
            if (GROUP_ALL)
            {
                // ALL + DETAIL
                AllUserGroupReportDetailList = Load_AllUserGroup_DetailList(
                    submissionsByPeriod,
                    UserList,
                    DepartmentList);

                // PREPARE FOR EXCEL FILE
                var colPeriod = "Period";
                var colCandidateID = "Candidate ID";
                var colCandidate = "Candidate";
                var colDepartment = "Department";
                var colGroupName = "Group";
                var colDepartmentNameList = new List<string>();
                // Row: [Total] [Total] [Total]

                // 
                var excelData = new List<Dictionary<string, object>>();
                excelData = AllUserGroupReportDetailList.Select(submission =>
                {
                    var data = new Dictionary<string, object>()
                    {
                        [colPeriod] = submission.PeriodName ?? "[undefined period]",
                        [colCandidateID] = submission.SubmittedBy.UserCode ?? "[undefined ID]",
                        [colCandidate] = submission.SubmittedBy.FullName ?? "[undefined candidate]",
                        [colDepartment] = submission.SubmittedBy.Department.DepartmentName ?? "[undefined department]",
                        [colGroupName] = submission.SubmittedBy.UserGroup.GroupName.ToUpper()
                    };

                    foreach (var ds in submission.DepartmentScores)
                    {
                        data[$"{ds.DepartmentName}"] = Convert.ToDecimal(ds.ScoreValue.ToString("0.00"));
                        data[$"Comment on {ds.DepartmentName}"] = ds.Comment ?? string.Empty;
                    }

                    return data;
                }).ToList();

                // Dynamic Columns
                var dynamicCols = new List<DynamicExcelColumn>
                {
                    new(colPeriod) { Width = 10 },
                    new(colCandidate) { Width = 25 }
                };

                // Add Missing Keys for Dynamic Columns
                // **pass dynamicCols and update it (as dynamicCols is reference type)
                AddMissingDynamicColumns(excelData, dynamicCols);

                // Write Excel file
                var memoryStream = new MemoryStream();
                MiniExcel.SaveAs(
                    stream: memoryStream,
                    value: excelData,
                    configuration: new OpenXmlConfiguration
                    {
                        DynamicColumns = dynamicCols.ToArray(),
                    }
                );
                memoryStream.Position = 0; // Reset stream position

                return File(
                    memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report_DepartmentKPI_AllGroup_Detail_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
                );
            }
            // DETAIL
            else
            {
                // SINGLE + DETAIL
                // ViewMode ~= hod, management, staff,...
                SingleUserGroupReportDetailList = Load_SingleUserGroup_DetailList(
                    SelectedPeriod.PeriodName,
                    Group,
                    submissionsByPeriod,
                    UserList,
                    DepartmentList);

                // PREPARE FOR EXCEL FILE
                var colPeriod = "Period";
                var colCandidateID = "Candidate ID";
                var colCandidate = "Candidate";
                var colDepartment = "Department";
                var colGroupName = "Group";
                // var colDepartmentNameList = new List<string>();

                var excelData = new List<Dictionary<string, object>>();
                excelData = SingleUserGroupReportDetailList.Select(submission =>
                {
                    var data = new Dictionary<string, object>()
                    {
                        [colPeriod] = submission.PeriodName ?? "undefined period",
                        [colCandidateID] = submission.SubmittedBy.UserCode ?? "[undefined ID]",
                        [colCandidate] = submission.SubmittedBy.FullName ?? "[undefined candidate]",
                        [colDepartment] = submission.SubmittedBy.Department.DepartmentName ?? "[undefined department]",
                        [colGroupName] = submission.SubmittedBy.UserGroup.GroupName.ToUpper() ?? "[undefined group]"
                    };

                    foreach (var ds in submission.DepartmentScores)
                    {
                        if (ds.DepartmentName != null)
                        {
                            data[$"{ds.DepartmentName}"] = Convert.ToDecimal(ds.ScoreValue.ToString("0.00"));
                            data[$"Comment on {ds.DepartmentName}"] = ds.Comment ?? string.Empty;
                        }
                    }

                    return data;
                }).ToList();


                // Dynamic Columns
                var dynamicCols = new List<DynamicExcelColumn>
                {
                    new(colPeriod) { Width = 10 },
                    new(colCandidate) { Width = 25 }
                };
                // Add Missing Keys for Dynamic Columns
                // **pass dynamicCols and update it (as dynamicCols is reference type)
                AddMissingDynamicColumns(excelData, dynamicCols);

                // Write Excel file
                var memoryStream = new MemoryStream();
                MiniExcel.SaveAs(
                    stream: memoryStream,
                    value: excelData,
                    configuration: new OpenXmlConfiguration
                    {
                        DynamicColumns = dynamicCols.ToArray(),
                    }
                );
                memoryStream.Position = 0; // Reset stream position

                return File(
                    memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report_DepartmentKPI_{Group}_Detail_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
                );
            }
        }










        /*
        // ----------GROUP: ALL-------------------------------------------------
        if (Group.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            // ----------MODE: SUMMARY------------------------------------------
            if (ViewMode.Equals("summary", StringComparison.OrdinalIgnoreCase))
            {
                // ALL + SUMMARY
                AllUserGroupReportSummaryList = LoadAllUserGroupSummaryList(
                    submissionsByPeriod,
                    DepartmentList);

                // PREPARE FOR EXCEL FILE
                var colPeriod = "Period";
                var colCaseDepartment = "Department";
                var colTotalSubmissions = "Total Submissions";
                var colTotalScore = "Total Score";
                var colKpiScore = "KPI Score";


                var excelData = new List<Dictionary<string, object>>();
                excelData = AllUserGroupReportSummaryList.Select(submission =>
                {
                    var data = new Dictionary<string, object>()
                    {
                        [colPeriod] = submission.PeriodName ?? "[undefined period]",
                        [colCaseDepartment] = submission.DepartmentName ?? "[undefined department]",
                    };

                    foreach (var item in submission.UserGroupSubmissions)
                    {
                        data[$"{item.GroupName} Submissions"] = Convert.ToDecimal(item.TotalSubmissions.ToString("0"));
                        data[$"{item.GroupName} Scores"] = Convert.ToDecimal(item.TotalScore.ToString("0.00"));
                    }

                    data["Total Submissions"] = Convert.ToDecimal(submission.TotalSubmissions.ToString("0.00"));
                    data["Total Score"] = Convert.ToDecimal(submission.TotalScore.ToString("0.00"));
                    data["KPI Score"] = Convert.ToDecimal(submission.KpiScore.ToString("0.00"));

                    return data;
                }).ToList();

                // Dynamic Columns
                var dynamicCols = new List<DynamicExcelColumn>
                {
                    new(colPeriod) { Width = 10 },
                    new(colCaseDepartment) { Width = 30 },
                    new(colTotalSubmissions) { Width = 16 },
                    new(colTotalScore) { Width = 12 },
                    new(colKpiScore) { Width = 12 }
                };

                // // Get Missing Dict Keys
                // // Get all unique keys from excelData dictionaries
                // var allKeys = excelData
                //     .SelectMany(dict => dict.Keys)  // Flatten all keys
                //     .Distinct()                     // Remove duplicates
                //     .ToList();
                // // Get column names already in dynamicCols
                // var existingColNames = dynamicCols.Select(col => col.Key).ToList();
                // // Find keys not in dynamicCols (case-insensitive comparison)
                // var missingKeys = allKeys
                //     .Where(key => !existingColNames.Contains(key, StringComparer.OrdinalIgnoreCase))
                //     .ToList();
                // // Add missingKeys to dynamicCols
                // foreach (var key in missingKeys)
                //     dynamicCols.Add(new DynamicExcelColumn(key) { Width = 20 }); // Default width

                // Add Missing Keys for Dynamic Columns
                // **pass dynamicCols and update it (as dynamicCols is reference type)
                AddMissingDynamicColumns(excelData, dynamicCols);


                // Write Excel file
                var memoryStream = new MemoryStream();
                MiniExcel.SaveAs(
                    stream: memoryStream,
                    value: excelData,
                    configuration: new OpenXmlConfiguration
                    {
                        DynamicColumns = dynamicCols.ToArray(),
                    }
                );
                memoryStream.Position = 0; // Reset stream position

                return File(
                    memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report_DepartmentKPI_Summary_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
                );
            }

            // ----------MODE: DETAIL-------------------------------------------
            else if (ViewMode.Equals("detail", StringComparison.OrdinalIgnoreCase))
            {
                // ALL + DETAIL
                UserList = await LoadUserList();
                if (UserList.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "No users found.");
                    return Page();
                }

                AllUserGroupReportDetailList = LoadAllUserGroupDetailList(
                    submissionsByPeriod,
                    UserList, DepartmentList);

                // PREPARE FOR EXCEL FILE
                var colPeriod = "Period";
                var colCandidate = "Candidate";
                var colDepartmentNameList = new List<string>();
                // Row: [Total] [Total] [Total]

                // 
                var excelData = new List<Dictionary<string, object>>();
                excelData = AllUserGroupReportDetailList.Select(submission =>
                {
                    var data = new Dictionary<string, object>()
                    {
                        [colPeriod] = submission.PeriodName ?? "[undefined period]",
                        [colCandidate] = submission.SubmittedBy.FullName ?? "[undefined candidate]",
                    };

                    foreach (var ds in submission.DepartmentScores)
                    {
                        data[$"{ds.DepartmentName}"] = Convert.ToDecimal(ds.ScoreValue.ToString("0.00"));
                    }

                    return data;
                }).ToList();

                // Dynamic Columns
                var dynamicCols = new List<DynamicExcelColumn>
                {
                    new(colPeriod) { Width = 10 },
                    new(colCandidate) { Width = 25 }
                };

                // Add Missing Keys for Dynamic Columns
                // **pass dynamicCols and update it (as dynamicCols is reference type)
                AddMissingDynamicColumns(excelData, dynamicCols);

                // Write Excel file
                var memoryStream = new MemoryStream();
                MiniExcel.SaveAs(
                    stream: memoryStream,
                    value: excelData,
                configuration: new OpenXmlConfiguration
                {
                    DynamicColumns = dynamicCols.ToArray(),
                }
                );
                memoryStream.Position = 0; // Reset stream position

                return File(
                    memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report_DepartmentKPI_Detail_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
                );
            }
        }
        // ----------GROUP: SINGLE----------------------------------------------
        else
        {
            // ----------MODE: SUMMARY------------------------------------------
            if (ViewMode.Equals("summary", StringComparison.OrdinalIgnoreCase))
            {
                // SINGLE + SUMMARY

            }

            // ----------MODE: DETAIL-------------------------------------------
            else if (ViewMode.Equals("detail", StringComparison.OrdinalIgnoreCase))
            {
                // SINGLE + DETAIL

            }
        }
        */


        return RedirectToPage();
    }


    // ========== METHODS ======================================================
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

    private async Task<List<UserViewModel>> LoadUserList(string roleName)
    {
        // var users = await _userService.FindAllActiveAsync(roleName);
        var users = await _userService.FindAllAsync(includeLockedUser: true);

        if (users.IsSuccess && users.Data != null)
        // if (users.Any())
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

    private List<SelectListItem> InitViewModeListItems()
    {
        return new List<SelectListItem>
        {
            new() { Value = "summary", Text = "Summary" },
            new() { Value = "detail", Text = "Detail" }
        };
    }

    // loop Department
    // 

    /// <summary>
    /// Load All User Group Summary List
    /// ** show for all group
    /// </summary>
    /// <param name="submissions"></param>
    /// <param name="DepartmentList"></param>
    /// <returns></returns>
    private List<AllUserGroupReportSummaryViewModel> LoadAllUserGroupSummaryList(
        List<KpiSubmissionDto> submissions,
        List<DepartmentViewModel> DepartmentList)
    {
        // ** loop department to include all department (rows)
        return DepartmentList.Select(department =>
        {
            var submissionToDepartment = submissions
                .Where(s => s.DepartmentId == department.Id)
                .ToList();

            // ** loop Group to get each Group's total submissions/score
            var groupScores = UserGroupList.Select(group =>
            {
                // submissions to [Department] by [Group] -> eg: hod, 34, 272
                var submissions = submissionToDepartment
                    .Where(s => s.SubmittedBy.UserGroup.Id == group.Id)
                    .ToList();

                return new UserGroupSubmissionInfoViewModel
                {
                    GroupName = group.GroupName,
                    TotalSubmissions = submissions.Count,
                    TotalScore = submissions.Sum(e => e.ScoreValue)
                };
            }).ToList();

            // CALCULATE TOTAL SCORE (summarized score) 
            // get from groupScores or submissionToDepartment // -> var totalSubmissions = groupScores.Sum(g => g.TotalSubmissions); // var totalScore = groupScores.Sum(g => g.TotalScore);
            var totalSubmissions = submissionToDepartment.Count;
            var totalScore = submissionToDepartment.Sum(g => g.ScoreValue);
            var kpiScore = (totalSubmissions > 0)
                    ? (totalScore / totalSubmissions) : 0M;

            return new AllUserGroupReportSummaryViewModel
            {
                PeriodName = SelectedPeriod.PeriodName,
                DepartmentName = department.DepartmentName,
                UserGroupSubmissions = groupScores,
                TotalSubmissions = totalSubmissions,
                TotalScore = totalScore,
                KpiScore = kpiScore
            };
        }).ToList();
    }

    /// <summary>
    /// Load Single User Group Summary List
    /// ** only show for selected group
    /// </summary>
    /// <param name="periodName"></param>
    /// <param name="selectedGroupName"></param>
    /// <param name="submissions"></param>
    /// <param name="departmentList"></param>
    /// <returns></returns>
    private static List<SingleUserGroupReportSummaryViewModel> Load_SingleUserGroup_SummaryList(
        string periodName,
        string selectedGroupName,
        List<KpiSubmissionDto> submissionsByPeriod, // or accept ViewModel
        List<DepartmentViewModel> departmentList)
    {
        // filter by selected Group first
        var submissionsByGroup = submissionsByPeriod
            .Where(s => s.SubmittedBy.UserGroup.GroupName
                .Equals(selectedGroupName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // ** loop department to include all department (rows)
        return departmentList.Select(department =>
        {
            var submissionToDepartment = submissionsByGroup
                .Where(submissions => submissions.DepartmentId == department.Id)
                .ToList();

            // CALCULATE TOTAL SCORE (summarized score) 
            // get from groupScores or submissionToDepartment // -> var totalSubmissions = groupScores.Sum(g => g.TotalSubmissions); // var totalScore = groupScores.Sum(g => g.TotalScore);
            var totalSubmissions = submissionToDepartment.Count;
            var totalScore = submissionToDepartment.Sum(g => g.ScoreValue);
            var kpiScore = (totalSubmissions > 0)
                    ? (totalScore / totalSubmissions) : 0M;

            return new SingleUserGroupReportSummaryViewModel
            {
                PeriodName = periodName,
                GroupName = selectedGroupName,
                DepartmentName = department.DepartmentName,
                TotalSubmissions = totalSubmissions,
                TotalScore = totalScore,
                KpiScore = kpiScore
            };
        }).ToList();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="submissionsByPeriod"></param>
    /// <param name="userList"></param>
    /// <param name="departmentList"></param>
    /// <returns></returns>
    private List<AllUserGroupReportDetailViewModel> Load_AllUserGroup_DetailList(
        List<KpiSubmissionDto> submissionsByPeriod,
        List<UserViewModel> userList,
        List<DepartmentViewModel> departmentList)
    {
        // Option 2: return only submitted records
        // filter user who have submitted
        var filteredUsers = userList
            .Where(user => submissionsByPeriod.Any(s => s.SubmitterId == user.Id))
            .ToList();

        return filteredUsers.Select(user =>
        {
            var userSubmissions = submissionsByPeriod
                .Where(s => s.SubmitterId == user.Id)
                // **note: sort by DepartmentName is required
                .OrderBy(s => s.TargetDepartment.DepartmentName)
                .ToList() ?? [];

            // WHY departments??
            // > to get all department in the result 
            //   even if there is no submissions
            //   for some departments 
            var departmentScores = departmentList
                .Select(dpt =>
                {
                    var submission = userSubmissions
                        .Where(s => s.DepartmentId == dpt.Id)
                        .FirstOrDefault();
                    Console.WriteLine("##############################################", dpt.DepartmentName);
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
                // DepartmentScores should includes all departments
                // for consistency (set score to 0.00 if no submision) 
                DepartmentScores = departmentScores
                // DepartmentScores = userSubmissions.Select(submission => new DepartmentScoreViewModel
                // {
                //     DepartmentName = submission.TargetDepartment.DepartmentName,
                //     ScoreValue = submission.ScoreValue
                // }).ToList()
            };
        }).ToList();
    }

    private static List<SingleUserGroupReportDetailViewModel> Load_SingleUserGroup_DetailList(
        string periodName,
        string selectedGroupName,
        List<KpiSubmissionDto> submissionsByPeriod,
        List<UserViewModel> userList,
        List<DepartmentViewModel> departmentList)
    {
        var filteredUsers = userList
            .Where(user => submissionsByPeriod.Any(s => s.SubmitterId == user.Id))
            .ToList();

        return filteredUsers
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
            }).ToList();
    }



    // Excel helper
    /// <summary>
    /// Add Missing Dynamic Columns (for summary view)
    /// - find and add missing keys to dynamicCols based on excelData
    /// - excelData's each row might contains different column count
    /// </summary>
    /// <param name="excelData"></param>
    /// <param name="dynamicCols"></param>
    private static void AddMissingDynamicColumns(
        List<Dictionary<string, object>> excelData,
        List<DynamicExcelColumn> dynamicCols)
    {
        // Get all unique keys from excelData dictionaries
        var allKeys = excelData
            .SelectMany(dict => dict.Keys)  // Flatten all keys
            .Distinct()                     // Remove duplicates
            .ToList();

        // Get column names already in dynamicCols
        var existingColNames = dynamicCols.Select(col => col.Key).ToList();

        // Find keys not in dynamicCols (case-insensitive comparison)
        var missingKeys = allKeys
            .Where(key => !existingColNames.Contains(key, StringComparer.OrdinalIgnoreCase))
            .ToList();

        // Add missingKeys to dynamicCols
        foreach (var key in missingKeys)
        {
            dynamicCols.Add(new DynamicExcelColumn(key) { Width = 20 }); // Default width
        }
    }


    // ========== MODELS =======================================================
    // =========================================================================

    // ----------SUMMARY + ALL -------------------------------------------------
    public class AllUserGroupReportSummaryViewModel
    {
        public string? PeriodName { get; set; }
        public string? DepartmentName { get; set; }
        public List<UserGroupSubmissionInfoViewModel> UserGroupSubmissions { get; set; } = [];
        public long TotalSubmissions { get; set; }
        public decimal TotalScore { get; set; }
        public decimal KpiScore { get; set; }
    }
    public List<AllUserGroupReportSummaryViewModel> AllUserGroupReportSummaryList { get; set; } = [];
    // -------------------------------------------------------------------------

    // public class AllUserGroupReportDetailViewModel
    // {
    //     public string? PeriodName { get; set; }
    //     public UserViewModel SubmittedBy { get; set; } = null!;
    //     public string? DepartmentName { get; set; }
    //     public decimal ScoreValue { get; set; }
    // }

    // DEPARTMENT SCORE for DETAILS VIEW
    // for:  AllUserGroupReportDetailViewModel
    //       SingleUserGroupReportDetailViewModel
    public class DepartmentScoreViewModel
    {
        public string? DepartmentName { get; set; }
        public decimal ScoreValue { get; set; }
        public string? Comment { get; set; }
    }

    // ----------DETAIL + ALL---------------------------------------------------
    public class AllUserGroupReportDetailViewModel
    {
        public string? PeriodName { get; set; }
        public UserViewModel SubmittedBy { get; set; } = null!;
        public List<DepartmentScoreViewModel> DepartmentScores { get; set; } = [];
    }
    public List<AllUserGroupReportDetailViewModel> AllUserGroupReportDetailList { get; set; } = [];


    // ----------SUMMARY + SINGLE---------------------------------------------------------------
    public class SingleUserGroupReportSummaryViewModel
    {
        public string? PeriodName { get; set; }
        public string? GroupName { get; set; }
        public string? DepartmentName { get; set; }
        public long TotalSubmissions { get; set; }
        public decimal TotalScore { get; set; }
        public decimal KpiScore { get; set; }
    }
    public List<SingleUserGroupReportSummaryViewModel> SingleUserGroupReportSummaryList { get; set; } = [];

    // ----------DETAIL + SINGLE---------------------------------------------------------------
    public class SingleUserGroupReportDetailViewModel
    {
        public string? PeriodName { get; set; }
        public UserViewModel SubmittedBy { get; set; } = null!;
        public string? GroupName { get; set; }
        public List<DepartmentScoreViewModel> DepartmentScores { get; set; } = [];
    }
    public List<SingleUserGroupReportDetailViewModel> SingleUserGroupReportDetailList { get; set; } = [];
    // -------------------------------------------------------------------------

    public List<KeyKpiSubmissionViewModel> KeyKpiSubmissions { get; set; } = [];

    public KpiPeriodViewModel SelectedPeriod { get; set; } = null!;

    // ----------Excel Models----------
    // public class KeyKpiSubmissionExportViewModel
    // {
    // }

    // public List<KeyKpiSubmissionExportViewModel>


    public string SelectedPeriodName { get; set; } = null!;

    // public string? Submitter { get; set; }
    public List<DepartmentViewModel> DepartmentList { get; set; } = [];
    public List<UserGroupViewModel> UserGroupList { get; set; } = [];
    public List<UserViewModel> UserList { get; set; } = [];
    // ----------Select/Options Data----------
    [BindProperty]
    public List<SelectListItem> UserGroupListItems { get; set; } = []; // for select element

    [BindProperty(SupportsGet = true)]
    public string? Group { get; set; } // selected item (for filter select element)

    [BindProperty]
    public List<SelectListItem> ViewModeListItems { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? ViewMode { get; set; } // selected item (for filter select element)

}






// ALL + SUMMARY
// LINQ Approach
// loop department
// AllUserGroupReportSummaryList = DepartmentList.Select(department =>
// {
//     // Score received to each Department
//     var submissionToDepartment = allSubmissionsByPeriod
//         .Where(s => s.DepartmentId == department.Id)
//         .ToList();

//     // total submission and score of each group
//     // eg: hod, 34, 272
//     var groupScores = UserGroupList.Select(group =>
//     {
//         var submissionsByGroup = submissionToDepartment
//             .Where(s => s.SubmittedBy.UserTitleId == group.Id)
//             .ToList();

//         // summary score by single group
//         return new UserGroupSubmissionInfoViewModel
//         {
//             GroupName = group.GroupName,
//             TotalSubmissions = submissionsByGroup.Count,
//             TotalScore = submissionsByGroup.Sum(e => e.ScoreValue)
//         };
//     }).ToList();

//     var totalSubmissions = groupScores.Sum(g => g.TotalSubmissions);
//     var totalScore = groupScores.Sum(g => g.TotalScore);

//     return new AllUserGroupReportSummaryViewModel
//     {
//         PeriodName = SelectedPeriod.PeriodName,
//         DepartmentName = department.DepartmentName,
//         UserGroupSubmissions = groupScores,
//         TotalSubmissions = totalSubmissions,
//         TotalScore = totalScore,
//         KpiScore = totalSubmissions > 0
//             ? totalScore / totalSubmissions
//             : 0M
//     };
// }).ToList();

// ALL + SUMMARY
// Foreach Approach
// 1. loop Department
// foreach (var department in DepartmentList)
// {
//     var submissionToDepartment = allSubmissionsByPeriod
//         .Where(s => s.DepartmentId == department.Id)
//         .ToList();

//     // 2. loop Group to get each Group's total submissions/score
//     var userGroupSubmissions = new List<UserGroupSubmissionInfoViewModel>();
//     foreach (var group in UserGroupList)
//     {
//         var submissionsByGroup = submissionToDepartment
//             .Where(s => s.SubmittedBy.UserTitleId == group.Id)
//             .ToList();

//         userGroupSubmissions.Add(new UserGroupSubmissionInfoViewModel
//         {
//             GroupName = group.GroupName,
//             TotalSubmissions = submissionsByGroup.Count,
//             TotalScore = submissionsByGroup.Sum(s => s.ScoreValue)
//         });
//     }

//     // CALCULATE TOTAL SCORE (summarized score) 
//     var totalSubmissions = submissionToDepartment.Count;
//     var totalScore = submissionToDepartment.Sum(e => e.ScoreValue);
//     var kpiScore = (totalSubmissions > 0)
//         ? (totalScore / totalSubmissions)
//         : 0M;

//     AllUserGroupReportSummaryList.Add(new AllUserGroupReportSummaryViewModel
//     {
//         PeriodName = SelectedPeriod.PeriodName,
//         DepartmentName = department.DepartmentName,
//         UserGroupSubmissions = UserGroupList.Select(group =>
//         {
//             var submissionsByGroup = submissionToDepartment
//                 .Where(s => s.SubmittedBy.UserTitleId == group.Id)
//                 .ToList();

//             return new UserGroupSubmissionInfoViewModel
//             {
//                 GroupName = group.GroupName,
//                 TotalSubmissions = submissionsByGroup.Count,
//                 TotalScore = submissionsByGroup.Sum(s => s.ScoreValue)
//             };
//         }).ToList(),
//         TotalSubmissions = totalSubmissions,
//         TotalScore = totalScore,
//         KpiScore = kpiScore
//     });

// }