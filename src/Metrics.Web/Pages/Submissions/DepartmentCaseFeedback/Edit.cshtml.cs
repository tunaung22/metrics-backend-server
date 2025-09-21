using Metrics.Application.Authorization;
using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.DepartmentCaseFeedback;

[Authorize(Policy = ApplicationPolicies.CanGiveFeedbackPolicy)]
public class EditModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;
    // private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly ICaseFeedbackService _caseFeedbackSubmissionService;

    public EditModel(
        IUserService userService,
        IDepartmentService departmentService,
        // IKpiSubmissionPeriodService kpiPeriodService,
        ICaseFeedbackService caseFeedbackSubmissionService)
    {
        _userService = userService;
        _departmentService = departmentService;
        // _kpiPeriodService = kpiPeriodService;
        _caseFeedbackSubmissionService = caseFeedbackSubmissionService;

    }
    // =============== MODELS ==================================================
    public class FormInputModel
    {
        public long SubmissionPeriodId { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
        public string SubmitterId { get; set; } = null!; // Foreign Keys

        // [Required(ErrorMessage = "Score is required")]
        // [Range(-5, -1, ErrorMessage = "Score must be between -1 to -5")]
        // public decimal ScoreValue { get; set; } // **note: Negative value

        [Required(ErrorMessage = "Department is required")]
        public long CaseDepartmentId { get; set; } // Foreign Keys

        public DateTimeOffset IncidentAt { get; set; }

        [Required(ErrorMessage = "Ward Name is required")]
        public string WardName { get; set; } = null!;

        [Required(ErrorMessage = "CPI Number is required")]
        public string CPINumber { get; set; } = null!; // Common Patient Identifier or Central/Clinical Patient Index

        [Required(ErrorMessage = "Patient Name is required")]
        public string PatientName { get; set; } = null!;

        [Required(ErrorMessage = "Room Number is required")]
        public string RoomNumber { get; set; } = null!;
        public string? Description { get; set; } = string.Empty; // Case Details
        // public string? Comments { get; set; } = string.Empty; // Additional Notes
    }

    [BindProperty]
    public FormInputModel FormInput { get; set; } = new();

    // public decimal[] ScoreValues { get; } = [-1M, -2M, -3M, -4M, -5M];
    // public decimal[] ScoreValues { get; } = [];

    public List<SelectListItem> DepartmentListItems { get; set; } = null!;

    public UserViewModel Submitter { get; set; } = null!;
    public string? CurrentUserGroupName { get; set; } = string.Empty;
    // public KpiPeriodViewModel SelectedPeriod { get; set; } = null!;

    // [BindProperty]
    // public string? SelectedPeriodName { get; set; } = string.Empty;

    [BindProperty]
    public string? TargetSubmissionLookupId { get; set; } = string.Empty;


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(
        // [FromRoute] string periodName,
        [FromRoute] string lookupId)
    {
        // if (string.IsNullOrEmpty(periodName) || string.IsNullOrEmpty(lookupId))
        // {

        //     // Can't process form wihout lookupId & periodName, 
        //     // return to List instead.
        //     return RedirectToPage("./List", new { periodName = SelectedPeriodName });
        // }

        // // ----------KPI PERIOD-------------------------------------------------
        // var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(periodName);
        // if (kpiPeriod != null)
        // {
        //     SelectedPeriodName = kpiPeriod.PeriodName;
        //     SelectedPeriod = new KpiPeriodViewModel() // ---- do we need entire KPI Period object??
        //     {
        //         Id = kpiPeriod.Id,
        //         PeriodName = kpiPeriod.PeriodName,
        //         SubmissionStartDate = kpiPeriod.SubmissionStartDate,
        //         SubmissionEndDate = kpiPeriod.SubmissionEndDate
        //     };
        // }
        // else
        // {
        //     // ModelState.AddModelError("", $"Period {periodName} not found.");
        //     // return Page();
        //     return RedirectToPage("./List", new { periodName = SelectedPeriodName });
        // }

        // ----------DEPARTMENT-------------------------------------------------
        DepartmentListItems = await LoadDepartmentList();

        // ----------LOOKUP ID--------------------------------------------------
        // ----------GET EXISTING SUBMISSION------------------------------------
        TargetSubmissionLookupId = lookupId;
        var submission = await _caseFeedbackSubmissionService
            .FindByLookupIdAsync(TargetSubmissionLookupId);
        if (submission == null)
        {
            // Return to List if no submission found
            // return RedirectToPage("./List", new { periodName = SelectedPeriodName });
            return RedirectToPage("./List");
        }

        Submitter = new UserViewModel
        {
            Id = submission.SubmittedBy.Id,
            UserCode = submission.SubmittedBy.UserCode,
            UserName = submission.SubmittedBy.UserName!,
            FullName = submission.SubmittedBy.FullName,
            PhoneNumber = submission.SubmittedBy.PhoneNumber,
            ContactAddress = submission.SubmittedBy.ContactAddress,
            UserGroup = new UserGroupViewModel
            {
                Id = submission.SubmittedBy.UserTitle.Id,
                GroupCode = submission.SubmittedBy.UserTitle.TitleCode,
                GroupName = submission.SubmittedBy.UserTitle.TitleName,
                Description = submission.SubmittedBy.UserTitle.Description
            },
            Department = new DepartmentViewModel
            {
                Id = submission.SubmittedBy.Department.Id,
                DepartmentCode = submission.SubmittedBy.Department.DepartmentCode,
                DepartmentName = submission.SubmittedBy.Department.DepartmentName
            }
        };
        CurrentUserGroupName = Submitter.UserGroup.GroupName;

        // ----------FORM INPUT-------------------------------------------------
        FormInput = new FormInputModel
        {
            // SubmissionPeriodId = submission.KpiSubmissionPeriodId,
            SubmittedAt = submission.SubmittedAt,
            SubmitterId = submission.SubmitterId,
            // ScoreValue = submission.NegativeScoreValue,
            CaseDepartmentId = submission.CaseDepartmentId,
            IncidentAt = submission.IncidentAt,
            WardName = submission.WardName,
            CPINumber = submission.CPINumber,
            PatientName = submission.PatientName,
            RoomNumber = submission.RoomNumber,
            Description = submission.Description ?? string.Empty,
            // Comments = submission.Comments ?? string.Empty
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        //  [FromRoute] string periodName,
        [FromRoute] string lookupId)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // if (string.IsNullOrEmpty(periodName) || string.IsNullOrEmpty(lookupId))
        // {
        //     // Can't process form wihout lookupId & periodName, 
        //     // return to List instead.
        //     return RedirectToPage("./List", new { periodName = SelectedPeriodName });
        // }

        // // ----------KPI PERIOD-------------------------------------------------
        // var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(periodName);
        // if (kpiPeriod != null)
        // {
        //     SelectedPeriodName = kpiPeriod.PeriodName;
        //     SelectedPeriod = new KpiPeriodViewModel() // ---- do we need entire KPI Period object??
        //     {
        //         Id = kpiPeriod.Id,
        //         PeriodName = kpiPeriod.PeriodName,
        //         SubmissionStartDate = kpiPeriod.SubmissionStartDate,
        //         SubmissionEndDate = kpiPeriod.SubmissionEndDate
        //     };
        // }
        // else
        // {
        //     // ModelState.AddModelError("", $"Period {periodName} not found.");
        //     // return Page();
        //     return RedirectToPage("./List", new { periodName = SelectedPeriodName });
        // }

        // ----------DEPARTMENT-------------------------------------------------
        DepartmentListItems = await LoadDepartmentList();

        // ----------LOOKUP ID--------------------------------------------------
        TargetSubmissionLookupId = lookupId;

        // ----------SUBMIT THE FORM--------------------------------------------
        try
        {
            var entity = CaseFeedback.Create(
                // kpiSubmissionPeriodId: SelectedPeriod.Id,
                submittedAt: DateTimeOffset.UtcNow,
                submitterId: FormInput.SubmitterId,
                caseDepartmentId: FormInput.CaseDepartmentId,
                wardName: FormInput.WardName,
                cPINumber: FormInput.CPINumber,
                patientName: FormInput.PatientName,
                roomNumber: FormInput.RoomNumber,
                incidentAt: FormInput.IncidentAt.UtcDateTime,
                description: FormInput.Description,
                isDeleted: false
            );

            await _caseFeedbackSubmissionService.UpdateAsync(TargetSubmissionLookupId, entity);

            // return RedirectToPage("./List", new { periodName = SelectedPeriodName });
            return RedirectToPage("./List");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Submission failed.");
            return Page();
        }
    }

    public IActionResult OnPostCancel()
    {
        // return RedirectToPage("./List", new { periodName = SelectedPeriodName });
        return RedirectToPage("./List");
    }

    // ========== METHODS ======================================================
    private async Task<UserViewModel?> GetCurrentUser()
    {
        // Less likely to cause user not found, so throw just in case
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("User not found. Please login again.");

        var user = await _userService.FindByIdAsync(userId);
        if (user != null)
        {
            return new UserViewModel
            {
                Id = user.Id,
                UserCode = user.UserCode,
                UserName = user.UserName!,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                ContactAddress = user.ContactAddress,
                UserGroup = new UserGroupViewModel
                {
                    Id = user.UserTitle.Id,
                    GroupCode = user.UserTitle.TitleCode,
                    GroupName = user.UserTitle.TitleName,
                    Description = user.UserTitle.Description
                },
                Department = new DepartmentViewModel
                {
                    Id = user.Department.Id,
                    DepartmentCode = user.Department.DepartmentCode,
                    DepartmentName = user.Department.DepartmentName
                }
            };
        }

        return null;
    }

    private async Task<List<SelectListItem>> LoadDepartmentList()
    {
        var departments = await _departmentService.FindAllAsync();
        if (departments.Any())
        {
            return departments.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.DepartmentName
            }).ToList();
        }

        ModelState.AddModelError("", "Departments not exist. Try to add department and continue.");
        return [];
    }
}
