using Metrics.Application.Authorization;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.DepartmentCaseFeedback;

[Authorize(Policy = ApplicationPolicies.CanGive_Feedback_Policy)]
public class ListModel(
        IUserService userService,
        ICaseFeedbackService caseFeedbackService
) : PageModel
{
    private readonly IUserService _userService = userService;
    private readonly ICaseFeedbackService _caseFeedbackService = caseFeedbackService;

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

    public string? ReturnUrl { get; set; } = string.Empty;


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(string? returnUrl)
    {
        // ----------RETURN URL-------------------------------------------------
        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User is not authenticated.");
        var currentUser = await _userService.FindByIdAsync(userId);
        if (currentUser != null)
        {
            // get all existing submissions
            var existingSubmissions = await _caseFeedbackService
                .FindActiveBySubmitterAsync(currentUser.Id);
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
                            UserCode = s.SubmittedBy.UserCode,
                            UserName = s.SubmittedBy.UserName ?? string.Empty,
                            FullName = s.SubmittedBy.FullName,
                            PhoneNumber = s.SubmittedBy.PhoneNumber,
                            ContactAddress = s.SubmittedBy.ContactAddress,
                            DepartmentId = s.SubmittedBy.DepartmentId,
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
