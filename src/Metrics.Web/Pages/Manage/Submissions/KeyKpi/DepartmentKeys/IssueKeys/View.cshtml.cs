using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models.DepartmentKeyMetric;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace Metrics.Web.Pages.Manage.Submissions.KeyKpi.DepartmentKeys.IssueKeys;

public class ViewModel(
    IDepartmentService departmentService,
    IKpiSubmissionPeriodService kpiPeriodService,
    IKeyMetricService keyMetricService,
    IDepartmentKeyMetricService dkmService) : PageModel
{
    private readonly IDepartmentService _departmentService = departmentService;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService = kpiPeriodService;
    private readonly IKeyMetricService _keyMetricService = keyMetricService;
    private readonly IDepartmentKeyMetricService _dkmService = dkmService;


    // =============== MODELS ==================================================
    public string SelectedPeriodName { get; set; } = null!;

    public List<DepartmentKeyMetricViewModel> SelectedDKMs { get; set; } = [];


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync(string periodName)
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
            return Page();
        }
        SelectedPeriodName = @Uri.UnescapeDataString(periodName);

        SelectedDKMs = await LoadDepartmentKeysByPeriod(SelectedPeriodName);

        return Page();
    }

    private async Task<List<DepartmentKeyMetricViewModel>> LoadDepartmentKeysByPeriod(string sourcePeriodName)
    {
        var dkms = await _dkmService.FindByPeriodNameAsync(Uri.UnescapeDataString(sourcePeriodName));
        if (!dkms.IsSuccess || dkms.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to fetch department keys by source period.");
            return [];
        }

        return dkms.Data
            .Where(dkm => dkm.IsDeleted == false)
            .OrderBy(dkm => dkm.KeyIssueDepartment.DepartmentName)
                .ThenBy(dkm => dkm.KeyMetric.MetricTitle)
            .Select(dkm => dkm.MapToViewModel())
            .ToList();
    }
}








