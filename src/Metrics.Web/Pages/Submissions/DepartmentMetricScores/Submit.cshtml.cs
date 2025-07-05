using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.DepartmentMetricScores;

[Authorize(Policy = "CanSubmitKeyScorePolicy")]
public class SubmitModel : PageModel
{
    private readonly IKeyKpiSubmissionService _keyMetricSubmissionService;
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IUserService _userService;
    private readonly IDepartmentKeyMetricService _departmentKeyMetricService;

    public SubmitModel(
        IDepartmentService departmentService,
        IKpiSubmissionPeriodService kpiPeriodService,
        IKeyKpiSubmissionService keyMetricSubmissionService,
        IUserService userService,
        IDepartmentKeyMetricService departmentKeyMetricService)
    {
        _departmentService = departmentService;
        _kpiPeriodService = kpiPeriodService;
        _keyMetricSubmissionService = keyMetricSubmissionService;
        _userService = userService;
        _departmentKeyMetricService = departmentKeyMetricService;
    }

    // =========================================================================

    // =============== MODELS ==================================================
    public class SubmissionInputModel
    {
        public DateTimeOffset SubmittedAt { get; set; }

        public long KpiPeriodId { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public long DepartmentId { get; set; }

        public List<KeyKpiSubmissionItemInputModel> KeyKpiSubmissionItems { get; set; } = [];
    }

    public class KeyKpiSubmissionItemInputModel
    {
        public long DepartmentKeyMetricsId { get; set; }

        [Required(ErrorMessage = "Score is required.")]
        [Range(1, 10, ErrorMessage = "Score is required and choose between 1 to 10.")]
        public decimal ScoreValue { get; set; }

        public string? Comments { get; set; } = string.Empty;

    }

    [BindProperty]
    public List<SubmissionInputModel> SubmissionInput { get; set; } = [];


    public class KpiPeriodViewModel
    {
        public long Id { get; set; }
        public string PeriodName { get; set; } = null!;
    }
    public KpiPeriodViewModel TargetKpiPeriod { get; set; } = null!;

    public class DepartmentViewModel
    {
        public long Id { get; set; }
        public Guid DepartmentCode { get; set; }
        public string DepartmentName { get; set; } = null!;
    }
    public List<DepartmentViewModel> DepartmentList { get; set; } = [];

    public class DepartmentKeyMetricViewModel
    {
        public long Id { get; set; }
        public Guid DepartmentKeyMetricCode { get; set; }
        public long KpiSubmissionPeriodId { get; set; }
        public long DepartmentId { get; set; }
        public long KeyMetricId { get; set; }
        public KpiSubmissionPeriod KpiSubmissionPeriod { get; set; } = null!;
        public Department TargetDepartment { get; set; } = null!;
        public KeyMetric KeyMetric { get; set; } = null!;
    }
    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];

    public ApplicationUser Submitter
    { get; set; } = null!;

    public string TargetKpiPeriodName { get; set; } = null!;
    public string? CurrentUserGroupName { get; set; } = string.Empty;

    public bool IsSubmissionValid { get; set; } = false; // check is current date not early or late
    public bool IsSubmissionsExist { get; set; } = false;

