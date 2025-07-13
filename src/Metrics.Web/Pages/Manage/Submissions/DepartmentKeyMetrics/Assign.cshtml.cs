using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Manage.Submissions.DepartmentKeyMetrics;

public class AssignModel : PageModel
{
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionPeriodService _kpiSubmissionPeriodService; // Period
    private readonly IKeyMetricService _keyMetricService; // Key Metric
    private readonly IDepartmentKeyMetricService _departmentKeyMetricService;

    public AssignModel(
        IDepartmentService departmentService,
        IKpiSubmissionPeriodService kpiSubmissionPeriodService,
        IKeyMetricService keyMetricService,
        IDepartmentKeyMetricService departmentKeyMetricService)
    {
        _departmentService = departmentService;
        _kpiSubmissionPeriodService = kpiSubmissionPeriodService;
        _keyMetricService = keyMetricService;
        _departmentKeyMetricService = departmentKeyMetricService;
    }

    // =============== MODELS ==================================================
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
        public DepartmentViewModel Department { get; set; } = null!;
        public long KeyMetricId { get; set; }
        public KeyMetricViewModel KeyMetric { get; set; } = null!;
    }

    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];

    [BindProperty]
    public string? CurrentPeriodName { get; set; }

    // public List<DepartmentViewModel> Departments { get; set; }
    // public string? SelectedDepartmentCode { get; set; }
    [BindProperty]
    public List<SelectListItem> DepartmentListItems { get; set; } = [];
    [BindProperty(Name = "Department", SupportsGet = true)]
    public string? CurrentDepartmentCode { get; set; } // QueryString => Current Department Code

    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync([FromRoute] string periodName)
    {
        // Selected Period
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
            return Page();
        }
        CurrentPeriodName = periodName;

        // Department List Items
        var departments = await _departmentService.FindAllAsync();
        if (!departments.Any())
        {
            ModelState.AddModelError(string.Empty, "No Department exist.");
            return Page();
        }
        DepartmentListItems = departments.Select((d, index) => new SelectListItem
        {
            // $"{(index + 1).ToString("00")}" 
            Value = d.DepartmentCode.ToString(),
            Text = d.DepartmentName
        }).ToList();
        // Departments = departments.Select(d => new DepartmentViewModel
        // {
        //     Id = d.Id,
        //     DepartmentCode = d.DepartmentCode,
        //     DepartmentName = d.DepartmentName
        // }).ToList();

        // Selected Department Code
        // if (string.IsNullOrEmpty(SelectedDepartmentCode))
        // {
        //     var code = TempData["SelectedDepartmentCode"]?.ToString();
        //     if (!string.IsNullOrEmpty(code))
        //         SelectedDepartmentCode = code;
        //     else
        //         SelectedDepartmentCode = departments.First().DepartmentCode.ToString();
        // }

        // Current Department Code and Name
        if (string.IsNullOrEmpty(CurrentDepartmentCode))
        {
            var code = TempData["CurrentDepartmentCode"]?.ToString();
            if (!string.IsNullOrEmpty(code))
                CurrentDepartmentCode = code;
            else
            {
                CurrentDepartmentCode = departments.First().DepartmentCode.ToString();
            }
        }

        // Key Metrics
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
                        Department = new DepartmentViewModel
                        {
                            Id = k.TargetDepartment.Id,
                            DepartmentCode = k.TargetDepartment.DepartmentCode,
                            DepartmentName = k.TargetDepartment.DepartmentName
                        },
                        KeyMetricId = k.KeyMetricId,
                        KeyMetric = new KeyMetricViewModel
                        {
                            Id = k.KeyMetric.Id,
                            MetricCode = k.KeyMetric.MetricCode,
                            MetricTitle = k.KeyMetric.MetricTitle
                        },
                        IsDeleted = k.IsDeleted
                    }).ToList();
            }
        }

        return Page();

    }



    // public async Task<IActionResult> OnPostToggleKeyMetricAsync(
    //     [FromRoute] string periodName,
    //     [FromQuery] string keymetric,
    //     [FromQuery] string department)
    // {
    // }

    public async Task<IActionResult> OnPostUpdateAsync(
        [FromRoute] string periodName,
    // [FromQuery] string keymetric,
        [FromQuery] string department,
        [FromBody] InputValueViewModel requestBody)
    {
        // Selected Period
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
            return Page();
        }
        CurrentPeriodName = periodName;


        // Department List Items
        var departments = await _departmentService.FindAllAsync();
        if (!departments.Any())
        {
            ModelState.AddModelError(string.Empty, "No Department exist.");
            return Page();
        }
        DepartmentListItems = departments.Select((d, index) => new SelectListItem
        {
            Value = d.DepartmentCode.ToString(),
            Text = $"{(index + 1).ToString("00")}. {d.DepartmentName}"
        }).ToList();
        // Departments = departments.Select(d => new DepartmentViewModel
        // {
        //     Id = d.Id,
        //     DepartmentCode = d.DepartmentCode,
        //     DepartmentName = d.DepartmentName
        // }).ToList();

        // Selected Department Code
        // if (string.IsNullOrEmpty(SelectedDepartmentCode))
        // {
        //     var code = TempData["SelectedDepartmentCode"]?.ToString();
        //     if (!string.IsNullOrEmpty(code))
        //         SelectedDepartmentCode = code;
        //     else
        //         SelectedDepartmentCode = departments.First().DepartmentCode.ToString();
        // }

        // Current Department Code and Name
        if (string.IsNullOrEmpty(CurrentDepartmentCode))
        {
            var code = TempData["CurrentDepartmentCode"]?.ToString();
            if (!string.IsNullOrEmpty(code))
                CurrentDepartmentCode = code;
            else
            {
                CurrentDepartmentCode = departments.First().DepartmentCode.ToString();
            }
        }

        // Key Metrics
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
                        Department = new DepartmentViewModel
                        {
                            Id = k.TargetDepartment.Id,
                            DepartmentCode = k.TargetDepartment.DepartmentCode,
                            DepartmentName = k.TargetDepartment.DepartmentName
                        },
                        KeyMetricId = k.KeyMetricId,
                        KeyMetric = new KeyMetricViewModel
                        {
                            Id = k.KeyMetric.Id,
                            MetricCode = k.KeyMetric.MetricCode,
                            MetricTitle = k.KeyMetric.MetricTitle
                        },
                        IsDeleted = k.IsDeleted
                    }).ToList();
            }

            // using var transaction = await _context.Database.BeginTransactionAsync();
            // ===== ASSIGNMENT Operation ==========================================
            try
            {
                if (requestBody.DepartmentCode == Guid.Parse(CurrentDepartmentCode))
                {
                    // update (department key metrics)
                    // 1. unassign all keys which   does not  exist in requestBody
                    // 2. assign all keys which     does      exist in requestBody
                    // compare keys from request and Assign
                    // if keys status is not isDeleted && exists:
                    //      - set isDeleted to TRUE

                    var inputKeys = requestBody.KeyMetrics;

                    // ========== UNASSIGN =====================================
                    // 1. unassign all keys which   does not  exist in requestBody
                    // -----soft delete Department Key Metrics
                    var keysToUnassign = KeyMetrics
                        .Where(k => !inputKeys
                            .Select(i => i.MetricCode)
                            .Contains(k.MetricCode))
                        .ToList();

                    // fetch DepartmentKeyMetrics where keymetrics == keysToUnassign
                    var toDeleteItems = DepartmentKeyMetrics
                        .Where(dkm => keysToUnassign
                            .Select(k => k.MetricCode)
                            .Contains(dkm.KeyMetric.MetricCode)
                            && dkm.IsDeleted == false)
                        .ToList();
                    foreach (var k in toDeleteItems)
                    {
                        // soft delete the items
                        await _departmentKeyMetricService
                            .DeleteAsync(k.DepartmentKeyMetricCode);
                    }

                    // ========== ASSIGN =======================================
                    // 2. assign all keys which     does      exist in requestBody
                    // **** need review this block
                    // **** this code run even already exist
                    //      and update and set isDelete to false (aka. unDelete)
                    var keysToAssign = KeyMetrics
                        .Where(k => inputKeys
                            .Select(i => i.MetricCode)
                            .Contains(k.MetricCode))
                        .ToList();
                    // fetch DepartmentKeyMetrics where keymetrics == keysToAssign

                    // keysToAssign -> reassign deleted items
                    // Old items
                    var toReassignItems = DepartmentKeyMetrics
                        .Where(dkm => keysToAssign
                            .Select(k => k.MetricCode)
                            .Contains(dkm.KeyMetric.MetricCode)
                            && dkm.IsDeleted == true)
                        .ToList();
                    foreach (var k in toReassignItems)
                    {
                        // soft delete the items
                        await _departmentKeyMetricService.UnDeleteAsync(k.DepartmentKeyMetricCode);
                    }

                    // keysToAssign-> assign new item
                    // New items
                    var newItems = keysToAssign
                        .Where(k => !DepartmentKeyMetrics
                            .Select(dkm => dkm.KeyMetric.MetricCode)
                            .Contains(k.MetricCode))
                        .ToList();
                    if (newItems.Count > 0)
                    {
                        // ---------- KPI Period -----------------------------------------------
                        if (string.IsNullOrEmpty(periodName))
                        {
                            ModelState.AddModelError(string.Empty, "Submission Period is required!");
                            return Page();
                        }
                        var kpiPeriod = await _kpiSubmissionPeriodService
                            .FindByKpiPeriodNameAsync(periodName);
                        if (kpiPeriod == null)
                        {
                            ModelState.AddModelError(string.Empty, $"Invalid submission period: {periodName}.");
                            return Page();
                        }

                        var departmentEntity = await _departmentService
                            .FindByDepartmentCodeAsync(department);
                        foreach (var k in newItems)
                        {
                            var entity = new DepartmentKeyMetric
                            {
                                KpiSubmissionPeriodId = kpiPeriod.Id,
                                DepartmentId = departmentEntity.Id,
                                KeyMetricId = k.Id,
                            };
                            await _departmentKeyMetricService.CreateAsync(entity);
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        return Page();
    }
    /* {
        departmentCode: "123-456-789",
        departmentName: "Admin Department",
        keyMetrics: [
            {
                metricCode: "abc123",
                metricTitle: "service must be as fast as possible"
            }
        ]
    } */

    [BindProperty]
    public InputValueViewModel InputValues { get; set; } = null!;

    public class InputValueViewModel
    {
        public Guid DepartmentCode { get; set; }
        public List<KeyMetricModel> KeyMetrics { get; set; } = [];
    }
    public class KeyMetricModel
    {
        public Guid MetricCode { get; set; }
        public string MetricTitle { get; set; } = null!;
    }
}
