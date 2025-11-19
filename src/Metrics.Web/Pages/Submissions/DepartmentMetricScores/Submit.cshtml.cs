using Metrics.Application.Authorization;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.KeyKpiSubmissions;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Metrics.Web.Models.DepartmentKeyMetric;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.DepartmentMetricScores;

[Authorize(Policy = ApplicationPolicies.CanSubmitKeyKpiScorePolicy)]
public class SubmitModel(ILogger<SubmitModel> logger,
        IKpiSubmissionPeriodService kpiPeriodService,
        IUserService userService,
        IDepartmentKeyMetricService departmentKeyMetricService,
        IKeyKpiSubmissionConstraintService keyKpiSubmissionConstraintService,
        IKeyKpiSubmissionService keyKpiSubmissionService)
    : PageModel
{
    private readonly ILogger<SubmitModel> _logger = logger;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IUserService _userService = userService;
    private readonly IDepartmentKeyMetricService _dkmService = departmentKeyMetricService;
    private readonly IKeyKpiSubmissionConstraintService _submissionConstraintService = keyKpiSubmissionConstraintService;
    private readonly IKeyKpiSubmissionService _submissionService = keyKpiSubmissionService;

    // =============== MODELS ==================================================

    // --------------- View ----------------------------------------------------
    public class FinishedSubmissionViewModel
    {
        public DateOnly SubmissionDate { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
        // public long KpiPeriodId { get; set; }
        // public string SubmitterId { get; set; } = null!;
        // public long KeyIssueDepartmentId { get; set; }
        public string TargetDepartmentName { get; set; } = null!;
        public List<ExistingSubmissionItemViewModel> SubmissionInputDetails { get; set; } = [];
    }

    public class ExistingSubmissionItemViewModel
    {
        // public long DepartmentKeyMetricsId { get; set; }
        public DepartmentKeyMetricViewModel DepartmentKeyMetrics { get; set; } = null!;
        public decimal ScoreValue { get; set; }
        public string? Comments { get; set; } = string.Empty;
    }

    public IEnumerable<FinishedSubmissionViewModel> FinishedSubmissions { get; set; } = [];



    public class FinishedViewModel
    {
        public DateOnly SubmissionDate { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
        // public long KpiPeriodId { get; set; }
        // public string SubmitterId { get; set; } = null!;
        // public long KeyIssueDepartmentId { get; set; }
        public string KeyIssueDepartmentName { get; set; } = null!;
        // public long DepartmentKeyMetricsId { get; set; }
        public DepartmentKeyMetricViewModel DepartmentKeyMetrics { get; set; } = null!;
        public decimal ScoreValue { get; set; }
        public string? Comments { get; set; } = string.Empty;
    }
    public IEnumerable<FinishedViewModel> Finished { get; set; } = [];


    // --------------- Input ---------------------------------------------------
    public class InputModel
    {
        public long KeyIssueDepartmentId { get; set; }
        public List<InputDetailModel> InputDetails { get; set; } = [];
    }

    [BindProperty]
    public List<InputModel> Inputs { get; set; } = [];

    public class InputDetailModel
    {
        public long DepartmentKeyMetricsId { get; set; }
        [Required(ErrorMessage = "Score is required.")]
        [Range(1, 10, ErrorMessage = "Score is required and choose between 1 to 10.")]
        public decimal ScoreValue { get; set; }
        public string? Comments { get; set; } = string.Empty;
        public DepartmentKeyMetricViewModel? DepartmentKeyMetric { get; set; } // for info only
    }

    // public class SubmissionInputModel
    // {
    //     public DateTimeOffset SubmittedAt { get; set; }
    //     public long DepartmentKeyMetricsId { get; set; }
    //     [Required(ErrorMessage = "Score is required.")]
    //     [Range(1, 10, ErrorMessage = "Score is required and choose between 1 to 10.")]
    //     public decimal ScoreValue { get; set; }
    //     public string? Comments { get; set; } = string.Empty;

    //     public DepartmentKeyMetricViewModel? DepartmentKeyMetric { get; set; }
    // }

    // [BindProperty]
    // public List<SubmissionInputModel> SubmissionInputs { get; set; } = [];

    // -------------------------------------------------------------------------
    public KpiPeriodViewModel TargetKpiPeriod { get; set; } = null!;

    public List<DepartmentViewModel> KeyIssueDepartmentList { get; set; } = [];

    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = []; // by period by submitter department

    public List<KeyKpiSubmissionConstraintViewModel> KeyKpiSubmissionConstraints { get; set; } = [];

    // -----Submitter-----
    public UserViewModel Submitter { get; set; } = null!;

    // public ApplicationUser Submitter { get; set; } = null!;
    public DepartmentViewModel SubmitterDepartment { get; set; } = null!;
    public string? CurrentUserGroupName { get; set; } = string.Empty;

    // -----Period-----
    public string TargetKpiPeriodName { get; set; } = null!;

    // -----Submission-----
    public bool IsSubmissionValid { get; set; } = false; // check is current date not early or late
    public bool IsSubmissionsExist { get; set; } = false;

    // -----  -----
    public string? ReturnUrl { get; set; } = string.Empty;


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(string periodName, string? returnUrl)
    {
        // ==========INITIALIZE INPUT MODEL=================================
        // KeyKpiSubmissionConstraints  -> get DepartmentList
        // DepartmentList               -> get SubmissionInputs
        // DepartmentKeyMetrics         -> get SubmissionInputItems
        // from existingSubmissions     -> get DepartmentKeyMetrics

        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        // ----------KPI PERIOD-------------------------------------------------
        var selectedPeriod = await LoadKpiPeriod(periodName);
        if (selectedPeriod == null)
            return Page();

        TargetKpiPeriod = selectedPeriod;
        TargetKpiPeriodName = selectedPeriod.PeriodName;

        // ---------- Check Today Submission is Valid based on KPI Period ------
        // ---------- CHECK TARGET PERIOD IS VALID OR NOT ----------------------
        IsSubmissionValid = CheckSubmissionDateValidity(
                    startDate: TargetKpiPeriod.SubmissionStartDate,
                    endDate: TargetKpiPeriod.SubmissionEndDate);
        if (!IsSubmissionValid)
            return Page();

        // ----------SUBMITTER--------------------------------------------------
        var submitter = await GetCurrentUser();
        if (submitter == null)
            return Page();

        Submitter = submitter;
        CurrentUserGroupName = Submitter.UserGroup.GroupName ?? string.Empty;
        SubmitterDepartment = submitter.Department;

        // // ----------DEPARTMENT KEY METRICS----------------------------------
        // check Department Key Metric exist
        // var departmentKeyMetrics = await LoadDepartmentKeyMetrics(TargetKpiPeriod.Id, SubmitterDepartment.DepartmentCode);
        // if (departmentKeyMetrics == null || departmentKeyMetrics.Count == 0)
        //     return Page();
        // // department keys by period by submitter department
        // DepartmentKeyMetrics = departmentKeyMetrics;

        // ----------SUBMISSION CONSTRAINTS-------------------------------------
        // get constraints by period by submitter department (can get department keys)
        // no KeyKpiSubmissionConstraints means: no DepartmentKeyMetrics
        var submissionConstraints = await LoadSubmissionConstraints(TargetKpiPeriod.Id, SubmitterDepartment.DepartmentCode);
        if (submissionConstraints == null || submissionConstraints.Count == 0)
            return Page();
        KeyKpiSubmissionConstraints = submissionConstraints;
        // key issue departments
        KeyIssueDepartmentList = submissionConstraints
            .DistinctBy(c => c.DepartmentKeyMetric.KeyIssueDepartment.DepartmentName)
            .Select(c => c.DepartmentKeyMetric.KeyIssueDepartment)
            .ToList();

        // ----------INPUT MODEL------------------------------------------------
        // There are 3 possible conditions
        // CASE 1: (ALL SUBMISSIONS EXIST)
        //      All submissions already exist/fulfilled.
        //      - no need to submit anymore
        // CASE 2: (PART OF SUBMISSIONS EXIST)
        //      Part of previous submissions found, 
        //      but can submit for other departments 
        //      that were added later time after first submission.
        //      - Submit missing entries (partial).
        //      - ** how to identify entries that are editable?
        // CASE 3: (NO PREVIOUS EXIST)
        //      No previous submissions found.
        //      - Submit new entries (all).
        // var existingSubmissions = await GetExistingSubmissions(DepartmentList);

        var existingSubmissions = await _submissionService
            .FindByPeriodBySubmitterAsync(TargetKpiPeriod.Id, Submitter.Id);
        if (existingSubmissions.IsSuccess)
        {
            if (existingSubmissions.Data != null && existingSubmissions.Data.Count > 0)
            {
                IsSubmissionsExist = true;
                Finished = existingSubmissions.Data
                    .OrderBy(submission => submission.DepartmentKeyMetric.KeyIssueDepartment.DepartmentName)
                    .Select(submission => new FinishedViewModel
                    {
                        SubmissionDate = DateOnly.FromDateTime(submission.SubmittedAt.ToLocalTime().DateTime),
                        SubmittedAt = submission.SubmittedAt.ToLocalTime().DateTime,
                        KeyIssueDepartmentName = submission.DepartmentKeyMetric.KeyIssueDepartment.DepartmentName,
                        DepartmentKeyMetrics = submission.DepartmentKeyMetric.MapToViewModel(),
                        ScoreValue = submission.ScoreValue,
                        Comments = submission.Comments,
                    }
                    ).ToList();

                // get existing keys (dkms)
                var existingDKMs = existingSubmissions.Data
                    .Select(dkm => dkm.MapToViewModel())
                    .ToList();
                if (existingDKMs.Count > 0)
                {
                    // load old submission data
                }
                return Page();
            }
            else
            {
                // ----------NEW SUBMISSION-------------------------------------
                var departmentGroup = KeyKpiSubmissionConstraints
                    .GroupBy(c => c.DepartmentKeyMetric.KeyIssueDepartmentId)
                    .ToList();
                Inputs = departmentGroup
                    .Select(group => new InputModel // loop keys of each group to get InputModel
                    {
                        KeyIssueDepartmentId = group.Key,
                        InputDetails = group.Select(item => new InputDetailModel
                        {
                            DepartmentKeyMetricsId = item.DepartmentKeyMetricId,
                            ScoreValue = 5M,
                            Comments = string.Empty,
                            DepartmentKeyMetric = item.DepartmentKeyMetric
                        }).ToList()
                    }).ToList();
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Failed to load submissions.");
        }




        // TODO: how to know NEW or OLD?
        // get existing submission by Submitter by Period (KeyKpiSubmission + DepartmentKeyMetric)
        /// DepartmentKeyMetric-------------not-exists---------> Exit (not assigned yet)
        /// KeyKpiSubmissionConstaints------not-exists---------> Exit (not assigned yet or not allowed to submit)
        // SubmissionInputs = DepartmentKeyMetrics.Select(dkm => new SubmissionInputModel
        // {
        //     KpiPeriodId = TargetKpiPeriod.Id,
        //     SubmitterId = Submitter.Id,
        //     SubmittedAt = DateTimeOffset.UtcNow,
        //     DepartmentKeyMetricsId = dkm.Id,
        //     KeyIssueDepartmentId = dkm.KeyIssueDepartmentId,
        //     ScoreValue = 5.00M,
        //     Comments = string.Empty,
        // }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string periodName)
    {
        // ---------- KPI Period -----------------------------------------------
        // ----------KPI PERIOD-------------------------------------------------
        var selectedPeriod = await LoadKpiPeriod(periodName);
        if (selectedPeriod == null)
            return Page();

        TargetKpiPeriod = selectedPeriod;
        TargetKpiPeriodName = selectedPeriod.PeriodName;

        // ---------- Check Today Submission is Valid based on KPI Period ------
        // ---------- CHECK TARGET PERIOD IS VALID OR NOT ----------------------
        IsSubmissionValid = CheckSubmissionDateValidity(
                    startDate: TargetKpiPeriod.SubmissionStartDate,
                    endDate: TargetKpiPeriod.SubmissionEndDate);
        if (!IsSubmissionValid)
            return Page();

        // ---------- SUBMITTER ------------------------------------------------
        var submitter = await GetCurrentUser();
        if (submitter == null)
            return Page();

        Submitter = submitter;
        CurrentUserGroupName = Submitter.UserGroup.GroupName ?? string.Empty;
        SubmitterDepartment = submitter.Department;

        if (Inputs != null && ModelState.IsValid)
        {
            try
            {
                var createDTOs = Inputs.SelectMany(input =>
                {
                    return input.InputDetails.Select(detail =>
                    {
                        return new CreateKeyKpiSubmissionDto
                        {
                            PeriodId = TargetKpiPeriod.Id,
                            DepartmentKeyMetricId = detail.DepartmentKeyMetricsId,
                            ScoreValue = detail.ScoreValue,
                            Comments = detail.Comments,
                            SubmitterId = Submitter.Id,
                            SubmittedAt = DateTimeOffset.UtcNow,
                        };
                    }).ToList();
                }).ToList();

                if (createDTOs.Count > 0)
                {
                    var result = await _submissionService.SubmitSubmissionsAsync(createDTOs);
                    if (result.IsSuccess)
                    {
                        TempData["TargetKpiPeriodName"] = TargetKpiPeriodName;
                        var successUrl = Url.Page("/Submissions/DepartmentMetricScores/Success", new { periodName = periodName });
                        if (string.IsNullOrEmpty(successUrl))
                            return RedirectToPage("/Submissions/DepartmentMetricScores/Index");
                        return LocalRedirect(successUrl);
                    }
                    ModelState.AddModelError(string.Empty, "Submission failed!");
                }
                ModelState.AddModelError(string.Empty, "Submission items cannot be empty.");
            }
            catch (DuplicateContentException ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError(string.Empty, "Already submitted for current period.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occured while submitting key kpi score. {msg}", ex.Message);
                ModelState.AddModelError(string.Empty, "Submission failed! Please try again.");
            }
        }
        else
        {
            _logger.LogError("SubmissionInputs is empty.");
            ModelState.AddModelError(string.Empty, "Invalid submission inputs.");
        }

        return Page();
    }

    // =============== METHODS =================================================
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

    private async Task<ApplicationUser?> LoadSubmitter()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User is not found.");

        return await _userService.FindByIdAsync(userId);
    }


    // private async Task<List<KeyKpiSubmissionConstraintViewModel>> LoadSubmissionConstraints(
    //     Guid submitterDepartmentCode)
    // {
    //     var constraints = await _submissionConstraintService.FindBySubmitterDepartmentAsync(submitterDepartmentCode);
    //     if (constraints is { IsSuccess: true, Data: not null }) // constraints.IsSuccess && constraints.Data != null
    //         return constraints.Data.Select(c => c.MapToViewModel()).ToList();
    //     else
    //         ModelState.AddModelError(string.Empty, "Failed to fetch submission constriants. Please try again later.");
    //     return [];
    // }

    // private async Task<List<DepartmentViewModel>> LoadDepartments()
    // {

    // }

    private async Task<List<KeyKpiSubmissionConstraintViewModel>> LoadSubmissionConstraints(
        long periodId,
        Guid submitterDepartmentCode)
    {
        var constraints = await _submissionConstraintService
            .FindByPeriodBySubmitterDepartmentAsync(
                periodId,
                submitterDepartmentCode);
        if (constraints is { IsSuccess: true, Data: not null }) // constraints.IsSuccess && constraints.Data != null
            return constraints.Data
                .Select(c => c.MapToViewModel())
                .ToList();
        else
            ModelState.AddModelError(string.Empty, "Failed to fetch submission constriants. Please try again later.");
        return [];
    }

    /// <summary>
    /// LoadDepartmentKeyMetrics by Period ID and by Submitter Department Code
    /// </summary>
    /// <param name="periodId"></param>
    /// <param name="submitterDepartmentCode"></param>
    /// <returns></returns>
    private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeyMetrics(long periodId, Guid submitterDepartmentCode)
    {
        var dkms = await _dkmService.FindByPeriodByKeyIssueDepartmentAsync(periodId, submitterDepartmentCode);
        if (dkms is { IsSuccess: true, Data: not null })
        {
            var result = dkms.Data.Select(d => d.MapToViewModel()).ToList();

            if (result.Count > 0)
                return result;
            else
                ModelState.AddModelError(string.Empty, "Keys have not been assigend. Contact Administrator.");
        }
        else
            //Error processing submission form. Reason: failed to fetch department keys.
            ModelState.AddModelError(string.Empty, "Failed to fetch department keys. Please try again later.");
        return [];
    }

    private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeyMetrics(
        List<DepartmentViewModel> keyDepartmentList)
    {
        // Load Key Metrics by period 
        foreach (var department in keyDepartmentList)
        {
            List<DepartmentKeyMetricViewModel> tmpViewModel = [];

            // fetch key metrics by period by department
            var departmentKeyMetrics = await _dkmService
                .FindByPeriodByKeyIssueDepartmentAsync(TargetKpiPeriod.Id, department.DepartmentCode);
            if (departmentKeyMetrics.IsSuccess)
            {
                if (departmentKeyMetrics.Data != null && departmentKeyMetrics.Data.Count > 0)
                {
                    var dkms = departmentKeyMetrics.Data
                        // filter by constraint
                        // constraint.department == dkm.department == departmentList.department
                        .Where(k =>
                            KeyKpiSubmissionConstraints.Select(c => c.DepartmentKeyMetric.KeyIssueDepartmentId)
                                .Contains(k.KeyIssueDepartment.Id)
                                && k.KeyIssueDepartmentId == department.Id)
                        .Select(k => k.MapToViewModel()).ToList();
                    tmpViewModel.AddRange(dkms);
                    return tmpViewModel;
                }
                return [];
            }
            ModelState.AddModelError(string.Empty, "Failed to fetch department key metrics by department list.");
        }

        return [];
    }
}










/*
OnGetAsync
previous code








    // private bool CheckSubmissionValidity(KpiPeriodViewModel period)
    // {
    //     var currentDateTime = DateTimeOffset.UtcNow;

    //     if (currentDateTime < period.SubmissionStartDate
    //         || currentDateTime > period.SubmissionEndDate)
    //     {
    //         // EARLY
    //         if (currentDateTime < period.SubmissionStartDate)
    //             ModelState.AddModelError(string.Empty, "Invalid submission. This submission is not ready yet.");
    //         // LATE
    //         if (currentDateTime > period.SubmissionEndDate)
    //             ModelState.AddModelError(string.Empty, "Invalid submission. This submission is due.");

    //         return false;
    //     }

    //     return true;
    // }














  // SubmitterDepartment ကအမှတ်ပေးလို့ရတဲ့ keys တွေရှာ
        // from Submission Constraints, get key metrics issued by departments
        // ဘယ် department user က ဘယ် department ရဲ့ key တွေကို အမှတ်ပေးရမလဲ ဆိုတာကိုဆွဲထုတ်
        var keyKpiSubmissionConstraints = await _submissionConstraintService
            .FindByPeriodBySubmitterDepartmentAsync(
                TargetKpiPeriod.Id,
                SubmitterDepartment.DepartmentCode
            );

        if (keyKpiSubmissionConstraints.IsSuccess)
        {
            // EXTRACT DATA for navigational properties
            // MAP KeyKpiSubmissionConstraint to KeyKpiSubmissionConstraintViewModel
            if (keyKpiSubmissionConstraints.Data != null && keyKpiSubmissionConstraints.Data.Count > 0)
            {
                KeyKpiSubmissionConstraints = keyKpiSubmissionConstraints.Data
                    .Select(c => c.MapToViewModel())
                    .ToList();
            }

            if (KeyKpiSubmissionConstraints.Count > 0) // DepartmentList, DepartmentKeyMetrics
            {
                // ----- Submission are based on DepartmentKeyMetric (which is defined at Constraints table) -----

                // ---------- DEPARTMENT -----------------------------------
                // ----- Get DISTINCT Departments from Constraints.DepartmentKeyMetrics -----
                // အမှတ်ပေးရမည့် departments များ (Issuer Departments)
                DepartmentList = KeyKpiSubmissionConstraints
                    .Select(d => d.DepartmentKeyMetric.KeyIssueDepartment)
                    .DistinctBy(department => department.Id)
                    .Select(department => new DepartmentViewModel
                    {
                        Id = department.Id,
                        DepartmentCode = department.DepartmentCode,
                        DepartmentName = department.DepartmentName
                    })
                    .ToList();
                // DepartmentList = KeyKpiSubmissionConstraints
                //     .Select(c => new DepartmentViewModel
                //     {
                //         Id = c.DepartmentKeyMetric.TargetDepartment.Id,
                //         DepartmentCode = c.DepartmentKeyMetric.TargetDepartment.DepartmentCode,
                //         DepartmentName = c.DepartmentKeyMetric.TargetDepartment.DepartmentName
                //     })
                //     .DistinctBy(d => d.Id)
                //     .ToList();

                if (DepartmentList == null || DepartmentList.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "No Department exists to proceed. Please contact the Administrator.");
                    return Page();
                }

                // ---------- DKMs -----------------------------------------
                // DepartmentKeyMetricViewModel
                DepartmentKeyMetrics = KeyKpiSubmissionConstraints
                    .OrderBy(c => c.SubmitterDepartmentId) // Key Issuer
                    .Select(c => c.DepartmentKeyMetric)
                    .ToList();

                // CASES:
                // 1. (ALL SUBMISSIONS EXIST) all submissions already exist/fullfilled, no need to submit anymore.
                // 2. (PART OF SUBMISSIONS EXIST) part of previous submissions found, but can submit for other departments that were added later time after first submission.
                //      2.1 SubmissionItems missing (Insert ONLY Child items.)
                //      2.2 Submission missing (Insert BOTH Parent and Child items.)
                // 3. (NO PREVIOUS EXIST) no previous submissions found, need to submit.
                // var existingSubmissions = await GetExistingSubmissions(DepartmentList);

                // Get Existing Submission by Period, Submitter, Department ID List
                var existingSubmissions = await _submissionService
                    .FindBySubmitterByPeriodByDepartmentListAsync(
                        Submitter,
                        TargetKpiPeriod.Id,
                        DepartmentList.Select(department => department.Id).ToList<long>());
                // extract method: FindExistingSubmissions(ApplicationUser submitter, KpiSubmissionPeriod TargetKpiPeriod, List<long> departmentIDs)

                if (existingSubmissions.Count > 0) // **note: submission count == department count
                {
                    // ========== အမှတ်ပေးပြီးသား key ===========================
                    // How to get DKMs from existingSubmissions?
                    var existingDKMs = existingSubmissions
                        .SelectMany(s => s.KeyKpiSubmissionItems)
                        .Select(s => s.DepartmentKeyMetric);

                    // How to check existingSubmissions have the same DKMs as submissionConstraints?
                    // - existingSubmissions have: submissionItems > DKMs
                    // - submissionConstraints have: DKMS
                    if (DepartmentKeyMetrics.Count == existingDKMs.Count())
                    {
                        // 1. (ALL SUBMISSIONS EXIST) all submissions already exist/fullfilled, no need to submit anymore.
                        IsSubmissionsExist = true;
                        // TODO::fetch submitted data for display
                        // Find submissions by Submitter and Period
                        // ...
                        FinishedSubmissions = existingSubmissions.Select(s => new FinishedSubmissionViewModel
                        {
                            SubmissionDate = s.SubmissionDate,
                            SubmittedAt = s.SubmittedAt,
                            TargetDepartmentName = s.TargetDepartment.DepartmentName,
                            SubmissionInputItems = s.KeyKpiSubmissionItems.Select(i => new ExistingSubmissionItemViewModel
                            {
                                DepartmentKeyMetrics = i.DepartmentKeyMetric.MapToViewModel(),
                                ScoreValue = i.ScoreValue,
                                Comments = i.Comments
                            }).ToList()
                        }).ToList();

                        return Page();
                    }
                    else
                    {
                        // 2. (PART OF SUBMISSIONS EXIST) part of previous submissions found, but can submit for other departments that were added later time after first submission.
                        // **Note:  no. of submissions != no. of departments
                        //          can't check submission count based on department list
                        //          each department have a number of key metrics

                        // **To check submissioin needs submitting even if it was already submitted.
                        //   when new constriants or new departments were added after submitted.
                        // CASES:
                        // **2.1 SubmissionItems missing (Insert ONLY Child items.)
                        // **2.2 Submission missing (Insert BOTH Parent and Child items.)
                        // **Use the same Input modle for both 2.1 and 2.2
                        // **and logic check at Service???

                        // ========== အမှတ်ပေးဖို့ကျန်တဲ့ key ======================
                        // Get အမှတ်ထဲ့ဖို့ ကျန်နေသေးတဲ့ DepartmentKeyMetrics:: avaiableDKMs = (DKMs - existingDKMs)
                        var avaiableDKMs = DepartmentKeyMetrics
                            .Where(dkms => existingDKMs
                                .Select(e => e.Id)
                                .Contains(dkms.Id));
                        // Update DepartmentList from avaible DepartmentKeyMetircs
                        DepartmentList = avaiableDKMs
                            .Select(dkm => new DepartmentViewModel
                            {
                                Id = dkm.TargetDepartment.Id,
                                DepartmentCode = dkm.TargetDepartment.DepartmentCode,
                                DepartmentName = dkm.TargetDepartment.DepartmentName
                            })
                            .DistinctBy(dkm => dkm.Id)
                            .ToList();

                        // Initialize Input Model
                        // Use DepartmentList to render Submission Inputs
                        SubmissionInputs = DepartmentList.Select(department => new SubmissionInputModel
                        {

                            KpiPeriodId = TargetKpiPeriod.Id,
                            KeyIssueDepartmentId = department.Id,
                            SubmissionInputItems = avaiableDKMs
                                .Where(dkms => dkms.DepartmentId == department.Id)
                                .Select(dkms => new KeyKpiSubmissionInputItemModel
                                {
                                    DepartmentKeyMetricsId = dkms.Id,
                                    ScoreValue = 5,
                                    Comments = string.Empty
                                }).ToList()
                        }).ToList();
                    }
                }
                else
                {
                    // 3. NEW ENTRY (no previous entries)
                    // SubmissionInput = new List<SubmissionInputModel>();
                    SubmissionInputs = DepartmentList.Select(department => new SubmissionInputModel
                    {
                        // init input itmes based on DepartmentKeyMetrics of the department
                        // where d.id == dkm.targetDepartmentId
                        SubmittedAt = DateTimeOffset.UtcNow, // **will set at service
                        KpiPeriodId = TargetKpiPeriod.Id,
                        KeyIssueDepartmentId = department.Id,
                        SubmissionInputItems = DepartmentKeyMetrics
                            .Where(dkms => dkms.DepartmentId == department.Id)
                            .Select(dkm => new KeyKpiSubmissionInputItemModel
                            {
                                DepartmentKeyMetricsId = dkm.Id,
                                ScoreValue = 5,
                                Comments = string.Empty

                            }).ToList()
                    }).ToList();

                    // SubmissionInputs = DepartmentList.Select(department =>
                    // {
                    //     var dkms = DepartmentKeyMetrics
                    //         .Where(dkms => dkms.DepartmentId == department.Id)
                    //         .Select(dkm => new KeyKpiSubmissionInputItemModel
                    //         {
                    //             DepartmentKeyMetricsId = dkm.Id,
                    //             ScoreValue = 5,
                    //             Comments = string.Empty
                    //         }).ToList();

                    //     return new SubmissionInputModel
                    //     {
                    //         TargetDepartmentId = department.Id,
                    //         SubmissionInputItems = dkms
                    //     };
                    // }).ToList();
                }
            }
            else // KeyKpiSubmissionConstraints.Count == 0)
            {
                // ----- NO CONSTRAINTS SET KeyKpiSubmissionConstraints (ViewModel) ------------
                _logger.LogError("DepartmentMetricScore: KeyKpiSubmissionConstraints (ViewModel) is empty.");
                ModelState.AddModelError(string.Empty, "The submission isn’t ready yet. Please contact the administrator.");
            }
        }
        else // keyKpiSubmissionConstraints is NULL)
        {
            // ----- NO CONSTRAINTS SET keyKpiSubmissionConstraints ------------
            _logger.LogError("DepartmentMetricScore: keyKpiSubmissionConstraints is empty.");
            // ModelState.AddModelError(string.Empty, "No metric score for submissions have set yet. Please contact Administator.");
            ModelState.AddModelError(string.Empty, "The submission isn’t ready yet. Please contact the administrator.");
        }

*/





/*
OnPost

 if (SubmissionInputs != null && ModelState.IsValid)
        {
            try
            {
                // 1. map ViewModel to DTO
                // 2. call Submit service method (pass DTO)
                var submissionsCreateDtos = SubmissionInputs
                    .Where(s => s.SubmissionInputItems.Count > 0)
                    .Select(s =>
                    {
                        var kksubmissionItemDtos = s.SubmissionInputItems
                                .Select(item => new KeyKpiSubmissionItemDto
                                {
                                    DepartmentKeyMetricId = item.DepartmentKeyMetricsId,
                                    ScoreValue = item.ScoreValue,
                                    Comments = item.Comments
                                }).ToList();



                        return new KeyKpiSubmissionCreateDto
                        {
                            ScoreSubmissionPeriodId = TargetKpiPeriod.Id,
                            DepartmentId = s.TargetDepartmentId,
                            ApplicationUserId = submitter.Id,
                            KeyKpiSubmissionItemDtos = kksubmissionItemDtos
                        };
                    }).ToList();

                if (submissionsCreateDtos.Any())
                {

                    var success = await _submissionService.SubmitScoreAsync(submissionsCreateDtos);
                    if (success)
                    {
                        TempData["TargetKpiPeriodName"] = TargetKpiPeriodName;
                        var successUrl = Url.Page("/Submissions/DepartmentMetricScores/Success", new { periodName = periodName });
                        if (string.IsNullOrEmpty(successUrl))
                            return RedirectToPage("/Submissions/DepartmentMetricScores/Index");
                        return LocalRedirect(successUrl);
                    }
                    ModelState.AddModelError(string.Empty, "Submission failed!");
                }
                ModelState.AddModelError(string.Empty, "Submission items cannot be empty.");
            }
            catch (DuplicateContentException ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError(string.Empty, "Already submitted for current period.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError(string.Empty, "Submission failed! Please try again.");
            }
        }
        else
        {
            _logger.LogError("SubmissionInputs is empty.");
            ModelState.AddModelError(string.Empty, "Invalid form inputs.");
        }

        return Page();
*/