using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.Departments.Scores;

[Authorize(Policy = "CanSubmitBaseScorePolicy")]
public class SubmitModel : PageModel
{
    private readonly IKpiSubmissionService _kpiSubmissionService;
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IUserService _userService;

    public SubmitModel(
                        IDepartmentService departmentService,
                        IKpiSubmissionPeriodService kpiPeriodService,
                        IKpiSubmissionService kpiSubmissionService,
                        IUserService userService)
    {
        _departmentService = departmentService;
        _kpiPeriodService = kpiPeriodService;
        _kpiSubmissionService = kpiSubmissionService;
        _userService = userService;
    }

    // =========================================================================
    // - fetch all departments
    // - loop department -> render entry form
    // - check existing entry or new entry
    // - on next clicked -> save the entry to session storage
    // - on next clicked -> render new entry from
    // - on submit clicked -> bulk save the entries

    // QueryString: pageNumber = 1, pageSize = 5
    // 1. get employee id
    // 2. get kpi periods
    // 3. get departments
    // 4. get existing submission
    // var totalDepartments = await _departmentService.FindCount_Async();
    // TotalPages = (int)Math.Ceiling(totalDepartments / (double)PageSize);


    // =============== MODELS ==================================================
    public class KpiSubmissionGetModel // Model for all Submission by a Period
    {
        // public long KpiSubmissionPeriodId { get; set; }
        public DateOnly SubmissionDate { get; set; }
        // public long DepartmentId { get; set; }
        public string? DepartmentName { get; set; } = string.Empty;
        public decimal ScoreValue { get; set; }
        public string? PositiveAspects { get; set; } = string.Empty;
        public string? NegativeAspects { get; set; } = string.Empty;
        public string? Comments { get; set; } = string.Empty;
    }
    public List<KpiSubmissionGetModel> DoneKpiSubmissions { get; set; } = [];

    // Input Model for Staff
    public class SubmissionInputModel
    {
        // public long Id { get; set; }
        public DateTimeOffset SubmissionTime { get; set; }
        [Required(ErrorMessage = "KPI Score is required.")]
        [Range(1, 10, ErrorMessage = "KPI Score is required and choose between 1 to 10.")]
        public decimal KpiScore { get; set; }
        public string? PositiveAspects { get; set; }
        public string? NegativeAspects { get; set; }
        public string? Comments { get; set; }
        // Foreign Keys
        public long KpiPeriodId { get; set; }
        [Required(ErrorMessage = "Department ID is required.")]
        public long DepartmentId { get; set; }
        // public long EmployeeId { get; set; }
        // Reference Navigational Properties
        // public KpiPeriod KpiPeriod { get; set; } = null!;
        // public Department TargetDepartment { get; set; } = null!;
        // public Employee Candidate { get; set; } = null!;
    }

    public class DepartmentModel
    {
        public long Id { get; set; }
        public Guid DepartmentCode { get; set; }
        public string DepartmentName { get; set; } = null!;
    }
    // =========================================================================

