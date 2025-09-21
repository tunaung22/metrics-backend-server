using Metrics.Application.Authorization;
using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.DepartmentCaseFeedback;

[Authorize(Policy = ApplicationPolicies.CanGiveFeedbackPolicy)]
public class IndexModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;

    public IndexModel(IKpiSubmissionPeriodService kpiPeriodService)
    {
        _kpiPeriodService = kpiPeriodService;
    }

    // ========== MODELS =======================================================
    public class KpiSubmissionPeriodModel // Overview info of submission for the Period
    {
        public long Id { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public DateTimeOffset SubmissionStartDate { get; set; }
        public DateTimeOffset SubmissionEndDate { get; set; }
        public bool IsValid { get; set; } = false;
    }
    public List<KpiSubmissionPeriodModel> KpiPeriodList { get; set; } = [];
    // public List<KpiPeriodViewModel> KpiPeriodList { get; set; } = [];

    // ========== HANDLERS =====================================================
    public async Task<IActionResult> OnGetAsync()
    {
        var periods = await _kpiPeriodService.FindAllAsync();
        if (periods.Any())
        {
            KpiPeriodList = periods
                .Select(p => new KpiSubmissionPeriodModel()
                {
                    Id = p.Id,
                    PeriodName = p.PeriodName,
                    SubmissionStartDate = p.SubmissionStartDate.DateTime,
                    SubmissionEndDate = p.SubmissionEndDate.DateTime,
                    IsValid = DateTimeOffset.Now.UtcDateTime >= p.SubmissionStartDate.DateTime
                            && DateTimeOffset.Now.UtcDateTime <= p.SubmissionEndDate.DateTime
                })
                .ToList();
        }

        return Page();
    }

    // ========== METHODS ======================================================
}
