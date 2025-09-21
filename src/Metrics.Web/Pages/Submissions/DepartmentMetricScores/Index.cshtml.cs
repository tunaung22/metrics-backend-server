using Metrics.Application.Authorization;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.DepartmentMetricScores;

[Authorize(Policy = ApplicationPolicies.CanSubmitKeyKpiScorePolicy)]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;
    private readonly IKeyKpiSubmissionService _keyKpiSubmissionService;
    private readonly IKeyKpiSubmissionConstraintService _keyKpiSubmissionConstraintService;
    private readonly IKpiSubmissionPeriodService _kpiSubmissionPeriodService;

    public IndexModel(
            IUserService userService,
            IDepartmentService departmentService,
            IKeyKpiSubmissionService keyKpiSubmissionService,
            IKeyKpiSubmissionConstraintService keyKpiSubmissionConstraintService,
    IKpiSubmissionPeriodService kpiSubmissionPeriodService)
    {
        _userService = userService;
        _departmentService = departmentService;
        _keyKpiSubmissionService = keyKpiSubmissionService;
        _keyKpiSubmissionConstraintService = keyKpiSubmissionConstraintService;
        _kpiSubmissionPeriodService = kpiSubmissionPeriodService;
    }
    // ========== MODELS =======================================================
    public class KpiSubmissionPeriodModel // Overview info of submission for the Period
    {
        // public long Id { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public DateTimeOffset SubmissionStartDate { get; set; }
        public DateTimeOffset SubmissionEndDate { get; set; }
        public bool IsSubmitted { get; set; } = false;
        public bool IsValid { get; set; } = false;
    }
    public List<KpiSubmissionPeriodModel> SubmissionPeriods { get; set; } = []; // model for KPI period table

    public bool IsSubmissionAvaiable { get; set; } = false;


    // ========== HANDLERS =====================================================
    public async Task<IActionResult> OnGetAsync()
    {
        var kpiPeriodList = (await _kpiSubmissionPeriodService.FindAllAsync()).ToList();
        if (kpiPeriodList.Count <= 0)
        {
            IsSubmissionAvaiable = false;
            return Page();
        }
        IsSubmissionAvaiable = true;

        SubmissionPeriods = [];

        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User is not authenticated.");
        var currentUser = await _userService.FindByIdAsync(userId);
        if (currentUser != null)
        {
            // Check submissions is completed
            // parent   item count :: can compare with department count (based on period)
            // child    item count :: can compare with DKM's department count (based on period)
            // foreach period:
            //          get submissions where submissions.period == period
            //              submission's items .departments
            // - where::submissionConstraints.DKM.period == period 
            // submission's department count == 
            // - submissions.department.count
            SubmissionPeriods = [];

            foreach (var period in kpiPeriodList)
            {
                var UTC_NOW = DateTimeOffset.Now.UtcDateTime;
                var isPeriodValid = UTC_NOW > period.SubmissionStartDate
                    && UTC_NOW < period.SubmissionEndDate;
                //  submission
                // var departmentList = await _departmentService.FindAllAsync();

                var submissionConstraints = await _keyKpiSubmissionConstraintService
                    .FindAllByPeriodBySubmitterDepartmentAsync(
                        period.Id,
                        currentUser.DepartmentId
                    );

                // constraints -> departments   -> existing Submissions
                // constraints -> dkms          -> submision status
                if (submissionConstraints.Any())
                {
                    var dkms = submissionConstraints
                        .OrderBy(c => c.DepartmentId)
                        .Select(c => c.DepartmentKeyMetric);
                    var departments = submissionConstraints
                        .Select(c => c.DepartmentKeyMetric.KeyIssueDepartment)
                        .DistinctBy(department => department.Id)
                        .ToList();
                    // DepartmentList = KeyKpiSubmissionConstraints
                    // .Select(d => d.DepartmentKeyMetric.TargetDepartment)
                    // .DistinctBy(department => department.Id)
                    // .Select(department => new DepartmentViewModel
                    // {
                    //     Id = department.Id,
                    //     DepartmentCode = department.DepartmentCode,
                    //     DepartmentName = department.DepartmentName
                    // })
                    // .ToList();

                    var existingSubmissions = await _keyKpiSubmissionService
                        .FindBySubmitterByPeriodByDepartmentListAsync(
                            currentUser,
                            period.Id,
                            departments.Select(d => d.Id).ToList()
                        );
                    if (existingSubmissions.Count > 0)
                    {
                        var existingDkms = existingSubmissions
                            .SelectMany(s => s.KeyKpiSubmissionItems)
                            .Select(s => s.DepartmentKeyMetric);

                        // finished or in-progress
                        if (dkms.Count() == existingDkms.Count())
                        {
                            // finished
                            SubmissionPeriods.Add(new KpiSubmissionPeriodModel
                            {
                                PeriodName = period.PeriodName,
                                SubmissionStartDate = period.SubmissionStartDate,
                                SubmissionEndDate = period.SubmissionEndDate,
                                IsSubmitted = true,
                                IsValid = isPeriodValid
                            });
                        }
                        else
                        {
                            // in-progress
                            SubmissionPeriods.Add(new KpiSubmissionPeriodModel
                            {
                                PeriodName = period.PeriodName,
                                SubmissionStartDate = period.SubmissionStartDate,
                                SubmissionEndDate = period.SubmissionEndDate,
                                IsSubmitted = false,
                                IsValid = isPeriodValid
                            });
                        }
                    }
                    else
                    {
                        // new submission
                        SubmissionPeriods.Add(new KpiSubmissionPeriodModel
                        {
                            PeriodName = period.PeriodName,
                            SubmissionStartDate = period.SubmissionStartDate,
                            SubmissionEndDate = period.SubmissionEndDate,
                            IsSubmitted = false,
                            IsValid = isPeriodValid
                        });
                    }

                }
                else
                {
                    // invalid submission (date is dued)
                    SubmissionPeriods.Add(new KpiSubmissionPeriodModel
                    {
                        PeriodName = period.PeriodName,
                        SubmissionStartDate = period.SubmissionStartDate,
                        SubmissionEndDate = period.SubmissionEndDate,
                        IsSubmitted = false,
                        IsValid = isPeriodValid
                    });
                }

            }

            // var currentUserDepartmentCode = currentUser.Department.DepartmentCode;
            // for (int i = 0; i < kpiPeriodList.Count; i++)
            // {
            //     var departmentCount = (await _departmentService
            //         .FindAllAsync())
            //         .Where(d => d.DepartmentCode != currentUserDepartmentCode)
            //         .Count();

            //     var submissionCount = await _keyKpiSubmissionService
            //         .FindCountByUserByPeriodAsync(currentUser.Id, kpiPeriodList[i].Id);

            //     if (departmentCount == submissionCount)
            //     {
            //         // ---------- COMPLETE ----------------------------------------
            //         SubmissionPeriods.Add(new KpiSubmissionPeriodModel
            //         {
            //             PeriodName = kpiPeriodList[i].PeriodName,
            //             SubmissionStartDate = kpiPeriodList[i].SubmissionStartDate,
            //             SubmissionEndDate = kpiPeriodList[i].SubmissionEndDate,
            //             IsSubmitted = true
            //         });
            //     }
            //     else
            //     {
            //         // ---------- INCOMPLETE -----------------------------------
            //         // ----- departmentCound > submissionCount -----
            //         // ----- departmentCound < submissionCount -----
            //         SubmissionPeriods.Add(new KpiSubmissionPeriodModel
            //         {
            //             PeriodName = kpiPeriodList[i].PeriodName,
            //             SubmissionStartDate = kpiPeriodList[i].SubmissionStartDate,
            //             SubmissionEndDate = kpiPeriodList[i].SubmissionEndDate,
            //             IsSubmitted = false,
            //             IsValid = DateTimeOffset.Now.UtcDateTime > kpiPeriodList[i].SubmissionStartDate
            //                 && DateTimeOffset.Now.UtcDateTime < kpiPeriodList[i].SubmissionEndDate
            //         });
            //     }
            // }

        }

        return Page();
    }

    // ========== METHODS ======================================================

}
