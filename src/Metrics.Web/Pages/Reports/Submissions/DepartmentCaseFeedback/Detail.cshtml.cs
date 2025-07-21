using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;

namespace Metrics.Web.Pages.Reports.Submissions.DepartmentCaseFeedback;

public class DetailModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;
    private readonly ICaseFeedbackSubmissionService _caseFeedbackService;

    public DetailModel(
        IKpiSubmissionPeriodService kpiPeriodService,
        IUserService userService,
        IDepartmentService departmentService,
        ICaseFeedbackSubmissionService caseFeedbackService)
    {
        _kpiPeriodService = kpiPeriodService;
        _userService = userService;
        _departmentService = departmentService;
        _caseFeedbackService = caseFeedbackService;
    }

    // ========== MODELS =======================================================

    public class CaseFeedbackSubmissionViewModel
    {
        public long Id { get; set; }
        public Guid LookupId { get; set; }
        public string PeriodName { get; set; } = null!;
        public KpiSubmissionPeriodViewModel TargetPeriod { get; set; } = null!;
        public DateTimeOffset SubmittedAt { get; set; }
        // public string SubmitterId { get; set; } = null!;
        // name, department
        public UserViewModel SubmittedBy { get; set; } = null!;

        // case department
        // public long CaseDepartmentId { get; set; }
        public DepartmentViewModel CaseDepartment { get; set; } = null!;
        public decimal NegativeScoreValue { get; set; }

        // Case Info
        public DateTimeOffset IncidentAt { get; set; }
        public string WardName { get; set; } = null!;
        public string CPINumber { get; set; } = null!;
        public string PatientName { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        // Case Info > Details
        public string? Description { get; set; } = string.Empty;
        public string? Comments { get; set; } = string.Empty;
    }
    public List<CaseFeedbackSubmissionViewModel> CaseFeedbackSubmissions { get; set; } = [];

    public class CaseFeedbackExcelExportViewModel
    {
        [ExcelColumnWidth(14)]
        [ExcelColumn(Name = "Period Name")]
        public string? PeriodName { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Submitted By")]
        public string? SubmittedBy { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Case Department")]
        public string CaseDepartment { get; set; } = null!;

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Incident Time")]
        public DateTime IncidentTime { get; set; }

        [ExcelColumnWidth(14)]
        [ExcelColumn(Name = "Given Score")]
        public decimal Score { get; set; }

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Ward Name")]
        public string WardName { get; set; } = null!;

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "CPI Number")]
        public string CPINumber { get; set; } = null!;

        [ExcelColumnWidth(20)]
        [ExcelColumn(Name = "Patient Name")]
        public string PatientName { get; set; } = null!;

        [ExcelColumnWidth(15)]
        [ExcelColumn(Name = "Room Number")]
        public string RoomNumber { get; set; } = null!;

        [ExcelColumnWidth(30)]
        [ExcelColumn(Name = "Case Details")]
        public string? Description { get; set; } = string.Empty;

        [ExcelColumnWidth(30)]
        [ExcelColumn(Name = "Suggestions")]
        public string? Comments { get; set; } = string.Empty;
    }

    public KpiPeriodViewModel SelectedPeriod { get; set; } = null!;
    public string SelectedPeriodName { get; set; } = null!;
    public string? Submitter { get; set; } // for filter by user



    // ========== HANDLERS =======================================================
    public async Task<IActionResult> OnGetAsync(
        [FromRoute] string periodName,
        [FromQuery] string? submitter) // filter by submitter id
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is require.");
            return Page();
        }

        // CHECK periodName exist in submissions
        var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(periodName);
        if (kpiPeriod != null)
        {
            SelectedPeriodName = kpiPeriod.PeriodName;
            SelectedPeriod = new KpiPeriodViewModel() // ---- do we need entire KPI Period object??
            {
                Id = kpiPeriod.Id,
                PeriodName = kpiPeriod.PeriodName,
                SubmissionStartDate = kpiPeriod.SubmissionStartDate,
                SubmissionEndDate = kpiPeriod.SubmissionEndDate
            };
        }
        else
        {
            ModelState.AddModelError("", $"Period {periodName} not found.");
            return Page();
        }

        try
        {
            if (!string.IsNullOrEmpty(submitter))
            {
                // -----Filter by Submitter (id)----------
                // BY USER
                Submitter = submitter.Trim();
                var foundSubmitter = await _userService.FindByIdAsync(submitter.Trim());
                if (foundSubmitter == null)
                {
                    ModelState.AddModelError(string.Empty, $"Submitter with ID {Submitter} not found.");
                    return Page();
                }

                var caseFeedbackSubmissions = await _caseFeedbackService
                    .FindByKpiPeriodAndSubmitterAsync(
                        SelectedPeriod.Id,
                        foundSubmitter.Id);
                // FindByKpiPeriodAndCandidateIdAndKpiPeriodIdAsync(string candidateId, long kpiPeriodId);
                if (caseFeedbackSubmissions.Count > 0)
                {
                    CaseFeedbackSubmissions = ToViewModels(caseFeedbackSubmissions);
                }
            }
            else
            {
                // -----ALL USERS----------
                var caseFeedbackSubmissions = await _caseFeedbackService
                    .FindByKpiPeriodAsync(SelectedPeriod.Id);
                if (caseFeedbackSubmissions.Count > 0)
                {
                    CaseFeedbackSubmissions = ToViewModels(caseFeedbackSubmissions);
                }
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Error fetching department case feedbacks.");
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
        var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(periodName);
        if (kpiPeriod != null)
        {
            SelectedPeriodName = kpiPeriod.PeriodName;
            SelectedPeriod = new KpiPeriodViewModel() // ---- do we need entire KPI Period object??
            {
                Id = kpiPeriod.Id,
                PeriodName = kpiPeriod.PeriodName,
                SubmissionStartDate = kpiPeriod.SubmissionStartDate,
                SubmissionEndDate = kpiPeriod.SubmissionEndDate
            };
        }
        else
        {
            ModelState.AddModelError("", $"Period {periodName} not found.");
            return Page();
        }

        // Fetch Data and map to Model
        var data = new List<CaseFeedbackExcelExportViewModel>();

        var submissions = await _caseFeedbackService.FindByKpiPeriodAsync(SelectedPeriod.Id);
        if (submissions.Count > 0)
        {
            data = submissions.Select(s => new CaseFeedbackExcelExportViewModel
            {
                PeriodName = s.TargetPeriod.PeriodName,
                SubmittedBy = s.SubmittedBy.FullName,

                CaseDepartment = s.CaseDepartment.DepartmentName,
                IncidentTime = s.IncidentAt.LocalDateTime,
                Score = s.NegativeScoreValue,
                WardName = s.WardName,
                CPINumber = s.CPINumber,
                PatientName = s.PatientName,
                RoomNumber = s.RoomNumber,
                Description = s.Description ?? string.Empty,
                Comments = s.Comments ?? string.Empty,
            }).ToList();
        }

        // Export Excel file
        try
        {
            var memoryStream = new MemoryStream();
            MiniExcel.SaveAs(
                stream: memoryStream,
                value: data,
                configuration: new OpenXmlConfiguration
                {
                    // UseExcelColumnName = true, // Use property names as column headers
                    // SheetName = "Case Feedback Submissions",
                    // DateTimeFormat = "dd MMM, yyyy hh:mm tt" // Format for date columns
                    StyleOptions = new OpenXmlStyleOptions
                    {
                        WrapCellContents = true, // Wrap text in cells
                        // DateTimeFormat = "dd MMM, yyyy hh:mm tt", // Format for date columns
                        // UseExcelColumnName = true, // Use property names as column headers
                        // SheetName = "Case Feedback Submissions"
                    }
                }
            );
            memoryStream.Position = 0; // Reset stream position

            return File(
                memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Report_CaseFeedback_{SelectedPeriodName}_Detail_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx" // Added .xlsx extension
            );
        }
        catch (System.Exception)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while exporting the report to Excel.");
            return RedirectToPage();
        }
    }

    // ========== Methods ==================================================
    private static List<CaseFeedbackSubmissionViewModel> ToViewModels(
        List<CaseFeedbackSubmission> entities)
    {
        return entities.Select(s => new CaseFeedbackSubmissionViewModel
        {
            Id = s.Id,
            LookupId = s.LookupId,
            SubmittedAt = s.IncidentAt.ToLocalTime(),

            // SubmitterId = s.SubmitterId,
            SubmittedBy = new UserViewModel
            {
                Id = s.SubmittedBy.Id,
                UserName = s.SubmittedBy.UserName ?? string.Empty,
                FullName = s.SubmittedBy.FullName,
                PhoneNumber = s.SubmittedBy.PhoneNumber,
                ContactAddress = s.SubmittedBy.ContactAddress,
                Department = new DepartmentViewModel
                {
                    Id = s.SubmittedBy.Department.Id,
                    DepartmentCode = s.SubmittedBy.Department.DepartmentCode,
                    DepartmentName = s.SubmittedBy.Department.DepartmentName
                },
                UserGroup = new UserGroupViewModel
                {
                    Id = s.SubmittedBy.UserTitle.Id,
                    GroupCode = s.SubmittedBy.UserTitle.TitleCode,
                    GroupName = s.SubmittedBy.UserTitle.TitleName,
                    Description = s.SubmittedBy.UserTitle.Description
                }
            },
            // CaseDepartmentId = s.CaseDepartmentId,
            CaseDepartment = new DepartmentViewModel
            {
                Id = s.CaseDepartment.Id,
                DepartmentCode = s.CaseDepartment.DepartmentCode,
                DepartmentName = s.CaseDepartment.DepartmentName
            },
            NegativeScoreValue = s.NegativeScoreValue,

            IncidentAt = s.IncidentAt.ToLocalTime(),
            WardName = s.WardName,
            CPINumber = s.CPINumber,
            PatientName = s.PatientName,
            RoomNumber = s.RoomNumber,
            Description = s.Description ?? string.Empty,
            Comments = s.Comments ?? string.Empty
        })
            .ToList();

    }
}