    [BindProperty]
    public List<SubmissionInputModel> SubmissionInput { get; set; } = [];
    // public SubmissionInputModel SubmissionSingleInput { get; set; } = new SubmissionInputModel();
    // ==========
    public string TargetKpiPeriodName { get; set; } = null!;
    public long TargetKpiPeriodId { get; set; }
    public bool IsSubmissionValid { get; set; } = false; // check is current date not early or late
    // public long EmployeeId { get; set; }
    public ApplicationUser Submitter { get; set; } = null!;
    public List<DepartmentModel> DepartmentList { get; set; } = [];
    public bool IsSubmissionsExist { get; set; } = false;
    // this model won't need if submission time is based on current time 
    // public DateTimeOffset SubmissionTime { get; set; } = DateTimeOffset.UtcNow;
    public string? CurrentUserTitleName { get; set; } = string.Empty;
    // ==========
    // public bool IsSubmissionAvaiable { get; set; } = false;
    public string? ReturnUrl { get; set; } = string.Empty;


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(string periodName, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        TargetKpiPeriodName = periodName;

        // ---------- KPI Period ID ----------------------------------------
        // find kpi period id by period name
        // var periodId = await _kpiPeriodService.FindIdByKpiPeriodName_Async(TargetKpiPeriodName);

        // if (periodId <= 0)
        // {
        //     ModelState.AddModelError("", $"Submission not found for the period {TargetKpiPeriodName}.");
        //     IsSubmissionValid = false;

        //     return Page();
        // }
        // else
        // {
        //     IsSubmissionValid = true;
        //     TargetKpiPeriodId = periodId;
        // }



        // Assign: IsSubmissionValid, TargetKpiPeriodId
        // if (!await GetKpiPeriodId(periodName)) // set targetPeriod + get submissionValidity
        //     return Page();
        var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(TargetKpiPeriodName);
        if (kpiPeriod == null)
        {
            ModelState.AddModelError(string.Empty, $"Submission not found for the period {TargetKpiPeriodName}.");
            IsSubmissionValid = false;
            return Page();
        }
        TargetKpiPeriodId = kpiPeriod.Id;

        // ---------- Check Today Submission is Valid based on KPI Period ------
        // ---------- CHECK TARGET PERIOD IS VALID OR NOT ----------------------
        var utcNow = DateTimeOffset.UtcNow;
        if (utcNow < kpiPeriod.SubmissionStartDate || utcNow > kpiPeriod.SubmissionEndDate)
        {
            if (utcNow < kpiPeriod.SubmissionStartDate)
                ModelState.AddModelError(string.Empty, "Invalid submission. This submission is not ready yet.");
            if (utcNow > kpiPeriod.SubmissionEndDate)
                ModelState.AddModelError(string.Empty, "Invalid submission. This submission is due.");
            // IsSubmissionValid = false;
            return Page();
        }

        IsSubmissionValid = true;

        // ---------- Employee ID ----------------------------------------
        // EmployeeId = await GetEmployeeId();
        Submitter = await GetCurrentUser();
        CurrentUserTitleName = Submitter.UserTitle?.TitleName ?? string.Empty;

        // ----- Load Kpi Period List for Dropdown -----
        // var listItems = await LoadKpiPeriodListItems();
        // if (listItems == null)
        // {
        //     ModelState.AddModelError("", "No KPI Periods Avaiable Currently");
        //     return Page();
        // }
        // KpiPeriodListItems = listItems;
        // SelectedKpiPeriodId = long.TryParse(KpiPeriodListItems[0].Value, out long value) ? value : 0;

        // ---------- DepartmentList ----------------------------------------
        // var departments = await _departmentService.FindAllInsecure_Async();
        // if (!departments.Any())
        // {
        //     ModelState.AddModelError("", "No departments to submit score. Please contact authorities.");
        //     return Page();
        // }
        // DepartmentList = departments.Select(e => new DepartmentModel
        // {
        //     Id = e.Id,
        //     DepartmentCode = e.DepartmentCode,
        //     DepartmentName = e.DepartmentName
        // }).ToList();

        // Assign: DepartmentList
        DepartmentList = await GetDepartmentList(Submitter.Id);
        if (DepartmentList.Count == 0)
            return Page();



        // ---------- Reinitialize Department List -----------------------------
        // Find submission already exist or not
        // select submissions where employee_id + kpi_period_id + department_id
        // var departmentIDs = DepartmentList.Select(department => department.Id).ToList<long>();
        // var existingSubmissions = await _kpiSubmissionService.Find_Async(EmployeeId, TargetKpiPeriodId, departmentIDs);
        // if (existingSubmissions.Any())
        // {
        //     if (DepartmentList.Count == existingSubmissions.Count())
        //     {
        //         // ModelState.AddModelError("", "Already Submitted.");
        //         IsSubmissionsExist = true;
        //         return Page();
        //     }
        //     // ============================================================
        //     // var a = from d in DepartmentList
        //     //         join s in existingSubmissions
        //     //         on d.Id equals s.DepartmentId into submissionsGroup
        //     //         from s in submissionsGroup.DefaultIfEmpty() // Left join
        //     //         where s == null // Filter for departments with no submissions
        //     //         select d;
        //     // DepartmentList = a.ToList();
        //     // ============================================================
        //     // Filter DepartmentList only for new Submissions
        //     DepartmentList = DepartmentList
        //         .Where(d => !existingSubmissions.Any(s => s.DepartmentId == d.Id))
        //         .ToList();
        // }

        // 1. all submissions already exist/fullfilled, no need to submit anymore.
        // 2. part of previous submissions found, but can submit for other departments that were added later time after first submission.
        // 3. no previous submissions found, need to submit.
        var existingSubmissions = await GetExistingSubmissions(DepartmentList);
        if (existingSubmissions != null)
        {
            // all submission already exist/fullfilled
            if (DepartmentList.Count <= existingSubmissions.Count) // submissions always likely have more than department (as department can get deleted) 
            {
                IsSubmissionsExist = true;
                // fetch submitted data for display
                // Find submissions by Submitter and Period
                var submissions = await _kpiSubmissionService.FindByKpiPeriodAndSubmitterAsync(kpiPeriod.Id, Submitter.Id);
                if (submissions.Count > 0)
                {
                    DoneKpiSubmissions = submissions.Select(s => new KpiSubmissionGetModel
                    {
                        SubmissionDate = s.SubmissionDate,
                        // DepartmentId = s.DepartmentId,
                        DepartmentName = s.TargetDepartment?.DepartmentName,
                        ScoreValue = s.ScoreValue,
                        PositiveAspects = s.PositiveAspects,
                        NegativeAspects = s.NegativeAspects,
                        Comments = s.Comments
                    }).ToList();
                }
                return Page();
            }
            DepartmentList = await UpdateDepartmentList(DepartmentList, existingSubmissions);
        }
        // no previous submissions found (continue)

        // ---------- Bind Input Model List ------------------------------------
        SubmissionInput = new List<SubmissionInputModel>();
        SubmissionInput = DepartmentList.Select(department => new SubmissionInputModel
        {
            // EmployeeId = EmployeeId,
            // KpiPeriodId = TargetKpiPeriodId,
            // DepartmentId = department.Id,
            KpiScore = 5
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string periodName)
    {
        TargetKpiPeriodName = periodName;

        // ---------- Assign: IsSubmissionValid, TargetKpiPeriodId
        // if (!await GetKpiPeriodId(periodName))
        //     return Page();
        var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(periodName);
        if (kpiPeriod == null)
        {
            ModelState.AddModelError("", $"Submission not found for the period {periodName}.");
            IsSubmissionValid = false;
            return Page();
        }
        TargetKpiPeriodId = kpiPeriod.Id;
        IsSubmissionValid = true;

        // ---------- Assign: IsSubmissionValid
        // if (!await GetIsSubmissionValid(DateTimeOffset.UtcNow))
        //     return Page();
        var kpiPeriods = await _kpiPeriodService.FindAllByDateAsync(DateTimeOffset.UtcNow);
        if (!kpiPeriods.Any())
        {
            ModelState.AddModelError("", "Submission not avaiable.");
            IsSubmissionValid = false;
            return Page();
        }
        IsSubmissionValid = true;


        // EmployeeId = await GetEmployeeId();
        Submitter = await GetCurrentUser();
        CurrentUserTitleName = Submitter.UserTitle?.TitleName ?? string.Empty;

        // Assign: DepartmentList
        DepartmentList = await GetDepartmentList(Submitter.Id);
        if (DepartmentList.Count == 0)
            return Page();

        // 1. all submissions already exist/fullfilled, no need to submit anymore.
        // 2. part of previous submissions found, but can submit for other departments that were added later time after first submission.
        // 3. no previous submissions found, need to submit.
        var existingSubmissions = await GetExistingSubmissions(DepartmentList);
        if (existingSubmissions != null)
        {
            // all submission already exist/fullfilled
            if (DepartmentList.Count == existingSubmissions.Count)
            {
                IsSubmissionsExist = true;
                return Page();
            }

            DepartmentList = await UpdateDepartmentList(DepartmentList, existingSubmissions);
        }

        // no previous submissions found (continue)
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Form invalid");
            return Page();
        }

        // if (DepartmentList.Count != SubmissionInput.Count)
        // {
        //     ModelState.AddModelError("", "Department list and submission list mismatch. There will be chances of changes in departments data while you're filling the form. Please try again.");
        //     return Page();
        // }

        var submissionList = new List<KpiSubmission>();
        for (int i = 0; i < SubmissionInput.Count; i++)
        {
            var submission = new KpiSubmission
            {
                SubmittedAt = DateTimeOffset.UtcNow,
                ApplicationUserId = Submitter.Id,
                KpiSubmissionPeriodId = TargetKpiPeriodId,
                DepartmentId = DepartmentList[i].Id,
                ScoreValue = SubmissionInput[i].KpiScore.Equals(0) ? 1 : SubmissionInput[i].KpiScore,
                PositiveAspects = SubmissionInput[i].PositiveAspects ?? "",
                NegativeAspects = SubmissionInput[i].NegativeAspects ?? "",
                Comments = SubmissionInput[i].Comments ?? ""
            };
            submissionList.Add(submission);
        }

        try
        {
            var appliedRecords = await _kpiSubmissionService.CreateRangeAsync(submissionList);
            TempData["TargetKpiPeriodName"] = TargetKpiPeriodName;

            return RedirectToPage("Success");

        }
        catch (DuplicateContentException)
        {
            ModelState.AddModelError("", "Already submitted for current period.");
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return Page();
        }
    }

    public IActionResult OnPostCancel()
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }
        return RedirectToPage("./Index");
    }

    // ========== Methods ==================================================
    // private async Task<bool> GetKpiPeriodId(string kpiPeriodName)
    // {
    //     try
    //     {
    //         var periodId = await _kpiPeriodService.FindIdByKpiPeriodNameAsync(kpiPeriodName);

    //         if (periodId > 0)
    //         {
    //             IsSubmissionValid = true;
    //             TargetKpiPeriodId = periodId;

    //             return true;
    //         }
    //         return false;
    //     }
    //     catch (MetricsNotFoundException ex)
    //     {
    //         ModelState.AddModelError("", ex.Message);
    //         IsSubmissionValid = false;
    //         return false;
    //     }
    //     catch (Exception)
    //     {
    //         ModelState.AddModelError("", $"Unable to find {kpiPeriodName}.");
    //         IsSubmissionValid = false;
    //         return false;
    //     }
    // }

    private async Task<bool> SetKpiPeriodId(string kpiPeriodName)
    {
        var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(kpiPeriodName);

        if (kpiPeriod != null)
        {
            IsSubmissionValid = true;
            TargetKpiPeriodId = kpiPeriod.Id;

            return true;
        }

        ModelState.AddModelError("", $"Submission not found for the period {kpiPeriodName}.");
        IsSubmissionValid = false;

        return false;
    }

    // private async Task<bool> GetIsSubmissionValid(DateTimeOffset submissionTime)
    // {
    //     var kpiPeriods = await _kpiPeriodService.FindAllByDateAsync(submissionTime.UtcDateTime);

    //     if (!kpiPeriods.Any())
    //     {
    //         ModelState.AddModelError("", "Submission not avaiable.");
    //         IsSubmissionValid = false;

    //         return false;
    //     }

    //     IsSubmissionValid = true;
    //     return true;
    // }

    // private async Task<long> GetEmployeeId()
    // {
    //     // Less likely to cause user not found, so throw just in case
    //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
    //         ?? throw new Exception("User not found. Please login again.");

    //     return await _userService.FindByIdAsync(userId);
    // }

    private async Task<ApplicationUser> GetCurrentUser()
    {
        // Less likely to cause user not found, so throw just in case
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("User not found. Please login again.");

        return await _userService.FindByIdAsync(userId);
    }

    private async Task<List<DepartmentModel>> GetDepartmentList(string userId)
    {
        // find Department ID of the Employee by ID
        // findDepartmentIdByEmployeeId()
        var employee = await _userService.FindByIdAsync(userId);
        var excludedDepartmentId = employee?.DepartmentId;
        var departments = await _departmentService.FindAllAsync();
        if (departments.Any())
        {
            return departments
                .Where(d => d.Id != excludedDepartmentId)
                .Select(e => new DepartmentModel
                {
                    Id = e.Id,
                    DepartmentCode = e.DepartmentCode,
                    DepartmentName = e.DepartmentName
                }).ToList();
        }

        ModelState.AddModelError("", "No departments to submit score. Please contact authorities.");
        return [];
    }

    private async Task<List<KpiSubmission>> GetExistingSubmissions(List<DepartmentModel> departments)
    {
        var departmentIDs = departments.Select(department => department.Id).ToList<long>();
        var existingSubmissions = await _kpiSubmissionService
            .FindBySubmitterAndKpiPeriodAndDepartmentListAsync(
                Submitter,
                TargetKpiPeriodId,
                departmentIDs);

        return existingSubmissions;
    }


    /// <summary>
    /// Update the Department List (when part of previous submissions found)
    ///     CASE: User already have submitted the submission previously. Right fter 
    ///             that a new department was added which makes submissions not fulfilled.
    ///             user can submit for the later added department score.
    ///             Filter DepartmentList only for new Submissions
    /// </summary>
    /// <param name="departmentList"></param>
    /// <param name="existingSubmissions"></param>
    /// <returns></returns>
    private Task<List<DepartmentModel>> UpdateDepartmentList(
        List<DepartmentModel> departmentList,
        List<KpiSubmission> existingSubmissions)
    {
        // 
        // 
        /* 
                    
                    
        */
        var result = departmentList
            .Where(d => !existingSubmissions.Any(s => s.DepartmentId == d.Id))
            .ToList();

        return Task.FromResult(result);
        // ============================================================
        // var a = from d in DepartmentList
        //         join s in existingSubmissions
        //         on d.Id equals s.DepartmentId into submissionsGroup
        //         from s in submissionsGroup.DefaultIfEmpty() // Left join
        //         where s == null // Filter for departments with no submissions
        //         select d;
        // DepartmentList = a.ToList();
        // ============================================================
    }
}

// TODO: Unused method
// private async Task<List<SelectListItem>> LoadKpiPeriodListItems()
// {
//     var kpiPeriods = await _kpiPeriodService.FindAllByDateAsync(DateTimeOffset.UtcNow);

//     if (!kpiPeriods.Any())
//         return [];

//     return kpiPeriods.Select(e => new SelectListItem
//     {
//         Value = e.Id.ToString(),
//         Text = $"{e.PeriodName} ({e.SubmissionStartDate:dd/MMM/yyyy} - {e.SubmissionEndDate:dd/MMM/yyyy})"
//     }).ToList();
// }
// }