/*

private readonly IDepartmentService _departmentService = departmentService;
private readonly IKpiSubmissionPeriodService _kpiSubmissionPeriodService = kpiSubmissionPeriodService; // Period
private readonly IKeyMetricService _keyMetricService = keyMetricService; // Key Metric
private readonly IDepartmentKeyMetricService _departmentKeyMetricService = departmentKeyMetricService;

public class KeyMetricViewModel
{
    public long Id { get; set; }
    public Guid MetricCode { get; set; }
    public string MetricTitle { get; set; } = null!;
    public string? Description { get; set; }
}

public List<KeyMetricViewModel> KeyMetrics { get; set; } = [];

public class DepartmentKeyMetricViewModel
{
    public long Id { get; set; }
    public Guid DepartmentKeyMetricCode { get; set; }
    public bool IsDeleted { get; set; }
    public long KpiSubmissionPeriodId { get; set; }
    public long DepartmentId { get; set; }
    public long KeyMetricId { get; set; }
}

public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];

[BindProperty]
public List<SelectListItem> DepartmentListItems { get; set; } = [];

// [BindProperty]
// public List<SelectListItem> KeyMetricListItems { get; set; } = [];

[BindProperty]
public string? CurrentPeriodName { get; set; }

[BindProperty(Name = "Department", SupportsGet = true)]
public string? CurrentDepartmentCode { get; set; } // QueryString => Current Department Code

// public string? Department { get; set; } = string.Empty;


public async Task<IActionResult> OnGetAsync(
    [FromRoute] string periodName)
// [FromQuery] string? department)
{


    if (string.IsNullOrEmpty(periodName))
    {
        ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
        return Page();
    }
    CurrentPeriodName = periodName;


    // Departments
    var departments = await _departmentService.FindAllAsync();
    if (!departments.Any())
    {
        ModelState.AddModelError(string.Empty, "No Department exist.");
        return Page();
    }
    if (string.IsNullOrEmpty(CurrentDepartmentCode))
    {
        var code = TempData["CurrentDepartmentCode"]?.ToString();
        if (!string.IsNullOrEmpty(code))
            CurrentDepartmentCode = code;
        else
            CurrentDepartmentCode = departments.First().DepartmentCode.ToString();
    }

    DepartmentListItems = departments.Select((d, index) => new SelectListItem
    {
        Value = d.DepartmentCode.ToString(),
        Text = $"{(index + 1).ToString("00")}. {d.DepartmentName}"
    }).ToList();

    // Key Metrics
    var keyMetrics = await _keyMetricService.FindAllAsync();
    if (!keyMetrics.Any())
    {
        ModelState.AddModelError(string.Empty, "No Key Metrics. Add Key Metric and continue.");
        return Page();
    }
    // KeyMetricListItems = keyMetrics.Select(k => new SelectListItem
    // {
    //     Value = k.Id.ToString(),
    //     Text = k.MetricTitle
    // }).ToList();
    KeyMetrics = keyMetrics.Select(k => new KeyMetricViewModel
    {
        Id = k.Id,
        MetricCode = k.MetricCode,
        MetricTitle = k.MetricTitle,
        Description = k.Description
    }).ToList();

    // Department Key Metrics
    if (CurrentDepartmentCode != null)
    {
        var departmentKeyMetrics = await _departmentKeyMetricService
            // .FindAllByPeriodNameAsync(CurrentPeriodName);
            .FindAllByPeriodAndDepartmentAsync(
                CurrentPeriodName,
                Guid.Parse(CurrentDepartmentCode));
        if (departmentKeyMetrics.Any())
        {
            DepartmentKeyMetrics = departmentKeyMetrics
                .Select(k => new DepartmentKeyMetricViewModel
                {
                    Id = k.Id,
                    DepartmentKeyMetricCode = k.DepartmentKeyMetricCode,
                    KpiSubmissionPeriodId = k.KpiSubmissionPeriodId,
                    DepartmentId = k.DepartmentId,
                    KeyMetricId = k.KeyMetricId,
                    IsDeleted = k.IsDeleted
                }).ToList();
        }
    }

    return Page();
}

public async Task<IActionResult> OnPostToggleKeyMetricAsync(
    [FromRoute] string periodName,
    [FromQuery] string keymetric,
    [FromQuery] string department)
{
    if (string.IsNullOrEmpty(periodName))
    {
        ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
        return Page();
    }
    CurrentPeriodName = periodName;

    // ----------Key Metrics------------------------------------------------
    var keyMetrics = await _keyMetricService.FindAllAsync();
    if (!keyMetrics.Any())
    {
        ModelState.AddModelError(string.Empty, "No Key Metrics. Add Key Metric and continue.");
        return Page();
    }
    KeyMetrics = keyMetrics.Select(k => new KeyMetricViewModel
    {
        Id = k.Id,
        MetricCode = k.MetricCode,
        MetricTitle = k.MetricTitle,
        Description = k.Description
    }).ToList();
    // ----------Department Key Metrics-------------------------------------
    var departmentKeyMetrics = await _departmentKeyMetricService
        .FindAllByPeriodNameAsync(CurrentPeriodName);
    if (departmentKeyMetrics.Any())
    {
        DepartmentKeyMetrics = departmentKeyMetrics
            .Select(k => new DepartmentKeyMetricViewModel
            {
                Id = k.Id,
                DepartmentKeyMetricCode = k.DepartmentKeyMetricCode,
                KpiSubmissionPeriodId = k.KpiSubmissionPeriodId,
                DepartmentId = k.DepartmentId,
                KeyMetricId = k.KeyMetricId,
                IsDeleted = k.IsDeleted
            }).ToList();
    }
    try
    {
        // 1. get 
        // ...find DepartmentKeyMetric by key, period, department
        // await _departmentKeyMetricService.FindByPeriodNameAndDepartmentCodeAndKeyMetricCodeAsync();
        var existingItem = await _departmentKeyMetricService.FindByPeriodAndDepartmentAndKeyMetricAsync(
            periodName, Guid.Parse(department), Guid.Parse(keymetric)
        );

        if (existingItem == null)
        {
            // CREATE
            var periodEntity = await _kpiSubmissionPeriodService.FindByKpiPeriodNameAsync(periodName);
            var departmentEntity = await _departmentService.FindByDepartmentCodeAsync(department);
            var keyMetricEntity = await _keyMetricService.FindByCodeAsync(Guid.Parse(keymetric));

            if (periodEntity != null && departmentEntity != null && keyMetricEntity != null)
            {
                var entity = new DepartmentKeyMetric
                {
                    KpiSubmissionPeriodId = periodEntity.Id,
                    DepartmentId = departmentEntity.Id,
                    KeyMetricId = keyMetricEntity.Id,
                };
                await _departmentKeyMetricService.CreateAsync(entity);

                // Save the current Department
                TempData["CurrentDepartmentCode"] = department;

                return RedirectToPage();
            }
        }
        else
        {
            // UPDATE / TOGGLE STATE
            // CHECK DELETED?
            if (existingItem.IsDeleted == true)
            {
                // SET IsDeleted to false
                await _departmentKeyMetricService.UnDeleteAsync(existingItem.DepartmentKeyMetricCode);
            }
            else
            {
                // SET IsDeleted to true
                await _departmentKeyMetricService.DeleteAsync(existingItem.DepartmentKeyMetricCode);
            }
        }
        // 2. insert or update or soft delete
    }
    // catch ()
    // {
    // }
    catch (System.Exception)
    {
        ModelState.AddModelError("", "Unexpected Error occured.");
    }
    // return RedirectToPage("?CurrentDepartmentCode=CurrentDepartmentCode")
    // return RedirectToPage(new { department = CurrentDepartmentCode });
    return RedirectToPage();
}
*/
