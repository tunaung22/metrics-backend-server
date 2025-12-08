using Metrics.Application.Common.Mappers;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Metrics.Web.Pages.Reports.Submissions.KeyKpi;

public class SummaryModel(
    IKpiSubmissionPeriodService kpiPeriodService,
    IUserService userService,
    IUserTitleService userGroupService,
    IKeyKpiSubmissionService keyKpiSubmissionService,
    IKeyKpiSubmissionConstraintService submissionConstraintService,
    IDepartmentKeyMetricService departmentKeyMetricService) : PageModel
{
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IUserService _userService = userService;
    private readonly IUserTitleService _userGroupService = userGroupService;
    private readonly IKeyKpiSubmissionService _keyKpiSubmissionService = keyKpiSubmissionService;
    private readonly IKeyKpiSubmissionConstraintService _submissionConstraintService = submissionConstraintService;
    public readonly IDepartmentKeyMetricService _departmentKeyMetricService = departmentKeyMetricService;

    public async Task<IActionResult> OnGetAsync(string periodName)
    {
        // ----------PERIOD-----------------------------------------------------
        var period = await LoadKpiPeriod(periodName);
        if (period == null)
        {
            ModelState.AddModelError("", $"Period {periodName} not found.");
            return Page();
        }
        SelectedPeriod = period;
        SelectedPeriodName = period.PeriodName;

        // ----------USER GROUPS------------------------------------------------
        UserGroupList = await LoadUserGroups();
        if (UserGroupList.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "User Group is empty");
            return Page();
        }
        UserGroupListItems = LoadUserGroupListItems(UserGroupList);


        // ----------Key Issue Departments List---------------------
        var keyIssueDepartments = await _submissionConstraintService.FindByPeriodNameAsync(SelectedPeriodName);
        if (keyIssueDepartments.IsSuccess && keyIssueDepartments.Data != null)
        {
            if (keyIssueDepartments.Data.Count > 0)
            {
                // keyIssueDepartments from Submission Constraints => will get Key Issue Departments of existing submssion
                // keyIssueDepartments from Department Key Metrics => will get all Key Issue Departments (regardless of key kpi submission)
                // KeyIssueDepartmentList = keyIssueDepartments.Data
                //     .OrderBy(k => k.DepartmentKeyMetric.KeyIssueDepartment.DepartmentName)
                //     .Select(k => k.MapToViewModel())
                //     .ToList();
            }

        }
        else
        {
            ModelState.AddModelError(string.Empty, "Department keys does not exist.");
            //fail
        }
        return Page();
    }


    // ========== Methods ==================================================
    private async Task<KpiPeriodViewModel?> LoadKpiPeriod(string periodName)
    {
        var kpiPeriod = await _kpiPeriodService
            .FindByKpiPeriodNameAsync(periodName);

        // TODO: kpi period service return result<dto>
        if (kpiPeriod != null)
            return kpiPeriod.MapToDto().MapToViewModel();
        return null;
        // return new KpiPeriodViewModel()
        // {
        //     Id = kpiPeriod.Id,
        //     PeriodName = kpiPeriod.PeriodName,
        //     SubmissionStartDate = kpiPeriod.SubmissionStartDate,
        //     SubmissionEndDate = kpiPeriod.SubmissionEndDate
        // };
    }

    private async Task<List<UserGroupViewModel>> LoadUserGroups()
    {
        var userGroups = (await _userGroupService.FindAllAsync()).OrderBy(g => g.TitleName);
        if (userGroups?.Any() != true)
            return [];

        var excludedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sysadmin",
            "staff",
            // "management"
        };

        return userGroups
            .Where(g => !excludedGroups.Contains(g.TitleName))
            .OrderBy(g => g.TitleName)
            .Select(g => g.MapToDto().MapToViewModel())
            // .Select(g => new UserGroupViewModel
            // {
            //     Id = g.Id,
            //     GroupCode = g.TitleCode,
            //     GroupName = g.TitleName,
            //     Description = g.Description
            // }).ToList();
            .ToList();
    }

    private List<SelectListItem> LoadUserGroupListItems(List<UserGroupViewModel> userGroups)
    {
        if (userGroups.Count > 0)
        {
            // add All item before user group items
            var items = new List<SelectListItem>()
            {
                new() { Value = "all", Text = "All" }
            };

            foreach (var group in userGroups)
            {
                items.Add(new()
                {
                    Value = group.GroupName.ToLower(),
                    Text = group.GroupName,
                });
            }

            return items;
        }

        ModelState.AddModelError("", "User group does not exist. Try to add group and continue.");
        return [];
    }

    // ========== Binding ======================================================
    [BindProperty]
    public List<SelectListItem> UserGroupListItems { get; set; } = []; // for select element

    // ========== Models =======================================================
    public KpiPeriodViewModel SelectedPeriod { get; set; } = new();
    public string SelectedPeriodName { get; set; } = null!;
    public List<UserGroupViewModel> UserGroupList { get; set; } = [];
    public List<DepartmentViewModel> KeyIssueDepartmentList { get; set; } = [];


}
