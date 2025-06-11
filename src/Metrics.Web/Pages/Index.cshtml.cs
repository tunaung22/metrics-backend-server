using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IKpiSubmissionPeriodService _kpiSubmissionPeriodService;
    public IndexModel(
        IUserService userService,
        IKpiSubmissionPeriodService kpiSubmissionPeriodService
        )
    {
        _userService = userService;
        _kpiSubmissionPeriodService = kpiSubmissionPeriodService;
    }

    public string? FullName { get; set; } = string.Empty;
    public int PendingSubmission { get; set; }

    public List<KpiSubmissionPeriod> PeriodList { get; set; } = [];
    public async Task OnGet()
    {

        if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
        {
            var username = User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _userService.FindByUsernameAsync(username);
                FullName = user.FullName;
            }
        }

        // Get pending submissions count
        // 1. Get list of Valid Periods
        // 2. Loop Valid Periods
        // 3. ... Get Department Count
        // 4. ... Get Submissions Count
        // 5. ... Check Status
        // 6. ... Update PendingSubmission
        // PendingSubmission = 4;

        // 1. Get list of Valid Periods
        var periods = await _kpiSubmissionPeriodService.FindAllByDateAsync(DateTimeOffset.UtcNow);
        PeriodList = periods.ToList();


        var kpiPeriods = await _kpiSubmissionPeriodService.FindAllAsync();
        var KpiPeriodList = kpiPeriods.ToList();
        if (KpiPeriodList.Any())
        {

        }

    }
}
