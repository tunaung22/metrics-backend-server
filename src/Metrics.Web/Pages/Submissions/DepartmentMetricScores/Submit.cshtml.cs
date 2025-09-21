using Metrics.Application.Authorization;
using Metrics.Application.Domains;
using Metrics.Application.DTOs;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
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
    private readonly IDepartmentKeyMetricService _departmentKeyMetricService = departmentKeyMetricService;
    private readonly IKeyKpiSubmissionConstraintService _keyKpiSubmissionConstraintService = keyKpiSubmissionConstraintService;
    private readonly IKeyKpiSubmissionService _keyKpiSubmissionService = keyKpiSubmissionService;

    // =============== MODELS ==================================================

    // --------------- View ----------------------------------------------------
    public class FinishedSubmissionViewModel
    {
        public DateOnly SubmissionDate { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
        // public long KpiPeriodId { get; set; }
        // public string SubmitterId { get; set; } = null!;
        // public long TargetDepartmentId { get; set; }
        public string TargetDepartmentName { get; set; } = null!;
        public List<ExistingSubmissionItemViewModel> SubmissionInputItems { get; set; } = [];
    }

    public class ExistingSubmissionItemViewModel
    {
        // public long DepartmentKeyMetricsId { get; set; }
        public DepartmentKeyMetricViewModel DepartmentKeyMetrics { get; set; } = null!;
        public decimal ScoreValue { get; set; }
        public string? Comments { get; set; } = string.Empty;
    }

    public IEnumerable<FinishedSubmissionViewModel> FinishedSubmissions { get; set; } = [];


    // --------------- Input ---------------------------------------------------

    public class SubmissionInputModel
    {
        public DateTimeOffset SubmittedAt { get; set; }
        public long KpiPeriodId { get; set; }
        public string SubmitterId { get; set; } = null!;
        public long TargetDepartmentId { get; set; }
        public List<KeyKpiSubmissionInputItemModel> SubmissionInputItems { get; set; } = [];
    }

    public class KeyKpiSubmissionInputItemModel
    {
        public long DepartmentKeyMetricsId { get; set; }
        [Required(ErrorMessage = "Score is required.")]
        [Range(1, 10, ErrorMessage = "Score is required and choose between 1 to 10.")]
        public decimal ScoreValue { get; set; }
        public string? Comments { get; set; } = string.Empty;
    }

    [BindProperty]
    public List<SubmissionInputModel> SubmissionInputs { get; set; } = [];

    // -------------------------------------------------------------------------
    public KpiPeriodViewModel TargetKpiPeriod { get; set; } = null!;

    public List<DepartmentViewModel> DepartmentList { get; set; } = [];

    public class DepartmentKeyMetricViewModel
    {
        public long Id { get; set; }
        public Guid DepartmentKeyMetricCode { get; set; }
        public long KpiSubmissionPeriodId { get; set; }
        public long DepartmentId { get; set; }
        public long KeyMetricId { get; set; }
        // public KpiPeriodViewModel KpiSubmissionPeriod { get; set; } = null!;
        public KpiSubmissionPeriod KpiSubmissionPeriod { get; set; } = null!;
        // public DepartmentViewModel TargetDepartment { get; set; } = null!;
        public Department TargetDepartment { get; set; } = null!;
        // public KeyMetricViewModel KeyMetric { get; set; } = null!;
        public KeyMetric KeyMetric { get; set; } = null!;
    }
    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];

    public class KeyKpiSubmissionConstraintViewModel
    {
        public long Id { get; set; }
        public Guid LookupId { get; set; }
        public long DepartmentId { get; set; }
        public DepartmentViewModel SubmitterDepartment { get; set; } = null!;
        public long DepartmentKeyMetricId { get; set; }
        public DepartmentKeyMetricViewModel DepartmentKeyMetric { get; set; } = null!;
    }

    public List<KeyKpiSubmissionConstraintViewModel> KeyKpiSubmissionConstraints { get; set; } = [];

    // -----Submitter-----
    public ApplicationUser Submitter { get; set; } = null!;
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

        // ---------- KPI Period -----------------------------------------------
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "Submission Period is required!");
            return Page();
        }


        var kpiPeriod = await _kpiPeriodService
            .FindByKpiPeriodNameAsync(periodName);
        if (kpiPeriod == null)
        {
            ModelState.AddModelError(string.Empty, $"Invalid submission period: {TargetKpiPeriodName}.");
            IsSubmissionValid = false;
            return Page();
        }

        TargetKpiPeriodName = kpiPeriod.PeriodName;
        TargetKpiPeriod = new KpiPeriodViewModel
        {
            Id = kpiPeriod.Id,
            PeriodName = kpiPeriod.PeriodName,
            SubmissionStartDate = kpiPeriod.SubmissionStartDate,
            SubmissionEndDate = kpiPeriod.SubmissionEndDate
        };

        // ---------- Check Today Submission is Valid based on KPI Period ------
        // ---------- CHECK TARGET PERIOD IS VALID OR NOT ----------------------
        IsSubmissionValid = CheckSubmissionValidity(TargetKpiPeriod);

        // ---------- SUBMITTER ------------------------------------------------
        var submitter = await LoadSubmitter();
        if (submitter == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid user.");
            return Page();
        }

        Submitter = submitter;
        CurrentUserGroupName = submitter.UserTitle?.TitleName ?? string.Empty;
        SubmitterDepartment = new DepartmentViewModel
        {
            Id = submitter.Department.Id,
            DepartmentCode = submitter.Department.DepartmentCode,
            DepartmentName = submitter.Department.DepartmentName
        };


        // SubmitterDepartment ကအမှတ်ပေးလို့ရတဲ့ keys တွေရှာ
        // from Submission Constraints, get key metrics issued by departments
        // ဘယ် department က ဘယ် department ရဲ့ key metrics တွေကို အမှတ်ပေးရမလဲ ဆိုတာကိုဆွဲထုတ်
        var keyKpiSubmissionConstraints = await _keyKpiSubmissionConstraintService
            .FindAllByPeriodBySubmitterDepartmentAsync(
                TargetKpiPeriod.Id,
                SubmitterDepartment.Id
            );

        if (keyKpiSubmissionConstraints.Any())
        {
            // EXTRACT DATA for navigational properties
            // MAP KeyKpiSubmissionConstraint to KeyKpiSubmissionConstraintViewModel
            KeyKpiSubmissionConstraints = keyKpiSubmissionConstraints
                // .Where(c =>
                //     c.DepartmentKeyMetric.KpiSubmissionPeriodId == TargetKpiPeriod.Id)
                .Select(c => new KeyKpiSubmissionConstraintViewModel
                {
                    Id = c.Id,
                    LookupId = c.LookupId,
                    DepartmentId = c.DepartmentId, // submitter's department
                    SubmitterDepartment = new DepartmentViewModel // submitter's department
                    {
                        Id = c.Department.Id,
                        DepartmentCode = c.Department.DepartmentCode,
                        DepartmentName = c.Department.DepartmentName
                    },
                    DepartmentKeyMetricId = c.DepartmentKeyMetricId, // DKM
                    DepartmentKeyMetric = new DepartmentKeyMetricViewModel // DKM
                    {
                        Id = c.DepartmentKeyMetric.Id,
                        DepartmentKeyMetricCode = c.DepartmentKeyMetric.DepartmentKeyMetricCode, //lookup id
                        KpiSubmissionPeriodId = c.DepartmentKeyMetric.KpiSubmissionPeriodId,
                        DepartmentId = c.DepartmentKeyMetric.DepartmentId,
                        KeyMetricId = c.DepartmentKeyMetric.KeyMetricId,

                        KpiSubmissionPeriod = c.DepartmentKeyMetric.KpiSubmissionPeriod,
                        TargetDepartment = c.DepartmentKeyMetric.KeyIssueDepartment,
                        KeyMetric = c.DepartmentKeyMetric.KeyMetric,
                    }
                })
                .ToList();

            if (KeyKpiSubmissionConstraints.Count > 0) // DepartmentList, DepartmentKeyMetrics
            {
                // ----- Submission are based on DepartmentKeyMetric (which is defined at Constraints table) -----

                // ---------- DEPARTMENT -----------------------------------
                // ----- Get DISTINCT Departments from Constraints.DepartmentKeyMetrics -----
                // အမှတ်ပေးရမည့် departments များ (Issuer Departments)
                DepartmentList = KeyKpiSubmissionConstraints
                    .Select(d => d.DepartmentKeyMetric.TargetDepartment)
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
                    .OrderBy(c => c.DepartmentId) // Key Issuer
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
                var existingSubmissions = await _keyKpiSubmissionService
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
                                DepartmentKeyMetrics = new DepartmentKeyMetricViewModel
                                {
                                    Id = i.DepartmentKeyMetric.Id,
                                    DepartmentKeyMetricCode = i.DepartmentKeyMetric.DepartmentKeyMetricCode,
                                    DepartmentId = i.DepartmentKeyMetric.DepartmentId,
                                    TargetDepartment = i.DepartmentKeyMetric.KeyIssueDepartment,
                                    KeyMetricId = i.DepartmentKeyMetric.KeyMetricId,
                                    KeyMetric = i.DepartmentKeyMetric.KeyMetric
                                },
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
                            TargetDepartmentId = department.Id,
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
                        TargetDepartmentId = department.Id,
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

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string periodName)
    {
        // ---------- KPI Period -----------------------------------------------
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "Submission Period is required!");
            return Page();
        }
        TargetKpiPeriodName = periodName;

        var kpiPeriod = await _kpiPeriodService
                    .FindByKpiPeriodNameAsync(TargetKpiPeriodName);
        if (kpiPeriod == null)
        {
            ModelState.AddModelError(string.Empty, $"Invalid submission period: {TargetKpiPeriodName}.");
            IsSubmissionValid = false;
            return Page();
        }

        TargetKpiPeriod = new KpiPeriodViewModel
        {
            Id = kpiPeriod.Id,
            PeriodName = kpiPeriod.PeriodName,
            SubmissionStartDate = kpiPeriod.SubmissionStartDate,
            SubmissionEndDate = kpiPeriod.SubmissionEndDate
        };

        // ---------- Check Today Submission is Valid based on KPI Period ------
        // ---------- CHECK TARGET PERIOD IS VALID OR NOT ----------------------
        IsSubmissionValid = CheckSubmissionValidity(TargetKpiPeriod);


        // ---------- SUBMITTER ------------------------------------------------
        var submitter = await LoadSubmitter();
        if (submitter == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid user.");
            return Page();
        }

        Submitter = submitter;
        CurrentUserGroupName = submitter.UserTitle?.TitleName ?? string.Empty;
        SubmitterDepartment = new DepartmentViewModel
        {
            Id = submitter.Department.Id,
            DepartmentCode = submitter.Department.DepartmentCode,
            DepartmentName = submitter.Department.DepartmentName
        };


        if (SubmissionInputs != null)
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

                    var success = await _keyKpiSubmissionService.SubmitScoreAsync(submissionsCreateDtos);
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
            ModelState.AddModelError("", "Invalid form inputs.");
        }

        return Page();
    }

    // =============== METHODS =================================================
    private bool CheckSubmissionValidity(KpiPeriodViewModel period)
    {
        var currentDateTime = DateTimeOffset.UtcNow;

        if (currentDateTime < period.SubmissionStartDate
            || currentDateTime > period.SubmissionEndDate)
        {
            // EARLY
            if (currentDateTime < period.SubmissionStartDate)
                ModelState.AddModelError(string.Empty, "Invalid submission. This submission is not ready yet.");
            // LATE
            if (currentDateTime > period.SubmissionEndDate)
                ModelState.AddModelError(string.Empty, "Invalid submission. This submission is due.");

            return false;
        }

        return true;
    }

    private async Task<ApplicationUser?> LoadSubmitter()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User is not found.");

        return await _userService.FindByIdAsync(userId);
    }

    private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeyMetrics(
        List<DepartmentViewModel> departmentList)
    {
        // Load Key Metrics by period 
        foreach (var department in departmentList)
        {
            List<DepartmentKeyMetricViewModel> tmpViewModel = [];

            // fetch key metrics by period by department
            var departmentKeyMetrics = await _departmentKeyMetricService
                .FindAllByPeriodAndDepartmentAsync(TargetKpiPeriod.PeriodName, department.DepartmentCode);
            if (departmentKeyMetrics.Count() > 0)
            {
                var dkms = departmentKeyMetrics
                    // filter by constraint
                    // constraint.department == dkm.department == departmentList.department
                    .Where(k =>
                        KeyKpiSubmissionConstraints.Select(c => c.DepartmentKeyMetric.DepartmentId)
                            .Contains(k.KeyIssueDepartment.Id)
                            && k.DepartmentId == department.Id)
                    .Select(k => new DepartmentKeyMetricViewModel
                    {
                        Id = k.Id,
                        DepartmentKeyMetricCode = k.DepartmentKeyMetricCode,
                        KpiSubmissionPeriodId = k.KpiSubmissionPeriodId,
                        DepartmentId = k.DepartmentId,
                        KeyMetricId = k.KeyMetricId,

                        // KpiSubmissionPeriod = k.DepartmentKeyMetric.KpiSubmissionPeriod,
                        // TargetDepartment = k.DepartmentKeyMetric.TargetDepartment,
                        // KeyMetric = k.DepartmentKeyMetric.KeyMetric

                        // KpiSubmissionPeriod = new KpiPeriodViewModel
                        // {
                        //     Id = k.KpiSubmissionPeriod.Id,
                        //     PeriodName = k.KpiSubmissionPeriod.PeriodName
                        // },
                        // TargetDepartment = new DepartmentViewModel
                        // {
                        //     Id = k.TargetDepartment.Id,
                        //     DepartmentCode = k.TargetDepartment.DepartmentCode,
                        //     DepartmentName = k.TargetDepartment.DepartmentName
                        // },
                        // KeyMetric = new KeyMetricViewModel
                        // {
                        //     MetricTitle = k.KeyMetric.MetricTitle
                        // },

                    }).ToList();
                tmpViewModel.AddRange(dkms);
                return tmpViewModel;
            }
            return [];
        }

        return [];
    }
}