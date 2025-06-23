using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using System.Threading.Tasks;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentKpi;

public class DetailModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionService _kpiSubmissionService;
    private readonly IUserTitleService _userTitleService;

    public DetailModel(
        UserManager<ApplicationUser> userManager,
        IUserService userService,
        IKpiSubmissionPeriodService kpiPeriodService,
        IDepartmentService departmentService,
        IKpiSubmissionService kpiSubmissionService,
        IUserTitleService userTitleService)
    {
        _userService = userService;
        _kpiPeriodService = kpiPeriodService;
        _departmentService = departmentService;
        _kpiSubmissionService = kpiSubmissionService;
        _userTitleService = userTitleService;
    }

    // =============== MODELS ==================================================
    public class ScoreSubmissionDetailReportViewModel
    {
        public string? PeriodName { get; set; }
        public string? DepartmentName { get; set; }
        public Decimal GivenScore { get; set; }
        public string? UserFullName { get; set; }
        public string? ApplicationUserId { get; set; }
        public string? PositiveAspects { get; set; }
        public string? NegativeAspects { get; set; }
        public string? Comments { get; set; }
        // public string? UserGroupName { get; set; }
        // public long TotalUser { get; set; }
        // public long TotalSubmissions { get; set; }
        // public decimal TotalScore { get; set; }
        // public decimal KpiScore { get; set; }
    }

    public List<ScoreSubmissionDetailReportViewModel> ScoreSubmissionDetailReports { get; set; } = new();

    public KpiPeriodViewModel SelectedPeriod { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? Submitter { get; set; } // for filter by user

    public class ExcelDtoAllUserModel
    {
        [ExcelColumnWidth(15)]
        [ExcelColumn(Name = "Period Name")]
        public string? PeriodName { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Scoring Department")]
        public string? ScoringDepartment { get; set; }

        [ExcelColumnWidth(10)]
        [ExcelColumn(Name = "Score")]
        public decimal GivenScore { get; set; }

        [ExcelColumnWidth(15)]
        [ExcelColumn(Name = "Submitted By")]
        public string? SubmittedBy { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Positive Aspects")]
        public string? PositiveAspects { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Negative Aspects")]
        public string? NegativeAspects { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Comments")]
        public string? Comments { get; set; }
    }
    public class ExcelDtoSingleUserModel
    {
        [ExcelColumnWidth(15)]
        [ExcelColumn(Name = "Period Name")]
        public string? PeriodName { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Scoring Department")]
        public string? ScoringDepartment { get; set; }

        [ExcelColumnWidth(10)]
        [ExcelColumn(Name = "Score")]
        public decimal GivenScore { get; set; }

        [ExcelColumnWidth(15)]
        [ExcelColumn(Name = "Submitted By")]
        public string? SubmittedBy { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Positive Aspects")]
        public string? PositiveAspects { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Negative Aspects")]
        public string? NegativeAspects { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Comments")]
        public string? Comments { get; set; }
    }

    // =============== HANDLERS ================================================

    public async Task<IActionResult> OnGetAsync([FromRoute] string periodName, [FromQuery] string? submitter)
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }

        // CHECK periodName exist in submissions
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


        if (string.IsNullOrEmpty(submitter))
        {
            // BY ALL USERS
            var kpiSubmissions = await _kpiSubmissionService.FindByKpiPeriodAsync(SelectedPeriod.Id);
            if (kpiSubmissions.Any())
            {
                ScoreSubmissionDetailReports = kpiSubmissions.Select(s => new ScoreSubmissionDetailReportViewModel
                {
                    PeriodName = s.TargetPeriod.PeriodName,
                    DepartmentName = s.TargetDepartment.DepartmentName,
                    GivenScore = s.ScoreValue,
                    UserFullName = s.SubmittedBy.FullName,
                    ApplicationUserId = s.SubmittedBy.Id,
                    PositiveAspects = s.PositiveAspects ?? string.Empty,
                    NegativeAspects = s.NegativeAspects ?? string.Empty,
                    Comments = s.Comments ?? string.Empty
                }).ToList();
            }
        }
        else
        {
            // BY USER
            Submitter = submitter.Trim();
            var foundSubmitter = await _userService.FindByIdAsync(Submitter);
            if (foundSubmitter == null)
            {
                ModelState.AddModelError(string.Empty, $"Submitter with ID {Submitter} not found.");
                return Page();
            }
            var kpiSubmissions = await _kpiSubmissionService
                .FindByKpiPeriodAndSubmitterAsync(SelectedPeriod.Id, foundSubmitter.Id);
            // FindByKpiPeriodAndCandidateIdAndKpiPeriodIdAsync(string candidateId, long kpiPeriodId);
            if (kpiSubmissions.Any())
            {
                ScoreSubmissionDetailReports = kpiSubmissions.Select(s => new ScoreSubmissionDetailReportViewModel
                {
                    PeriodName = s.TargetPeriod.PeriodName,
                    DepartmentName = s.TargetDepartment.DepartmentName,
                    GivenScore = s.ScoreValue,
                    UserFullName = s.SubmittedBy.FullName,
                    ApplicationUserId = s.SubmittedBy.Id,
                    PositiveAspects = s.PositiveAspects ?? string.Empty,
                    NegativeAspects = s.NegativeAspects ?? string.Empty,
                    Comments = s.Comments ?? string.Empty
                }).ToList();
            }
        }


        return Page();
    }

    public async Task<IActionResult> OnPostExportExcelAsync(string periodName)
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }

        // CHECK periodName exist in submissions
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


        if (string.IsNullOrEmpty(Submitter))
        {
            // BY ALL USERS
            var kpiSubmissions = await _kpiSubmissionService.FindByKpiPeriodAsync(SelectedPeriod.Id);

            // Prepare for Excel export
            var data = new List<ExcelDtoAllUserModel>();
            if (kpiSubmissions.Any())
            {
                data = kpiSubmissions.Select(s => new ExcelDtoAllUserModel
                {
                    PeriodName = s.TargetPeriod.PeriodName ?? string.Empty,
                    ScoringDepartment = s.TargetDepartment.DepartmentName ?? string.Empty,
                    GivenScore = s.ScoreValue,
                    SubmittedBy = s.SubmittedBy.FullName ?? string.Empty,
                    PositiveAspects = s.PositiveAspects ?? string.Empty,
                    NegativeAspects = s.NegativeAspects ?? string.Empty,
                    Comments = s.Comments ?? string.Empty
                }).ToList();
            }

            // Export Excel file
            try
            {
                var memoryStream = new MemoryStream();
                MiniExcel.SaveAs(
                    stream: memoryStream,
                    value: data
                );
                memoryStream.Position = 0; // Reset stream position

                return File(
                    memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report-Detail-All-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
                );
            }
            catch (System.Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while exporting the report to Excel.");
                return RedirectToPage();
            }
        }
        else
        {
            // BY USER
            var foundSubmitter = await _userService.FindByIdAsync(Submitter);
            if (foundSubmitter == null)
            {
                ModelState.AddModelError(string.Empty, $"Submitter with ID {Submitter} not found.");
                return Page();
            }
            var kpiSubmissions = await _kpiSubmissionService
                .FindByKpiPeriodAndSubmitterAsync(SelectedPeriod.Id, foundSubmitter.Id);
            // FindByKpiPeriodAndCandidateIdAndKpiPeriodIdAsync(string candidateId, long kpiPeriodId);
            var data = new List<ExcelDtoSingleUserModel>();

            // Prepare for Excel export
            if (kpiSubmissions.Any())
            {
                data = kpiSubmissions.Select(s => new ExcelDtoSingleUserModel
                {
                    PeriodName = s.TargetPeriod.PeriodName ?? string.Empty,
                    ScoringDepartment = s.TargetDepartment.DepartmentName ?? string.Empty,
                    GivenScore = s.ScoreValue,
                    SubmittedBy = s.SubmittedBy.FullName ?? string.Empty,
                    PositiveAspects = s.PositiveAspects ?? string.Empty,
                    NegativeAspects = s.NegativeAspects ?? string.Empty,
                    Comments = s.Comments ?? string.Empty
                }).ToList();
            }

            // Export Excel file
            try
            {
                var memoryStream = new MemoryStream();
                MiniExcel.SaveAs(
                    stream: memoryStream,
                    value: data
                );
                memoryStream.Position = 0; // Reset stream position

                return File(
                    memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report-DepartmentScoreSubmission-Detail-{foundSubmitter.FullName}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx" // Added .xlsx extension
                );
            }
            catch (System.Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while exporting the report to Excel.");
                return RedirectToPage();
            }
        }

        // return Page();
    }


    // ========== Methods ==================================================


}
