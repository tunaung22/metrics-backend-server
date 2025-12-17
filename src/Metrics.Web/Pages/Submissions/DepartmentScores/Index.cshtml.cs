using Metrics.Application.Authorization;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.KpiPeriod;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Metrics.Web.Pages.Submissions.DepartmentScores;


[Authorize(Policy = ApplicationPolicies.CanSubmit_KpiScore_Policy)]
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


    public class KpiSubmissionPeriodModel // Overview info of submission for the Period
    {
        // public long Id { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public DateTimeOffset SubmissionStartDate { get; set; }
        public DateTimeOffset SubmissionEndDate { get; set; }
        public bool IsSubmitted { get; set; } = false;
        public bool IsValid { get; set; } = false;
    }
    public List<KpiSubmissionPeriodModel> KpiSubmissionPeriods { get; set; } = []; // model for rendering table

    public List<KpiPeriodViewModel> KpiPeriods { get; set; } = []; // model for KPI period table

    public DateTimeOffset SubmissionTime { get; set; } = DateTimeOffset.UtcNow;
    public bool IsSubmissionAvaiable { get; set; } = false;

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public long TotalSubmissions { get; set; } // Count

    public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalSubmissions, PageSize));
    // public bool ShowPrevious => CurrentPage > 1;
    // public bool ShowNext => CurrentPage < TotalPages;


    public async Task<IActionResult> OnGetAsync()
    {
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

        // ----------KPI PERIOD-------------------------------------------------
        var periodList = await LoadKpiPeriods();
        if (periodList == null || periodList.Count == 0)
            return Page();
        KpiPeriods = periodList.ToList();

        if (KpiPeriods.Count <= 0)
        {
            IsSubmissionAvaiable = false;
            return Page();
        }
        IsSubmissionAvaiable = true;

        // ----------CURRENT USER aka SUBMITTER----------------------------------------------
        var submitter = await GetCurrentUser();
        if (submitter == null)
            return Page();

        // TODO: or use submitter instead
        if (submitter != null)
        {
            for (int i = 0; i < KpiPeriods.Count; i++)
            {
                var departmentCount = await _departmentService.FindCountAsync();
                var submissionCount = await _kpiSubmissionService.FindCountByUserIdByKpiPeriodIdAsync(submitter.Id, KpiPeriods[i].Id);
                departmentCount -= 1; // Reduce for OWN department
                departmentCount -= 1; // Reduce for CCA department

                if (departmentCount == submissionCount)
                {
                    KpiSubmissionPeriods.Add(new KpiSubmissionPeriodModel
                    {
                        PeriodName = KpiPeriods[i].PeriodName,
                        SubmissionStartDate = KpiPeriods[i].SubmissionStartDate,
                        SubmissionEndDate = KpiPeriods[i].SubmissionEndDate,
                        IsSubmitted = true
                    });
                }
                else
                {
                    KpiSubmissionPeriods.Add(new KpiSubmissionPeriodModel
                    {
                        PeriodName = KpiPeriods[i].PeriodName,
                        SubmissionStartDate = KpiPeriods[i].SubmissionStartDate,
                        SubmissionEndDate = KpiPeriods[i].SubmissionEndDate,
                        IsSubmitted = false,
                        IsValid = DateTimeOffset.Now.UtcDateTime > KpiPeriods[i].SubmissionStartDate
                            && DateTimeOffset.Now.UtcDateTime < KpiPeriods[i].SubmissionEndDate
                    });
                }
            }
        }
        return Page();
    }

    private async Task<List<KpiPeriodViewModel>> LoadKpiPeriods()
    {
        var periods = await _kpiSubmissionPeriodService.FindAllAsync();

        return periods.Select(p => new KpiPeriodViewModel
        {
            Id = p.Id,
            PeriodName = p.PeriodName,
            SubmissionStartDate = p.SubmissionStartDate,
            SubmissionEndDate = p.SubmissionEndDate
        }).ToList();
    }


    // ========== METHODS ======================================================
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
        return kpiPeriods.ToList();
    }


    // private async Task<ApplicationUser?> GetCurrentUser()
    // {
    //     // Less likely to cause user not found, so throw just in case
    //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
    //         ?? throw new Exception("User not found. Please login again.");

    //     return await _userService.FindByIdAsync(userId);
    // }

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
}
