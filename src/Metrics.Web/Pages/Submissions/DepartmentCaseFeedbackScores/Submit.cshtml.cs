using Metrics.Application.Authorization;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.CaseFeedbackScoreSubmission;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.DepartmentCaseFeedbackScores;

[Authorize(Policy = ApplicationPolicies.CanSubmit_FeedbackScore_Policy)]
public class SubmitModel(
    IUserService userService,
    IKpiSubmissionPeriodService kpiPeriodService,
    ICaseFeedbackService feedbackService,
    ICaseFeedbackScoreSubmissionService submissionService
    ) : PageModel
{
    private readonly IUserService _userService = userService;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly ICaseFeedbackService _feedbackService = feedbackService;
    private readonly ICaseFeedbackScoreSubmissionService _submissionService = submissionService;

    // =============== MODELS ==================================================
    public decimal[] ScoreList { get; set; } = [-1M, -2M, -3M, -4M, -5M];

    public class FormInputModel
    {
        public long Id { get; set; }
        public Guid? LookupId { get; set; }
        public long SubmissionPeriodId { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }

        // public string SubmitterId { get; set; } = null!; // Foreign Keys

        [Required(ErrorMessage = "Score is required")]
        [Range(-5, -1, ErrorMessage = "Score must be between -1 to -5")]
        public decimal ScoreValue { get; set; } // **note: Negative value

        public long FeedbackId { get; set; }
        public string? Comments { get; set; } = string.Empty; // Additional Notes
    }

    [BindProperty]
    public List<FormInputModel> FormInput { get; set; } = [];

    // Feedback
    public class FeedbackViewModel
    {
        public long Id { get; set; }
        public long SubmissionPeriodId { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
        public string FeedbackSubmitterId { get; set; } = null!;
        public ApplicationUser FeedbackSubmitter { get; set; } = null!;
        public long CaseDepartmentId { get; set; }
        public Department? CaseDepartment { get; set; }
        public DateTimeOffset IncidentAt { get; set; }
        public string WardName { get; set; } = null!;
        public string CPINumber { get; set; } = null!;
        public string PatientName { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        public string? Description { get; set; } = string.Empty;
    }
    public List<FeedbackViewModel> Feedbacks { get; set; } = [];

    public UserViewModel Submitter { get; set; } = null!;
    public string? CurrentUserGroupName { get; set; } = string.Empty;
    public KpiPeriodViewModel SelectedPeriod { get; set; } = null!;

    [BindProperty]
    public string SelectedPeriodName { get; set; } = null!;

    // STATUS
    public bool NewSubmission { get; set; } = false;


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(
        string periodName)
    {
        // ----------KPI PERIOD-------------------------------------------------
        var selectedPeriod = await LoadKpiPeriod(periodName);
        if (selectedPeriod == null)
            return Page();

        SelectedPeriod = selectedPeriod;
        SelectedPeriodName = selectedPeriod.PeriodName;

        // ----------FEEDBACK---------------------------------------------------
        Feedbacks = await LoadFeedbacks(selectedPeriod.Id);
        if (Feedbacks.Count == 0)
            return Page();

        // ==========GUARD======================================================
        // ----------SUBMISSION VALIDITY (EARLY or DUE)-------------------------
        var validDate = CheckSubmissionDateValidity(
            startDate: SelectedPeriod.SubmissionStartDate,
            endDate: SelectedPeriod.SubmissionEndDate);
        if (!validDate)
            return Page();

        // ----------SUBMITTER--------------------------------------------------
        var submitter = await GetCurrentUser();
        if (submitter == null)
            return Page();

        Submitter = submitter;
        CurrentUserGroupName = Submitter.UserGroup.GroupName ?? string.Empty;

        // ----------FORM---------------------------------------------------
        // feedback count =? submission count
        // feedbackCount == submissionCount => done 
        //                                  => show table
        // feedbackCount > submissionCount  => need extra submssion 
        //                                  => show table + show form
        var existingSubmissions = await LoadExistingSubmissionsAsync(
            periodId: SelectedPeriod.Id,
            submitterId: Submitter.Id);

        if (existingSubmissions.Count == 0)
        {
            // NEW SUBMISSION
            // INSERT
            NewSubmission = true;
            FormInput = Feedbacks.Select(f => new FormInputModel
            {
                SubmittedAt = DateTimeOffset.UtcNow, // will set utcNow at OnPost for accuracy 
                ScoreValue = -5M,
                Comments = string.Empty,
            }).ToList();
        }
        else // (existingSubmissions.Count > 0)
        {
            // EXISTING SUBMISSION
            // UPSERT
            // existing (update) + new (insert)
            FormInput = Feedbacks.Select(feedback =>
            {
                // filter old feedback
                var submission = existingSubmissions
                    .Where(submission => submission.CaseFeedbackId == feedback.Id && submission.ModifiedAt.Date == DateTimeOffset.UtcNow.Date)
                    .FirstOrDefault();
                if (submission != null)
                {
                    // OLD
                    // fill old value
                    return new FormInputModel
                    {
                        Id = submission.Id,
                        LookupId = submission.LookupId,
                        FeedbackId = submission.CaseFeedbackId,
                        SubmittedAt = submission.SubmittedAt,
                        ModifiedAt = submission.ModifiedAt,
                        ScoreValue = submission.NegativeScoreValue,
                        Comments = submission.Comments ?? string.Empty,
                    };
                }
                else
                {
                    // NEW
                    // init new form
                    return new FormInputModel
                    {
                        ScoreValue = -5M,
                        Comments = string.Empty,
                    };
                }
            }).ToList();
        }

        return Page();
    }



    public async Task<IActionResult> OnPostAsync(string periodName)
    {
        // ----------KPI PERIOD-------------------------------------------------
        var selectedPeriod = await LoadKpiPeriod(periodName);
        if (selectedPeriod == null)
            return Page();

        SelectedPeriod = selectedPeriod;
        // SelectedPeriodName = selectedPeriod.PeriodName;

        // ----------FEEDBACK---------------------------------------------------
        Feedbacks = await LoadFeedbacks(selectedPeriod.Id);
        if (Feedbacks.Count == 0)
            return Page();

        // ==========GUARD======================================================
        // ----------SUBMISSION VALIDITY (EARLY or DUE)-------------------------
        var validDate = CheckSubmissionDateValidity(
            startDate: SelectedPeriod.SubmissionStartDate,
            endDate: SelectedPeriod.SubmissionEndDate);
        if (!validDate)
            return Page();

        // ----------SUBMITTER--------------------------------------------------
        var submitter = await GetCurrentUser();
        if (submitter == null)
            return Page();

        Submitter = submitter;
        CurrentUserGroupName = Submitter.UserGroup.GroupName ?? string.Empty;

        // ----------FORM-------------------------------------------------------
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Form is invalid.");
            return Page();
        }

        try
        {

            // INSERT OR UPSERT
            // INSERT
            if (NewSubmission)
            {
                List<CaseFeedbackScoreSubmissionCreateDto> submissions = [];
                submissions = FormInput
                    .Select(form => new CaseFeedbackScoreSubmissionCreateDto(
                        ScoreSubmitterId: Submitter.Id,
                        SubmittedAt: DateTimeOffset.UtcNow, //form.SubmittedAt,
                        CaseFeedbackId: form.FeedbackId,
                        NegativeScoreValue: form.ScoreValue,
                        Comments: form.Comments ?? string.Empty))
                    .ToList();
                await _submissionService.SaveRangeAsync(submissions);
            }
            // UPSERT
            // Insert new + Update old
            else
            {
                List<CaseFeedbackScoreSubmissionUpsertDto> submissions = [];
                submissions = FormInput
                    .Select(form => new CaseFeedbackScoreSubmissionUpsertDto(
                        Id: form.Id, // value or 0
                        LookupId: form.LookupId, // value or null
                        ScoreSubmitterId: Submitter.Id,
                        SubmittedAt: DateTimeOffset.UtcNow, // ** SubmittedAt for Insert, ModifiedAt for Update
                        CaseFeedbackId: form.FeedbackId,
                        NegativeScoreValue: form.ScoreValue,
                        Comments: form.Comments ?? string.Empty))
                    .ToList();
                var result = await _submissionService.UpsertRangeAsync(submissions);

                if (result.IsSuccess)
                {
                    // update FormInput.ModifiedAt
                    foreach (var item in FormInput)
                        item.ModifiedAt = DateTimeOffset.UtcNow;

                    TempData["StatusMessage"] = $"Score for {periodName} has been submitted successfully";

                    return RedirectToPage("./Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Submission failed.");
                }
            }

            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Submission failed.");
            return Page();
        }
    }

    public IActionResult OnPostCancel()
    {
        return RedirectToPage("./Index");
    }

    // ========== METHODS ======================================================
    private async Task<UserViewModel?> GetCurrentUser()
    {
        UserViewModel? data = null;

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Less likely to cause user not found, so throw just in case
            // ?? throw new Exception("User not found. Please login again.");
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userService.FindByIdAsync_2(userId);
                if (user.IsSuccess && user.Data != null)
                    data = user.Data.MapToViewModel();
            }
            else
                ModelState.AddModelError(string.Empty, "User not found. Please login again.");

            return data;
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Invalid current user.");
            return data;
        }
    }

    private async Task<KpiPeriodViewModel?> LoadKpiPeriod(string periodName)
    {
        KpiPeriodViewModel? kpiPeriodModel = null;

        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "Period Name is required.");
        }
        else
        {
            var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(periodName);
            if (kpiPeriod != null)
            {
                kpiPeriodModel = new KpiPeriodViewModel() // ---- do we need entire KPI Period object??
                {
                    Id = kpiPeriod.Id,
                    PeriodName = kpiPeriod.PeriodName,
                    SubmissionStartDate = kpiPeriod.SubmissionStartDate,
                    SubmissionEndDate = kpiPeriod.SubmissionEndDate
                };
            }
            else
            {
                ModelState.AddModelError(string.Empty, $"Period {periodName} not found.");
            }
        }

        return kpiPeriodModel;
    }

    // private async Task<KpiPeriodViewModel?> LoadKpiPeriod(string periodName)
    // {
    //     var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(periodName);
    //     if (kpiPeriod != null)
    //         return new KpiPeriodViewModel() // ---- do we need entire KPI Period object??
    //         {
    //             Id = kpiPeriod.Id,
    //             PeriodName = kpiPeriod.PeriodName,
    //             SubmissionStartDate = kpiPeriod.SubmissionStartDate,
    //             SubmissionEndDate = kpiPeriod.SubmissionEndDate
    //         };

    //     ModelState.AddModelError(string.Empty, $"Period {periodName} not found.");
    //     return null;
    // }

    /// <summary>
    /// Check Submission Date Validity
    /// date is early or due
    /// </summary>
    /// early: dt < start, late: dt > end
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    private bool CheckSubmissionDateValidity(
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        // EARLY => dt < start date
        if (DateTime.Now < startDate)
        {
            ModelState.AddModelError(string.Empty, "Submission not open yet.");
            return false;
        }
        // DUE   => dt > end date
        else if (DateTime.Now > endDate)
        {
            ModelState.AddModelError(string.Empty, "Submission not open yet.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Load unproceeded feedbacks (by date range??)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<List<FeedbackViewModel>> LoadFeedbacks(long id)
    {
        List<FeedbackViewModel> feedbackList = [];

        // TODO: load feedback by date range??
        // var feedbacks = await _feedbackService.FindByKpiPeriodAsync(id);
        // var feedbacks = await _feedbackService.FindAllActiveAsync();

        var feedbacks = await _feedbackService.FindAllActiveAsync(
            SelectedPeriod.SubmissionStartDate,
            SelectedPeriod.SubmissionEndDate);
        if (feedbacks.IsSuccess && feedbacks.Data != null)
        {
            feedbackList = feedbacks.Data.Select(fb => new FeedbackViewModel
            {
                Id = fb.Id,
                CaseDepartmentId = fb.CaseDepartmentId,
                CaseDepartment = fb.CaseDepartment,
                IncidentAt = fb.IncidentAt,
                WardName = fb.WardName,
                CPINumber = fb.CPINumber,
                Description = fb.Description,
                PatientName = fb.PatientName,
                RoomNumber = fb.RoomNumber,
                // SubmissionPeriodId = fb.KpiSubmissionPeriodId,
                SubmittedAt = fb.SubmittedAt,
                FeedbackSubmitterId = fb.SubmitterId,
                FeedbackSubmitter = fb.SubmittedBy
            }).ToList();
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Feedback does not exist. Try to add feedback and continue.");
        }

        return feedbackList;
    }

    private async Task<List<CaseFeedbackScoreSubmissionViewModel>> LoadExistingSubmissionsAsync(
        long periodId,
        string submitterId)
    {
        List<CaseFeedbackScoreSubmissionViewModel> data = [];
        var submissions = await _submissionService.FindByKpiPeriodAndSubmitterAsync(periodId, submitterId);

        // success -> null? -> return data or empty
        // fail    -> return empty
        if (!submissions.IsSuccess)
            ModelState.AddModelError(string.Empty, "Failed to fetch existing submissions.");

        if (submissions.IsSuccess && submissions.Data != null)
            data = submissions.Data
                .Select(submission => submission.MapToViewModel())
                .ToList();

        return data;
    }
}
