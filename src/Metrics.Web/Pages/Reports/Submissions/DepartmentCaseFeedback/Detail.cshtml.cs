using Metrics.Application.Common.Mappers;
using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Metrics.Web.Models.ExcelExportModels;
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
    private readonly ICaseFeedbackScoreSubmissionService _caseFeedbackScoreSubmissionService;

    public DetailModel(
        IKpiSubmissionPeriodService kpiPeriodService,
        IUserService userService,
        IDepartmentService departmentService,
        ICaseFeedbackScoreSubmissionService caseFeedbackScoreSubmissionService)
    {
        _kpiPeriodService = kpiPeriodService;
        _userService = userService;
        _departmentService = departmentService;
        _caseFeedbackScoreSubmissionService = caseFeedbackScoreSubmissionService;
    }

    // ========== MODELS =======================================================

    // public class CaseFeedbackScoreSubmissionViewModel
    // {
    //     public long Id { get; set; }
    //     public Guid LookupId { get; set; }
    //     public string PeriodName { get; set; } = null!;
    //     public KpiPeriodViewModel TargetPeriod { get; set; } = null!;
    //     public DateTimeOffset SubmittedAt { get; set; }
    //     // public string SubmitterId { get; set; } = null!;
    //     // name, department
    //     public UserViewModel SubmittedBy { get; set; } = null!;

    //     // case department
    //     // public long CaseDepartmentId { get; set; }
    //     public DepartmentViewModel CaseDepartment { get; set; } = null!;
    //     public decimal NegativeScoreValue { get; set; }

    //     // Case Info
    //     public DateTimeOffset IncidentAt { get; set; }
    //     public string WardName { get; set; } = null!;
    //     public string CPINumber { get; set; } = null!;
    //     public string PatientName { get; set; } = null!;
    //     public string RoomNumber { get; set; } = null!;
    //     // Case Info > Details
    //     public string? Description { get; set; } = string.Empty;
    //     public string? Comments { get; set; } = string.Empty;
    // }
    public List<CaseFeedbackScoreSubmissionViewModel> CaseFeedbackSubmissions { get; set; } = [];

    // public class CaseFeedbackExcelViewModel
    // {
    //     [ExcelColumnWidth(14)]
    //     [ExcelColumn(Name = "Period Name")]
    //     public string? PeriodName { get; set; }

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Submitted By")]
    //     public string? SubmittedBy { get; set; }

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Case Department")]
    //     public string CaseDepartment { get; set; } = null!;

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Incident Time")]
    //     public DateTime IncidentTime { get; set; }

    //     [ExcelColumnWidth(14)]
    //     [ExcelColumn(Name = "Given Score")]
    //     public decimal Score { get; set; }

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Ward Name")]
    //     public string WardName { get; set; } = null!;

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "CPI Number")]
    //     public string CPINumber { get; set; } = null!;

    //     [ExcelColumnWidth(20)]
    //     [ExcelColumn(Name = "Patient Name")]
    //     public string PatientName { get; set; } = null!;

    //     [ExcelColumnWidth(15)]
    //     [ExcelColumn(Name = "Room Number")]
    //     public string RoomNumber { get; set; } = null!;

    //     [ExcelColumnWidth(30)]
    //     [ExcelColumn(Name = "Case Details")]
    //     public string? Description { get; set; } = string.Empty;

    //     [ExcelColumnWidth(30)]
    //     [ExcelColumn(Name = "Suggestions")]
    //     public string? Comments { get; set; } = string.Empty;
    // }

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
            // if (!string.IsNullOrEmpty(submitter))
            // {
            //     // -----Filter by Submitter (id)----------
            //     // BY USER
            //     Submitter = submitter.Trim();
            //     var foundSubmitter = await _userService.FindByIdAsync(submitter.Trim());
            //     if (foundSubmitter == null)
            //     {
            //         ModelState.AddModelError(string.Empty, $"Submitter with ID {Submitter} not found.");
            //         return Page();
            //     }

            //     var submissions = await _caseFeedbackScoreSubmissionService
            //         .FindByKpiPeriodAndSubmitterAsync(
            //             SelectedPeriod.Id,
            //             foundSubmitter.Id);
            //     // FindByKpiPeriodAndCandidateIdAndKpiPeriodIdAsync(string candidateId, long kpiPeriodId);
            //     if (submissions.IsSuccess && submissions.Data != null)
            //     {
            //         CaseFeedbackSubmissions = submissions.Data
            //             .Select(s => s.MapToViewModel())
            //             .ToList();
            //     }
            // }
            // else
            {
                // -----ALL USERS----------
                var submissions = await _caseFeedbackScoreSubmissionService
                    .FindByKpiPeriodAsync(SelectedPeriod.Id);
                if (submissions.IsSuccess && submissions.Data != null)
                {
                    CaseFeedbackSubmissions = submissions.Data
                        .Select(s => s.MapToViewModel())
                        .ToList();
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
        var data = new List<CaseFeedbackExcelViewModel>();

        var submissions = await _caseFeedbackScoreSubmissionService.FindByKpiPeriodAsync(SelectedPeriod.Id);

        if (!submissions.IsSuccess || submissions.Data == null)
        {
            ModelState.AddModelError("", $"Failed to load submissions.");
            return Page();
        }

        // Export Excel file
        try
        {
            data = submissions.Data.Select(s => s.MapToExcelViewModel()).ToList();

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
    // private static List<CaseFeedbackScoreSubmissionViewModel> ToViewModels(
    //     List<CaseFeedbackScoreSubmission> entities)
    // {
    //     return entities.Select(s => new CaseFeedbackScoreSubmissionViewModel
    //     {
    //         Id = s.Id,
    //         LookupId = s.LookupId,
    //         SubmittedAt = s.Feedback.IncidentAt.ToLocalTime(),

    //         // SubmitterId = s.SubmitterId,
    //         SubmittedBy = new UserViewModel
    //         {
    //             Id = s.SubmittedBy.Id,
    //             UserName = s.SubmittedBy.UserName ?? string.Empty,
    //             FullName = s.SubmittedBy.FullName,
    //             PhoneNumber = s.SubmittedBy.PhoneNumber,
    //             ContactAddress = s.SubmittedBy.ContactAddress,
    //             Department = new DepartmentViewModel
    //             {
    //                 Id = s.SubmittedBy.Department.Id,
    //                 DepartmentCode = s.SubmittedBy.Department.DepartmentCode,
    //                 DepartmentName = s.SubmittedBy.Department.DepartmentName
    //             },
    //             UserGroup = new UserGroupViewModel
    //             {
    //                 Id = s.SubmittedBy.UserTitle.Id,
    //                 GroupCode = s.SubmittedBy.UserTitle.TitleCode,
    //                 GroupName = s.SubmittedBy.UserTitle.TitleName,
    //                 Description = s.SubmittedBy.UserTitle.Description
    //             }
    //         },
    //         // CaseDepartmentId = s.CaseDepartmentId,
    //         CaseDepartment = new DepartmentViewModel
    //         {
    //             Id = s.Feedback.CaseDepartment.Id,
    //             DepartmentCode = s.Feedback.CaseDepartment.DepartmentCode,
    //             DepartmentName = s.Feedback.CaseDepartment.DepartmentName
    //         },
    //         NegativeScoreValue = s.NegativeScoreValue,

    //         IncidentAt = s.Feedback.IncidentAt.ToLocalTime(),
    //         WardName = s.Feedback.WardName,
    //         CPINumber = s.Feedback.CPINumber,
    //         PatientName = s.Feedback.PatientName,
    //         RoomNumber = s.Feedback.RoomNumber,
    //         Description = s.Feedback.Description ?? string.Empty,
    //         Comments = s.Comments ?? string.Empty
    //     })
    //         .ToList();

    // }
}

