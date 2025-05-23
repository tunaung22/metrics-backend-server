using Metrics.Application.Domains;
using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.Departments.Scores;

[Authorize(Policy = "CanSubmitScorePolicy")]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionService _kpiSubmissionService;
    private readonly IKpiSubmissionPeriodService _kpiSubmissionPeriodService;

    public IndexModel(
        IUserService userService,
        IDepartmentService departmentService,
        IKpiSubmissionService kpiSubmissionService,
        IKpiSubmissionPeriodService kpiSubmissionPeriodService)
    {
        _userService = userService;
        _departmentService = departmentService;
        _kpiSubmissionService = kpiSubmissionService;
        _kpiSubmissionPeriodService = kpiSubmissionPeriodService;
    }

    public class KpiSubmissionPeriodModel
    {
        // public long Id { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public DateTimeOffset SubmissionStartDate { get; set; }
        public DateTimeOffset SubmissionEndDate { get; set; }
        public bool IsSubmitted { get; set; } = false;
    }

    public List<KpiSubmissionPeriodModel> KpiSubmissionPeriods { get; set; } = []; // model for rendering table

    public DateTimeOffset SubmissionTime { get; set; } = DateTimeOffset.UtcNow;
    public List<KpiSubmissionPeriod> KpiPeriodList { get; set; } = [];
    public bool IsSubmissionAvaiable { get; set; } = false;

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public long TotalSubmissions { get; set; } // Count

    public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalSubmissions, PageSize));
    public bool ShowPrevious => CurrentPage > 1;
    public bool ShowNext => CurrentPage < TotalPages;


    public async Task<IActionResult> OnGet()
    {

        // var listItems = await LoadKpiPeriodListItems();
        var listItems = await LoadKpiSubmissionPeriodListItems();
        if (listItems.Count <= 0)
        {
            // ModelState.AddModelError("", "No KPI Periods Avaiable Currently");
            IsSubmissionAvaiable = false;
            return Page();
        }
        KpiPeriodList = listItems;
        IsSubmissionAvaiable = true;

        // which period is already submitted?
        /*
            - select user_id
            - select * periods:
            - loop->
                - select COUNT:: departments as total_departments
                    - if total_departments
                        - select * kpi_submissions 
                          where user_id == current_user_id &&
                                period_id == current_period_id:
                        - loop->
                            - select COUNT:: kpi_submissions as total_submissions
                                - if total_departments != total_submissions:
                                    - incomplete
                                - else:
                                    - complete
        */

        KpiSubmissionPeriods = [];

        var currentUser = await GetCurrentUser();
        for (int i = 0; i < KpiPeriodList.Count; i++)
        {
            //department count
            var departmentCount = await _departmentService.FindCountAsync();
            // submission count by user
            var submissionCount = await _kpiSubmissionService
                .FindCountByUserIdByKpiPeriodIdAsync(currentUser.Id, KpiPeriodList[i].Id);
            if (departmentCount != submissionCount)
            {
                // KpiPeriodList[i] = incomplete
                KpiSubmissionPeriods.Add(new KpiSubmissionPeriodModel
                {
                    PeriodName = KpiPeriodList[i].PeriodName,
                    SubmissionStartDate = KpiPeriodList[i].SubmissionStartDate,
                    SubmissionEndDate = KpiPeriodList[i].SubmissionEndDate,
                    IsSubmitted = false
                });
            }
            else
            {
                // KpiPeriodList[i] = complete
                KpiSubmissionPeriods.Add(new KpiSubmissionPeriodModel
                {
                    PeriodName = KpiPeriodList[i].PeriodName,
                    SubmissionStartDate = KpiPeriodList[i].SubmissionStartDate,
                    SubmissionEndDate = KpiPeriodList[i].SubmissionEndDate,
                    IsSubmitted = true
                });
            }

        }


        return Page();
    }


    private async Task<List<KpiPeriodGetDto>> LoadKpiPeriodListItems()
    {
        var kpiPeriods = await _kpiSubmissionPeriodService.FindAllByDateAsync(SubmissionTime = DateTimeOffset.UtcNow);

        if (!kpiPeriods.Any())
            return [];

        return kpiPeriods.Select(e => new KpiPeriodGetDto
        {
            PeriodName = e.PeriodName,
            SubmissionStartDate = e.SubmissionStartDate,
            SubmissionEndDate = e.SubmissionEndDate
        }).ToList();
    }

    private async Task<List<KpiSubmissionPeriod>> LoadKpiSubmissionPeriodListItems()
    {
        var kpiPeriods = await _kpiSubmissionPeriodService.FindAllAsync();

        if (!kpiPeriods.Any())
            return [];

        return kpiPeriods.ToList();
    }


    private async Task<ApplicationUser> GetCurrentUser()
    {
        // Less likely to cause user not found, so throw just in case
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("User not found. Please login again.");

        return await _userService.FindByIdAsync(userId);
    }
}
