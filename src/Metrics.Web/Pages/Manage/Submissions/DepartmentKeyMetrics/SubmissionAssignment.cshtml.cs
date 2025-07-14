using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Metrics.Web.Pages.Manage.Submissions.DepartmentKeyMetrics;

public class SubmissionAssignmentModel : PageModel
{
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionPeriodService _kpiSubmissionPeriodService; // Period
    private readonly IKeyMetricService _keyMetricService; // Key Metric
    private readonly IDepartmentKeyMetricService _departmentKeyMetricService;
    private readonly IKeyKpiSubmissionConstraintService _keyKpiSubmissionConstraintService;

    public SubmissionAssignmentModel(
        IDepartmentService departmentService,
        IKpiSubmissionPeriodService kpiSubmissionPeriodService,
        IKeyMetricService keyMetricService,
        IDepartmentKeyMetricService departmentKeyMetricService,
        IKeyKpiSubmissionConstraintService keyKpiSubmissionConstraintService)
    {
        _departmentService = departmentService;
        _kpiSubmissionPeriodService = kpiSubmissionPeriodService;
        _keyMetricService = keyMetricService;
        _departmentKeyMetricService = departmentKeyMetricService;
        _keyKpiSubmissionConstraintService = keyKpiSubmissionConstraintService;
    }

    // =============== MODELS ==================================================
    public class KeyKpiSubmissionConstraintViewModel
    {
        public long Id { get; set; }
        public Guid LookupId { get; set; }
        public bool IsDeleted { get; set; }
        public long DepartmentId { get; set; }
        public DepartmentViewModel Department { get; set; } = null!;
        public long DepartmentKeyMetricId { get; set; }
        public DepartmentKeyMetricViewModel DepartmentKeyMetric { get; set; } = null!;
    }
    public List<KeyKpiSubmissionConstraintViewModel> UpdateConstraintViewModel { get; set; } = [];
    public List<KeyKpiSubmissionConstraintViewModel> KeyKpiSubmissionConstraints { get; set; } = [];

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

