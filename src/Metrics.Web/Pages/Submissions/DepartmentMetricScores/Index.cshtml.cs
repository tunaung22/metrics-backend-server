using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.DepartmentMetricScores;

[Authorize(Policy = "CanSubmitKeyScorePolicy")]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;
    private readonly IKeyKpiSubmissionService _keyMetricSubmissionService;
    private readonly IKpiSubmissionPeriodService _kpiSubmissionPeriodService;

    public IndexModel(
            IUserService userService,
            IDepartmentService departmentService,
            IKeyKpiSubmissionService keyMetricSubmissionService,
            IKpiSubmissionPeriodService kpiSubmissionPeriodService)
    {
        _userService = userService;
        _departmentService = departmentService;
        _keyMetricSubmissionService = keyMetricSubmissionService;
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
            var currentUserDepartmentCode = currentUser.Department.DepartmentCode;
            for (int i = 0; i < kpiPeriodList.Count; i++)
            {
                var departmentCount = (await _departmentService
                    .FindAllAsync())
                    .Where(d => d.DepartmentCode != currentUserDepartmentCode)
                    .Count();

                var submissionCount = await _keyMetricSubmissionService
                    .FindCountByUserByPeriodAsync(currentUser.Id, kpiPeriodList[i].Id);

                if (departmentCount == submissionCount)
                {
                    // ---------- COMPLETE ----------------------------------------
                    SubmissionPeriods.Add(new KpiSubmissionPeriodModel
                    {
                        PeriodName = kpiPeriodList[i].PeriodName,
                        SubmissionStartDate = kpiPeriodList[i].SubmissionStartDate,
                        SubmissionEndDate = kpiPeriodList[i].SubmissionEndDate,
                        IsSubmitted = true
                    });
                }
                else
                {
                    // ---------- INCOMPLETE -----------------------------------
                    // ----- departmentCound > submissionCount -----
                    // ----- departmentCound < submissionCount -----
                    SubmissionPeriods.Add(new KpiSubmissionPeriodModel
                    {
                        PeriodName = kpiPeriodList[i].PeriodName,
                        SubmissionStartDate = kpiPeriodList[i].SubmissionStartDate,
                        SubmissionEndDate = kpiPeriodList[i].SubmissionEndDate,
                        IsSubmitted = false,
                        IsValid = DateTimeOffset.Now.UtcDateTime > kpiPeriodList[i].SubmissionStartDate
                            && DateTimeOffset.Now.UtcDateTime < kpiPeriodList[i].SubmissionEndDate
                    });
                }
            }

        }

        return Page();
    }

    // ========== METHODS ======================================================

}