    public string? ReturnUrl { get; set; } = string.Empty;
    // =========================================================================


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(string periodName, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
            ReturnUrl = returnUrl;

        TargetKpiPeriodName = periodName;
        var kpiPeriod = await _kpiPeriodService.FindByKpiPeriodNameAsync(TargetKpiPeriodName);
        if (kpiPeriod == null)
        {
            ModelState.AddModelError(string.Empty, $"Submission not found for the period {TargetKpiPeriodName}.");
            IsSubmissionValid = false;
            return Page();
        }
        TargetKpiPeriod = new KpiPeriodViewModel
        {
            Id = kpiPeriod.Id,
            PeriodName = kpiPeriod.PeriodName
        };

        // ---------- Check Today Submission is Valid based on KPI Period ------
        // ---------- CHECK TARGET PERIOD IS VALID OR NOT ----------------------
        var UTC_NOW = DateTimeOffset.UtcNow;
        if (UTC_NOW < kpiPeriod.SubmissionStartDate
            || UTC_NOW > kpiPeriod.SubmissionEndDate)
        {
            // EARLY
            if (UTC_NOW < kpiPeriod.SubmissionStartDate)
                ModelState.AddModelError(string.Empty, "Invalid submission. This submission is not ready yet.");
            // LATE
            if (UTC_NOW > kpiPeriod.SubmissionEndDate)
                ModelState.AddModelError(string.Empty, "Invalid submission. This submission is due.");

            IsSubmissionValid = false;
            return Page();
        }

        IsSubmissionValid = true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User is not found.");
        var submitter = await _userService.FindByIdAsync(userId);

        if (submitter == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid current user.");
            return Page();
        }
        Submitter = submitter;
        CurrentUserGroupName = Submitter.UserTitle?.TitleName ?? string.Empty;

        var excludedDepartmentId = submitter.DepartmentId; // exclude user's own department
        DepartmentList = (await _departmentService.FindAllAsync())
            .Where(d => d.Id != excludedDepartmentId)
            .Select(d => new DepartmentViewModel
            {
                Id = d.Id,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName
            })
            .ToList();

        if (DepartmentList == null)
        {
            ModelState.AddModelError("", "No departments to submit score. Please contact authorities.");
            return Page();
        }

        // Load Key Metrics by period 
        DepartmentKeyMetrics = [];
        foreach (var department in DepartmentList)
        {
            // fetch key metrics by department
            var keyMetrics = await _departmentKeyMetricService
                .FindAllByPeriodAndDepartmentAsync(TargetKpiPeriod.PeriodName, department.DepartmentCode);
            if (keyMetrics.Any())
            {
                DepartmentKeyMetrics = keyMetrics.Select(k => new DepartmentKeyMetricViewModel
                {
                    Id = k.Id,
                    DepartmentKeyMetricCode = k.DepartmentKeyMetricCode,
                    KpiSubmissionPeriodId = k.KpiSubmissionPeriodId,
                    DepartmentId = k.DepartmentId,
                    KeyMetricId = k.KeyMetricId,
                    KpiSubmissionPeriod = k.KpiSubmissionPeriod,
                    TargetDepartment = k.TargetDepartment,
                    KeyMetric = k.KeyMetric
                }).ToList();
            }
            DepartmentKeyMetrics = [];
        }

        // CASES:
        // 1. (ALL SUBMISSIONS EXIST) all submissions already exist/fullfilled, no need to submit anymore.
        // 2. (PART OF SUBMISSIONS EXIST) part of previous submissions found, but can submit for other departments that were added later time after first submission.
        // 3. (NO PREVIOUS EXIST) no previous submissions found, need to submit.
        // var existingSubmissions = await GetExistingSubmissions(DepartmentList);
        var departmentIDs = DepartmentList.Select(department => department.Id).ToList<long>();
        var existingSubmissions = await _keyMetricSubmissionService
            .FindBySubmitterByPeriodByDepartmentListAsync(
                Submitter,
                TargetKpiPeriod.Id,
                departmentIDs);
        if (existingSubmissions.Count > 0)
        {
            // 1. 2.
            // submissions always likely have more than department (as department can get deleted) ???
            if (DepartmentList.Count <= existingSubmissions.Count)
            {
                // Submission Exist
                IsSubmissionsExist = true;
                // fetch submitted data for display
                // Find submissions by Submitter and Period
            }
        }
        else
        {
            // 3.
            // SubmissionInput = new List<SubmissionInputModel>();
            SubmissionInput = DepartmentList.Select(d => new SubmissionInputModel
            {

            }).ToList();
        }

        return Page();
    }

    // =============== METHODS =================================================

}
