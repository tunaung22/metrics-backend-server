using Metrics.Application.Common.Mappers;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Metrics.Web.Models.DepartmentKeyMetric;
using Metrics.Web.Models.KeyKpiSubmissionConstraint;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;

namespace Metrics.Web.Pages.Reports.Submissions.KeyKpi;

public class DetailModel(
    IKpiSubmissionPeriodService kpiPeriodService,
    IUserService userService,
    IUserTitleService userGroupService,
    IKeyKpiSubmissionService keyKpiSubmissionService,
    IKeyKpiSubmissionConstraintService submissionConstraintService,
    IDepartmentKeyMetricService departmentKeyMetricService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IUserService _userService = userService;
    private readonly IUserTitleService _userGroupService = userGroupService;
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

        // ----------SUBMISSIONS-------------------------------------------------
        var allSubmissions = await _keyKpiSubmissionService.FindByPeriodAsync(SelectedPeriod.Id);
        // fail or null or present
        if (!allSubmissions.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "Faild to fetch submissions.");
            return Page();
        }
        else if (allSubmissions.Data == null || allSubmissions.Data.Count == 0)
            return Page();
        else
            KeyKpiSubmissions = allSubmissions.Data.Select(s => s.MapToViewModel()).ToList();

        // ----------USER GROUPS------------------------------------------------
        UserGroupList = await LoadUserGroups();
        if (UserGroupList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "User Group is empty");
            return Page();
        }

        // ---------CANDIDATE DEPARTMENT------------------------------------------------
        CandidateDepartmentList = await LoadCandidateDepartmentFromConstraints(SelectedPeriod.Id);
        if (CandidateDepartmentList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "No Candidate Department.");
            return Page();
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
        }
        var KEY_DEPARTMENT__ALL = KeyDepartment.Equals("all", StringComparison.OrdinalIgnoreCase);


        // ----------SUBMISSION CONSTRAINTS-------------------------------------
        var allSubmissionConstraints = await LoadSubmissionConstraintByPeriod(SelectedPeriod.Id);
        // ----------DEPARTMENT KEY METRICS-------------------------------------
        DepartmentKeyMetrics = await LoadDepartmentKeyMetricByPeriod(SelectedPeriod.Id);



        // ----------------------------------------------------------------------
        // ----------ALL KEY ISSUE DEPARTMENT------------------------------------
        // ----------------------------------------------------------------------
        if (KEY_DEPARTMENT__ALL)
        {
            List<DetailReport__Model> detailReportData = [];
            foreach (var keyDpt in KeyIssueDepartmentList)
            {
                var submissionsPerKeyDepartment = allSubmissions.Data
                   .Where(s => s.DepartmentKeyMetric.KeyIssueDepartmentId == keyDpt.Id)
                   .ToList();
                var constraintsPerKeyDepartment = allSubmissionConstraints
                    .Where(c => c.DepartmentKeyMetric.KeyIssueDepartmentId == keyDpt.Id)
                    .ToList();
                var keyMetrics = submissionsPerKeyDepartment
                    .DistinctBy(s => s.DepartmentKeyMetric.KeyMetricId)
                    .OrderBy(s => s.DepartmentKeyMetric.KeyMetric.MetricTitle)
                    .Select(s => s.DepartmentKeyMetric.KeyMetric)
                    .ToList();

                foreach (var keyMetric in keyMetrics)
                {
                    var submissionsOnKey = submissionsPerKeyDepartment
                        .Where(s => s.DepartmentKeyMetric.KeyMetricId == keyMetric.Id)
                        .ToList();

                    // Per User
                    var users = submissionsOnKey.DistinctBy(s => s.SubmittedBy.UserName).Select(s => s.SubmittedBy);
                    foreach (var user in users)
                    {
                        // get candidate's submissions
                        // var cc = submissionsOnKey.Count(s => s.SubmitterId == user.Id);
                        var submissionOnKeyPerUser = submissionsOnKey.First(s => s.SubmitterId == user.Id);

                        var data = new DetailReport__Model
                        {
                            KeyIssueDepartment = new KeyIssueDepartment__Model
                            {
                                KeyIssueDepartmentName = keyDpt.DepartmentName,
                                KeyTitle = keyMetric.MetricTitle,
                            },
                            PeriodName = SelectedPeriod.PeriodName,
                            SubmissionDetails = new SubmissionDetail__Model
                            {
                                CandidateInfo = new Candidate__Model
                                {
                                    CandidateCode = user.UserCode,
                                    CandidateName = user.FullName,
                                    CandidateDepartmentName = user.Department.DepartmentName,
                                    CandidateGroupName = user.UserGroup.GroupName,
                                },
                                ScoreValue = submissionOnKeyPerUser.ScoreValue,
                                Comments = submissionOnKeyPerUser.Comments,
                            }
                        };
                        detailReportData.Add(data);
                    }
                }
            }
            DetailReportData = detailReportData;
        }

        // ----------------------------------------------------------------------
        // ----------SINGLE KEY ISSUE DEPARTMENT---------------------------------------
        // ----------------------------------------------------------------------
        else
        {
            if (Guid.TryParse(KeyDepartment, out Guid departmentCode) == false)
            {
                ModelState.AddModelError(string.Empty, "Invalid Department Code.");
                return Page();
            }
            SelectedKeyDepartment = KeyIssueDepartmentList.FirstOrDefault(d => d.DepartmentCode == departmentCode);

            if (SelectedKeyDepartment != null)
            {
                var submissionsPerKeyDepartment = allSubmissions.Data
                    .Where(s => s.DepartmentKeyMetric.KeyIssueDepartment.DepartmentCode == SelectedKeyDepartment.DepartmentCode)
                    .ToList();
                var constraintsPerKeyDepartment = allSubmissionConstraints
                    .Where(c => c.DepartmentKeyMetric.KeyIssueDepartment.DepartmentCode == SelectedKeyDepartment.DepartmentCode)
                    .ToList();

                // BASED ON SUBMISSIONS RECEIVED
                var keyMetrics = submissionsPerKeyDepartment
                    .DistinctBy(s => s.DepartmentKeyMetric.KeyMetricId)
                    .OrderBy(s => s.DepartmentKeyMetric.KeyMetric.MetricTitle)
                    .Select(s => s.DepartmentKeyMetric.KeyMetric)
                    .ToList();


                List<DetailReport__Model> detailReportData = [];
                foreach (var keyMetric in keyMetrics) // keys
                {
                    var submissionsOnKey = submissionsPerKeyDepartment
                        .Where(s => s.DepartmentKeyMetric.KeyMetricId == keyMetric.Id)
                        .ToList();

                    // Per User
                    var users = submissionsOnKey.DistinctBy(s => s.SubmittedBy.UserName).Select(s => s.SubmittedBy);
                    foreach (var user in users)
                    {
                        // get candidate's submissions
                        // var cc = submissionsOnKey.Count(s => s.SubmitterId == user.Id);
                        var submissionOnKeyPerUser = submissionsOnKey.First(s => s.SubmitterId == user.Id);

                        var data = new DetailReport__Model
                        {
                            KeyIssueDepartment = new KeyIssueDepartment__Model
                            {
                                KeyIssueDepartmentName = SelectedKeyDepartment.DepartmentName,
                                KeyTitle = keyMetric.MetricTitle,
                            },
                            PeriodName = SelectedPeriod.PeriodName,
                            SubmissionDetails = new SubmissionDetail__Model
                            {
                                CandidateInfo = new Candidate__Model
                                {
                                    CandidateCode = user.UserCode,
                                    CandidateName = user.FullName,
                                    CandidateDepartmentName = user.Department.DepartmentName,
                                    CandidateGroupName = user.UserGroup.GroupName,
                                },
                                ScoreValue = submissionOnKeyPerUser.ScoreValue,
                                Comments = submissionOnKeyPerUser.Comments,
                            }
                        };
                        detailReportData.Add(data);
                    }
                }
                DetailReportData = detailReportData;

            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unknown selected key issue department.");
            }
        }
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

        // ----------SUBMISSIONS-------------------------------------------------
        var allSubmissions = await _keyKpiSubmissionService.FindByPeriodAsync(SelectedPeriod.Id);
        // fail or null or present
        if (!allSubmissions.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, "Faild to fetch submissions.");
            return Page();
        }
        else if (allSubmissions.Data == null || allSubmissions.Data.Count == 0)
            return Page();
        else
            KeyKpiSubmissions = allSubmissions.Data.Select(s => s.MapToViewModel()).ToList();

        // ----------USER GROUPS------------------------------------------------
        UserGroupList = await LoadUserGroups();
        if (UserGroupList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "User Group is empty");
            return Page();
        }

        // ---------CANDIDATE DEPARTMENT------------------------------------------------
        CandidateDepartmentList = await LoadCandidateDepartmentFromConstraints(SelectedPeriod.Id);
        if (CandidateDepartmentList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "No Candidate Department.");
            return Page();
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
        }
        var KEY_DEPARTMENT__ALL = KeyDepartment.Equals("all", StringComparison.OrdinalIgnoreCase);


        // ----------SUBMISSION CONSTRAINTS-------------------------------------
        var allSubmissionConstraints = await LoadSubmissionConstraintByPeriod(SelectedPeriod.Id);
        // ----------DEPARTMENT KEY METRICS-------------------------------------
        DepartmentKeyMetrics = await LoadDepartmentKeyMetricByPeriod(SelectedPeriod.Id);

        // ----------------------------------------------------------------------
        // ----------ALL KEY ISSUE DEPARTMENT------------------------------------
        // ----------------------------------------------------------------------
        if (KEY_DEPARTMENT__ALL)
        {
            List<DetailReport__Model> detailReportData = [];
            foreach (var keyDpt in KeyIssueDepartmentList)
            {
                var submissionsPerKeyDepartment = allSubmissions.Data
                   .Where(s => s.DepartmentKeyMetric.KeyIssueDepartmentId == keyDpt.Id)
                   .ToList();
                var constraintsPerKeyDepartment = allSubmissionConstraints
                    .Where(c => c.DepartmentKeyMetric.KeyIssueDepartmentId == keyDpt.Id)
                    .ToList();
                var keyMetrics = submissionsPerKeyDepartment
                    .DistinctBy(s => s.DepartmentKeyMetric.KeyMetricId)
                    .OrderBy(s => s.DepartmentKeyMetric.KeyMetric.MetricTitle)
                    .Select(s => s.DepartmentKeyMetric.KeyMetric)
                    .ToList();

                foreach (var keyMetric in keyMetrics)
                {
                    var submissionsOnKey = submissionsPerKeyDepartment
                        .Where(s => s.DepartmentKeyMetric.KeyMetricId == keyMetric.Id)
                        .ToList();

                    // Per User
                    var users = submissionsOnKey.DistinctBy(s => s.SubmittedBy.UserName).Select(s => s.SubmittedBy);
                    foreach (var user in users)
                    {
                        var submissionOnKeyPerUser = submissionsOnKey.First(s => s.SubmitterId == user.Id);
                        var data = new DetailReport__Model
                        {
                            KeyIssueDepartment = new KeyIssueDepartment__Model
                            {
                                KeyIssueDepartmentName = keyDpt.DepartmentName,
                                KeyTitle = keyMetric.MetricTitle,
                            },
                            PeriodName = SelectedPeriod.PeriodName,
                            SubmissionDetails = new SubmissionDetail__Model
                            {
                                CandidateInfo = new Candidate__Model
                                {
                                    CandidateCode = user.UserCode,
                                    CandidateName = user.FullName,
                                    CandidateDepartmentName = user.Department.DepartmentName,
                                    CandidateGroupName = user.UserGroup.GroupName,
                                },
                                ScoreValue = submissionOnKeyPerUser.ScoreValue,
                                Comments = submissionOnKeyPerUser.Comments,
                            }
                        };
                        detailReportData.Add(data);
                    }
                }
            }
            DetailReportData = detailReportData;
        }

        // ----------------------------------------------------------------------
        // ----------SINGLE KEY ISSUE DEPARTMENT---------------------------------------
        // ----------------------------------------------------------------------
        else
        {
            if (Guid.TryParse(KeyDepartment, out Guid departmentCode) == false)
            {
                ModelState.AddModelError(string.Empty, "Invalid Department Code.");
                return Page();
            }
            SelectedKeyDepartment = KeyIssueDepartmentList.FirstOrDefault(d => d.DepartmentCode == departmentCode);

            if (SelectedKeyDepartment != null)
            {
                var submissionsPerKeyDepartment = allSubmissions.Data
                    .Where(s => s.DepartmentKeyMetric.KeyIssueDepartment.DepartmentCode == SelectedKeyDepartment.DepartmentCode)
                    .ToList();
                var constraintsPerKeyDepartment = allSubmissionConstraints
                    .Where(c => c.DepartmentKeyMetric.KeyIssueDepartment.DepartmentCode == SelectedKeyDepartment.DepartmentCode)
                    .ToList();

                // BASED ON SUBMISSIONS RECEIVED
                var keyMetrics = submissionsPerKeyDepartment
                    .DistinctBy(s => s.DepartmentKeyMetric.KeyMetricId)
                    .OrderBy(s => s.DepartmentKeyMetric.KeyMetric.MetricTitle)
                    .Select(s => s.DepartmentKeyMetric.KeyMetric)
                    .ToList();

                List<DetailReport__Model> detailReportData = [];
                foreach (var keyMetric in keyMetrics) // keys
                {
                    var submissionsOnKey = submissionsPerKeyDepartment
                        .Where(s => s.DepartmentKeyMetric.KeyMetricId == keyMetric.Id)
                        .ToList();

                    // Per User
                    var users = submissionsOnKey.DistinctBy(s => s.SubmittedBy.UserName).Select(s => s.SubmittedBy);
                    foreach (var user in users)
                    {
                        var submissionOnKeyPerUser = submissionsOnKey.First(s => s.SubmitterId == user.Id);
                        var data = new DetailReport__Model
                        {
                            KeyIssueDepartment = new KeyIssueDepartment__Model
                            {
                                KeyIssueDepartmentName = SelectedKeyDepartment.DepartmentName,
                                KeyTitle = keyMetric.MetricTitle,
                            },
                            PeriodName = SelectedPeriod.PeriodName,
                            SubmissionDetails = new SubmissionDetail__Model
                            {
                                CandidateInfo = new Candidate__Model
                                {
                                    CandidateCode = user.UserCode,
                                    CandidateName = user.FullName,
                                    CandidateDepartmentName = user.Department.DepartmentName,
                                    CandidateGroupName = user.UserGroup.GroupName,
                                },
                                ScoreValue = submissionOnKeyPerUser.ScoreValue,
                                Comments = submissionOnKeyPerUser.Comments,
                            }
                        };
                        detailReportData.Add(data);
                    }
                }
                DetailReportData = detailReportData;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unknown selected key issue department.");
            }
        }


        // ==========EXCEL========================================================================
        string excelFileName = "";
        var memStream = new MemoryStream();

        KEY_DEPARTMENT__ALL = KeyDepartment.Equals("all", StringComparison.OrdinalIgnoreCase);
        if (KEY_DEPARTMENT__ALL)
        {
            excelFileName = $"Report_KeyKPI_Detail_All_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";
            memStream = await Prepare_DetailReport_AllKeyDepartmentData(DetailReportData);
        }
        else
        {
            excelFileName = $"Report_KeyKPI_Detail_SingleKeyDepartment_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";
            memStream = await Prepare_DetailReport_AllKeyDepartmentData(DetailReportData);
        }

        return File(
            memStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            excelFileName
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

    private static async Task<MemoryStream> Prepare_DetailReport_AllKeyDepartmentData(List<DetailReport__Model> reportData)
    {
        List<Dictionary<string, object>> excelData = [];

        var colPeriod = "Period Name";
        var colKeyDepartment = "Key IssueDepartments";
        var colKpiKey = "KPI Key";
        var colCandidateCode = "Candidate Code";
        var colCandidateName = "Candidate Name";
        var colCandidateDepartment = "Candidate Department";
        var colCandidateGroup = "Candidate Group";
        var colScore = "Score";
        var colComment = "Comment";

        var dynamicCols = new List<DynamicExcelColumn>();
        dynamicCols = new List<DynamicExcelColumn>()
            .Concat(
            [
                new(colPeriod) { Width = 20 },
                new(colKeyDepartment) { Width = 25 },
                new(colKpiKey) { Width = 38 },
                new(colCandidateCode) { Width = 20 },
                new(colCandidateName) { Width = 20 },
                new(colCandidateDepartment) { Width = 20 },
                new(colCandidateGroup) { Width = 20 },
                new(colScore) { Width = 20 },
                new(colComment) { Width = 20 },
            ]).ToList();

        foreach (var item in reportData)
        {
            var excelRow = new Dictionary<string, object>();
            excelRow[colPeriod] = item.PeriodName;
            excelRow[colKeyDepartment] = item.KeyIssueDepartment.KeyIssueDepartmentName;
            excelRow[colKpiKey] = item.KeyIssueDepartment.KeyTitle;
            excelRow[colCandidateCode] = item.SubmissionDetails.CandidateInfo.CandidateCode;
            excelRow[colCandidateName] = item.SubmissionDetails.CandidateInfo.CandidateName;
            excelRow[colCandidateDepartment] = item.SubmissionDetails.CandidateInfo.CandidateDepartmentName;
            excelRow[colCandidateGroup] = item.SubmissionDetails.CandidateInfo.CandidateGroupName;
            excelRow[colScore] = item.SubmissionDetails.ScoreValue;
            excelRow[colComment] = item.SubmissionDetails.Comments ?? string.Empty;
            excelData.Add(excelRow);
        }

        var newStream = new MemoryStream();
        await MiniExcel.SaveAsAsync(
            stream: newStream,
            value: excelData,
            configuration: new OpenXmlConfiguration
            {
                DynamicColumns = [.. dynamicCols],
            }
        );
        newStream.Position = 0;

        return newStream;
    }


    // ========== Binding ======================================================
    // ---Period---
    public string SelectedPeriodName { get; set; } = null!;
    public KpiPeriodViewModel SelectedPeriod { get; set; } = new();

    // ----------SELECTION ITEMS-------------------------------------------------
    // ---KeyIssue Department---
    [BindProperty]
    public List<SelectListItem> KeyIssueDepartmentSelectItems { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public required string KeyDepartment { get; set; }

    // ---Candidate Department---
    // [BindProperty]
    // public List<SelectListItem> CandidateDepartmentSelectItems { get; set; } = [];
    // [BindProperty(SupportsGet = true)]
    // public required string CandidateDepartment { get; set; }
    // --------------------------------------------------------------------------
    public List<DepartmentViewModel> KeyIssueDepartmentList { get; set; } = [];
    public DepartmentViewModel? SelectedKeyDepartment { get; set; } = null;
    public List<DepartmentViewModel> CandidateDepartmentList { get; set; } = [];
    public List<KeyKpiSubmissionViewModel> KeyKpiSubmissions { get; set; } = [];
    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];
    public List<UserGroupViewModel> UserGroupList { get; set; } = [];
    // --------------------------------------------------------------------------


    // ========== MODELS CLASSES ===============================================
    public List<DetailReport__Model> DetailReportData { get; set; } = [];

    public class DetailReport__Model
    {
        public string PeriodName { get; set; } = null!;
        public KeyIssueDepartment__Model KeyIssueDepartment { get; set; } = null!;
        public SubmissionDetail__Model SubmissionDetails { get; set; } = null!;

    }

    public class KeyIssueDepartment__Model // key department, key title
    {
        public string KeyIssueDepartmentName { get; set; } = null!;
        public string KeyTitle { get; set; } = null!;
    }

    public class SubmissionDetail__Model
    {
        public Candidate__Model CandidateInfo { get; set; } = null!;
        public decimal ScoreValue { get; set; }
        public string? Comments { get; set; }
    }

    public class Candidate__Model
    {
        public string CandidateCode { get; set; } = null!;
        public string CandidateName { get; set; } = null!;
        public string CandidateDepartmentName { get; set; } = null!;
        public string CandidateGroupName { get; set; } = null!;
    }
}
