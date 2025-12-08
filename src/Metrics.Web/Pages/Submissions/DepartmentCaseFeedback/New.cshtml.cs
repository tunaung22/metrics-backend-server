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

[Authorize(Policy = ApplicationPolicies.CanGive_Feedback_Policy)]
public class NewModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;
    private readonly ICaseFeedbackService _caseFeedbackSubmissionService;

    public NewModel(
        IUserService userService,
        IDepartmentService departmentService,
        ICaseFeedbackService caseFeedbackSubmissionService)
    {
        _userService = userService;
        _departmentService = departmentService;
        _caseFeedbackSubmissionService = caseFeedbackSubmissionService;
    }

    // =============== MODELS ==================================================
    public class FormInputModel
    {
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

    public List<SelectListItem> DepartmentListItems { get; set; } = null!;

    public UserViewModel Submitter { get; set; } = null!;
    public string? CurrentUserGroupName { get; set; } = string.Empty;



    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync()
    {
        // ----------SUBMITTER--------------------------------------------------
        var submitter = await GetCurrentUser();
        if (submitter == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid current user.");
            return Page();
        }
        Submitter = submitter;
        CurrentUserGroupName = Submitter.UserGroup.GroupName ?? string.Empty;
        // ----------DEPARTMENT-------------------------------------------------
        DepartmentListItems = await LoadDepartmentList();
        FormInput = new FormInputModel
        {
            // ScoreValue = -1M,
            SubmitterId = Submitter.Id,
            IncidentAt = DateTime.Now
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // submit the form
            var entity = CaseFeedback.Create(
            //    kpiSubmissionPeriodId: SelectedPeriod.Id,
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

            await _caseFeedbackSubmissionService.SaveAsync(entity);

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
        // return Redirect("/Submissions/DepartmentCaseFeedback/List");
        // if (!string.IsNullOrEmpty(ReturnUrl))
        //     return LocalRedirect(ReturnUrl);
        // return RedirectToPage("/Submissions/DepartmentCaseFeedback/List", new { periodName = SelectedPeriod.PeriodName });
        // return RedirectToPage("/Submissions/DepartmentCaseFeedback/List", new { periodName = SelectedPeriod.PeriodName });
        // return LocalRedirect("/Submissions/DepartmentCaseFeedback/List");
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
                DepartmentId = user.DepartmentId,
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

