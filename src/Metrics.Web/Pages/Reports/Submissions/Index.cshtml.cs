using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Reports.Submissions;

public class IndexModel : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;

    private readonly IUserTitleService _userTitleService;

    public IndexModel(
        IKpiSubmissionPeriodService kpiPeriodService,
        IUserTitleService userTitleService)
    {
        _kpiPeriodService = kpiPeriodService;

        _userTitleService = userTitleService;
    }

    // ========== MODELS ==========
    public List<KpiPeriodViewModel> KpiPeriodList { get; set; } = [];
    public List<UserTitle> UserGroups { get; set; } = [];

    // ========== HANDLERS ==========
    public async Task<IActionResult> OnGetAsync()
    {
        // ---------- Load User Groups ------------------------------
        var userGroups = await _userTitleService.FindAllAsync();
        if (userGroups.Any())
            UserGroups = userGroups.ToList();

        var periods = await _kpiPeriodService.FindAllAsync();
        if (periods.Any())
        {
            KpiPeriodList = periods.Select(p => new KpiPeriodViewModel()
            {
                Id = p.Id,
                PeriodName = p.PeriodName,
                SubmissionStartDate = p.SubmissionStartDate,
                SubmissionEndDate = p.SubmissionEndDate
            }).ToList();
        }

        return Page();
    }
}
