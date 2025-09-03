using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.DepartmentCaseFeedback;

[Authorize(Policy = "CanSubmitCaseFeedbackPolicy")]
public class ListModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly ICaseFeedbackService _caseFeedbackService;

    public ListModel(
        IUserService userService,
        IKpiSubmissionPeriodService kpiPeriodService,
        ICaseFeedbackService caseFeedbackService)
    {
        _userService = userService;
        _kpiPeriodService = kpiPeriodService;
        _caseFeedbackService = caseFeedbackService;
    }


    // =============== MODELS ==================================================
    public class CaseFeedbackSubmissionViewModel
    {
        public long Id { get; set; }
        public Guid LookupId { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
        // public decimal NegativeScoreValue { get; set; }
        public string SubmitterId { get; set; } = null!;
        public UserViewModel SubmittedBy { get; set; } = null!;
        // Case Info
        public long CaseDepartmentId { get; set; }
        public DepartmentViewModel CaseDepartment { get; set; } = null!;
        public string WardName { get; set; } = null!;
        public string CPINumber { get; set; } = null!;
        public string PatientName { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        public DateTimeOffset IncidentAt { get; set; }
        // Case Info > Details
        public string? Description { get; set; } = string.Empty;
        // public string? Comments { get; set; } = string.Empty;
    }
    public List<CaseFeedbackSubmissionViewModel> ExistingCaseFeedbackSubmissions { get; set; } = [];

    public string SelectedPeriodName { get; set; } = null!;

    public string? ReturnUrl { get; set; } = string.Empty;

    public KpiPeriodViewModel SelectedPeriod { get; set; } = null!;

    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(
        [FromRoute] string periodName,
        string? returnUrl)
    {
        // ----------RETURN URL-------------------------------------------------
        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        // ----------KPI PERIOD-------------------------------------------------
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


        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User is not authenticated.");
        var currentUser = await _userService.FindByIdAsync(userId);
        if (currentUser != null)
        {
            // get all existing submissions
            var existingSubmissions = await _caseFeedbackService
                .FindByKpiPeriodAndSubmitterAsync(SelectedPeriod.Id, currentUser.Id);
            // .FindByKpiPeriodAsync(SelectedPeriod.Id);
            if (existingSubmissions.Count > 0)
            {
                // ExistingCaseFeedbackSubmissions = existingSubmissions
                ExistingCaseFeedbackSubmissions = existingSubmissions
                    .Select(s => new CaseFeedbackSubmissionViewModel
                    {
                        Id = s.Id,
                        LookupId = s.LookupId,
                        SubmittedAt = s.SubmittedAt.ToLocalTime(),

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
                        // NegativeScoreValue = s.NegativeScoreValue,

                        IncidentAt = s.IncidentAt.ToLocalTime(),
                        WardName = s.WardName,
                        CPINumber = s.CPINumber,
                        PatientName = s.PatientName,
                        RoomNumber = s.RoomNumber,
                        Description = s.Description ?? string.Empty,
                        // Comments = s.Comments ?? string.Empty
                    }).ToList();
            }
        }

        return Page();
    }

    // ========== METHODS ======================================================

}