    public class KeyMetricViewModel
    {
        public long Id { get; set; }
        public Guid MetricCode { get; set; }
        public string MetricTitle { get; set; } = null!;
        public string? Description { get; set; }
    }

    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];

    [BindProperty]
    public string? CurrentPeriodName { get; set; }

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

        // Department Key Metrics
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

        // KeyKpiSubmissionConstraint 
        // (Department Key Metric Submission Constraint)
        if (CurrentDepartmentCode != null)
        {
            var keyKpiSubmissionConstraint = await _keyKpiSubmissionConstraintService
                .FindAllByDepartmentAsync(Guid.Parse(CurrentDepartmentCode));
            if (keyKpiSubmissionConstraint.Any())
            {
                KeyKpiSubmissionConstraints = keyKpiSubmissionConstraint
                    .Select(k => new KeyKpiSubmissionConstraintViewModel
                    {
                        Id = k.Id,
                        LookupId = k.LookupId,
                        // Department = k.Department,
                        Department = new DepartmentViewModel
                        {
                            Id = k.Department.Id,
                            DepartmentCode = k.Department.DepartmentCode,
                            DepartmentName = k.Department.DepartmentName
                        },

                        DepartmentId = k.DepartmentId,
                        DepartmentKeyMetric = new DepartmentKeyMetricViewModel
                        {
                            Id = k.DepartmentKeyMetric.Id,
                            DepartmentKeyMetricCode = k.DepartmentKeyMetric.DepartmentKeyMetricCode,
                            IsDeleted = k.DepartmentKeyMetric.IsDeleted,
                            KpiSubmissionPeriodId = k.DepartmentKeyMetric.KpiSubmissionPeriodId,
                            DepartmentId = k.DepartmentKeyMetric.DepartmentId,
                            // Department = new DepartmentViewModel
                            // {
                            //     Id = k.TargetDepartment.Id,
                            //     DepartmentCode = k.TargetDepartment.DepartmentCode,
                            //     DepartmentName = k.TargetDepartment.DepartmentName
                            // },
                            //  DepartmentViewModel Department { get; set; } = null!;
                            KeyMetricId = k.DepartmentKeyMetric.KeyMetricId
                            //  KeyMetricViewModel KeyMetric { get; set; } = null!;
                        },
                        DepartmentKeyMetricId = k.DepartmentKeyMetricId,
                        IsDeleted = k.IsDeleted
                    }).ToList();
            }
        }

        return Page();
    }


    public async Task<IActionResult> OnPostUpdateAsync(
        [FromRoute] string periodName, // use to fetch "department key metric" by period
        [FromQuery] string department, // submitter department
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
            // $"{(index + 1).ToString("00")}" 
            Value = d.DepartmentCode.ToString(),
            Text = d.DepartmentName
        }).ToList();

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

        // Department Key Metrics (for the selected period)
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

        // KeyKpiSubmissionConstraint 
        // (Department Key Metric Submission Constraint)
        if (CurrentDepartmentCode != null)
        {
            var keyKpiSubmissionConstraint = await _keyKpiSubmissionConstraintService
                .FindAllByDepartmentAsync(
                    Guid.Parse(CurrentDepartmentCode));
            if (keyKpiSubmissionConstraint.Any())
            {
                KeyKpiSubmissionConstraints = keyKpiSubmissionConstraint
                    .Select(k => new KeyKpiSubmissionConstraintViewModel
                    {
                        Id = k.Id,
                        LookupId = k.LookupId,
                        // Department = k.Department,
                        Department = new DepartmentViewModel
                        {
                            Id = k.Department.Id,
                            DepartmentCode = k.Department.DepartmentCode,
                            DepartmentName = k.Department.DepartmentName
                        },

                        DepartmentId = k.DepartmentId,
                        DepartmentKeyMetric = new DepartmentKeyMetricViewModel
                        {
                            Id = k.DepartmentKeyMetric.Id,
                            DepartmentKeyMetricCode = k.DepartmentKeyMetric.DepartmentKeyMetricCode,
                            IsDeleted = k.DepartmentKeyMetric.IsDeleted,
                            KpiSubmissionPeriodId = k.DepartmentKeyMetric.KpiSubmissionPeriodId,
                            DepartmentId = k.DepartmentKeyMetric.DepartmentId,
                            // Department = new DepartmentViewModel
                            // {
                            //     Id = k.TargetDepartment.Id,
                            //     DepartmentCode = k.TargetDepartment.DepartmentCode,
                            //     DepartmentName = k.TargetDepartment.DepartmentName
                            // },
                            //  DepartmentViewModel Department { get; set; } = null!;
                            KeyMetricId = k.DepartmentKeyMetric.KeyMetricId
                            //  KeyMetricViewModel KeyMetric { get; set; } = null!;
                        },
                        DepartmentKeyMetricId = k.DepartmentKeyMetricId,
                        IsDeleted = k.IsDeleted
                    }).ToList();
            }


            // using var transaction = await _context.Database.BeginTransactionAsync();
            // ===== ASSIGNMENT Operation ==========================================
            try
            {
                if (requestBody.DepartmentCode == Guid.Parse(CurrentDepartmentCode))
                {
                    // 1. Unassign  all keys which   does not    exist in requestBody
                    // 2. Assign    all keys which   does        exist in requestBody
                    // 3. Create new keys
                    var inputDKMs = requestBody.DepartmentKeyMetrics;

                    // ========== UNASSIGN =====================================
                    // 1. Unassign  all keys which   does not    exist in requestBody
                    // *get not selected items to get "to delete items" later
                    // To get not Selected Items:
                    // DepartmentKeyMetrics - User selected DepartmentKeyMetrics
                    var notSelectedDKMs = DepartmentKeyMetrics
                        .Where(dkm => !inputDKMs
                            .Select(i => i.DepartmentKeyMetricCode)
                            .Contains(dkm.DepartmentKeyMetricCode))
                        .ToList();
                    var toDeleteItems = KeyKpiSubmissionConstraints
                        .Where(c => notSelectedDKMs
                            .Select(dkms => dkms.DepartmentKeyMetricCode)
                            .Contains(c.DepartmentKeyMetric.DepartmentKeyMetricCode)
                            && c.IsDeleted == false)
                        // .Select(dkms => dkms.Id)
                        // .Contains(c.DepartmentKeyMetricId))
                        .ToList();
                    foreach (var c in toDeleteItems)
                    {
                        await _keyKpiSubmissionConstraintService
                            .DeleteAsync(c.LookupId);
                    }

                    // ========== ASSIGN =======================================
                    var toAssignItems = DepartmentKeyMetrics
                       .Where(k => inputDKMs
                            .Select(i => i.DepartmentKeyMetricCode)
                            .Contains(k.DepartmentKeyMetricCode))
                       .ToList();

                    // 2. Assign    all keys which   does        exist in requestBody
                    // **** need review this block
                    // **** this code run even already exist
                    //      and update and set isDelete to false (aka. unDelete)
                    var toReassignItems = KeyKpiSubmissionConstraints
                        .Where(c => toAssignItems
                            .Select(i => i.DepartmentKeyMetricCode)
                            .Contains(c.DepartmentKeyMetric.DepartmentKeyMetricCode)
                            && c.IsDeleted == true)
                        .ToList();
                    foreach (var c in toReassignItems)
                    {
                        // Undelete each items
                        await _keyKpiSubmissionConstraintService.UnDeleteAsync(c.LookupId);
                    }


                    // 3. Create new keys
                    var newItems = toAssignItems
                        .Where(i => !KeyKpiSubmissionConstraints
                            .Select(c => c.DepartmentKeyMetric.DepartmentKeyMetricCode)
                            .Contains(i.DepartmentKeyMetricCode))
                        .ToList();
                    if (newItems.Count > 0)
                    {
                        var submitterDepartment = await _departmentService
                            .FindByDepartmentCodeAsync(department);
                        foreach (var c in newItems)
                        {
                            var entity = new KeyKpiSubmissionConstraint
                            {
                                DepartmentId = submitterDepartment.Id,
                                DepartmentKeyMetricId = c.Id
                            };
                            await _keyKpiSubmissionConstraintService.CreateAsync(entity);
                        }
                    }

                    StatusSuccess = true;
                    StatusMessage = "Updated successfully.";
                }
            }
            catch (Exception)
            {
                // 1. Log
                // 2. set message
                StatusSuccess = false;
                StatusMessage = "Save failed!";
            }
        }

        return Page();
    }

    [BindProperty]
    public InputValueViewModel InputValues { get; set; } = null!;

    public class InputValueViewModel
    {
        public Guid DepartmentCode { get; set; }
        public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];
    }



    // [TempData]
    // public MessageViewModel? TempMessage { get; set; }
    [TempData]
    public string? StatusMessage { get; set; }
    [TempData]
    public bool StatusSuccess { get; set; }
}


