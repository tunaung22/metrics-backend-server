using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Metrics.Web.Models.DepartmentKeyMetric;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Manage.Submissions.DepartmentKeyMetrics;

public class SubmissionAssignmentModel(
    ILogger<SubmissionAssignmentModel> logger,
    IDepartmentService departmentService,
    IKpiSubmissionPeriodService kpiSubmissionPeriodService,
    IKeyMetricService keyMetricService,
    IDepartmentKeyMetricService departmentKeyMetricService,
    IKeyKpiSubmissionConstraintService keyKpiSubmissionConstraintService) : PageModel
{
    private readonly ILogger<SubmissionAssignmentModel> _logger = logger;
    private readonly IDepartmentService _departmentService = departmentService;
    private readonly IKpiSubmissionPeriodService _kpiSubmissionPeriodService = kpiSubmissionPeriodService; // Period
    private readonly IKeyMetricService _keyMetricService = keyMetricService; // Key Metric
    private readonly IDepartmentKeyMetricService _departmentKeyMetricService = departmentKeyMetricService;
    private readonly IKeyKpiSubmissionConstraintService _keyKpiSubmissionConstraintService = keyKpiSubmissionConstraintService;


    // =============== MODELS ==================================================
    public List<KeyKpiSubmissionConstraintViewModel> UpdateConstraintViewModel { get; set; } = [];
    public List<KeyKpiSubmissionConstraintViewModel> KeyKpiSubmissionConstraints { get; set; } = [];

    // public class DepartmentKeyMetricViewModel
    // {
    //     public long Id { get; set; }
    //     public Guid DepartmentKeyMetricCode { get; set; }
    //     public bool IsDeleted { get; set; }
    //     public long KpiSubmissionPeriodId { get; set; }
    //     public long DepartmentId { get; set; }
    //     public DepartmentViewModel Department { get; set; } = null!;
    //     public long KeyMetricId { get; set; }
    //     public KeyMetricViewModel KeyMetric { get; set; } = null!;
    // }

    // public class KeyMetricViewModel
    // {
    //     public long Id { get; set; }
    //     public Guid MetricCode { get; set; }
    //     public string MetricTitle { get; set; } = null!;
    //     public string? Description { get; set; }
    // }

    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];

    [BindProperty]
    public string? SelectedPeriodName { get; set; }

    public KpiPeriodViewModel SelectedPeriod { get; set; } = null!;

    [BindProperty]
    public List<SelectListItem> DepartmentListItems { get; set; } = [];

    public List<DepartmentViewModel> Departments { get; set; } = [];

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

        // ----------KPI PERIOD-------------------------------------------------
        var selectedPeriod = await LoadKpiPeriod(periodName);
        if (selectedPeriod == null)
            return Page();

        SelectedPeriod = selectedPeriod;
        SelectedPeriodName = selectedPeriod.PeriodName;

        // ----------DEPARTMENTS------------------------------------------------
        Departments = await LoadDepartmentList();
        if (Departments.Count == 0)
            return Page();
        // Department List Items
        DepartmentListItems = Departments
            .Select((d, index) => new SelectListItem
            {
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
                CurrentDepartmentCode = Departments.First().DepartmentCode.ToString();
        }

        // Department Key Metrics
        var departmentKeyMetrics = await _departmentKeyMetricService
            .FindByPeriodIdAsync(SelectedPeriod.Id);

        if (departmentKeyMetrics.IsSuccess)
        {
            if (departmentKeyMetrics.Data != null)
            {
                DepartmentKeyMetrics = departmentKeyMetrics.Data
                    .Select(dkm => dkm.MapToViewModel())
                    .ToList();
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Failed to fetch department key metrics.");
        }


        // KeyKpiSubmissionConstraint 
        // (Department Key Metric Submission Constraint)
        if (CurrentDepartmentCode != null)
        {
            var keyKpiSubmissionConstraint = await _keyKpiSubmissionConstraintService
                .FindByPeriodBySubmitterDepartmentAsync(
                    SelectedPeriod.Id,
                    Guid.Parse(CurrentDepartmentCode));
            if (keyKpiSubmissionConstraint.IsSuccess)
            {
                if (keyKpiSubmissionConstraint.Data != null)
                {
                    KeyKpiSubmissionConstraints = keyKpiSubmissionConstraint.Data
                        .Select(k => k.MapToViewModel()).ToList();
                    // new KeyKpiSubmissionConstraintViewModel
                    // {
                    //     Id = k.Id,
                    //     LookupId = k.LookupId,
                    //     // Department = k.Department,
                    //     Department = k.DepartmentKeyMetric.KeyIssueDepartment.MapToViewModel(),
                    //     DepartmentId = k.DepartmentKeyMetric.KeyIssuDepartmentId,
                    //     DepartmentKeyMetric = k.DepartmentKeyMetric.MapToViewModel(),
                    //     DepartmentKeyMetricId = k.DepartmentKeyMetricId,
                    //     IsDeleted = k.IsDeleted
                    // }
                }
            }
        }
        return Page();
    }


    public async Task<IActionResult> OnPostUpdateAsync(
        [FromRoute] string periodName, // use to fetch "department key metric" by period
        [FromQuery] string department, // submitter department
        [FromBody] UserSelectionViewModel requestBody)
    {
        // Selected Period
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
            return Page();
        }
        // ----------KPI PERIOD-------------------------------------------------
        var selectedPeriod = await LoadKpiPeriod(periodName);
        if (selectedPeriod == null)
            return Page();

        SelectedPeriod = selectedPeriod;
        SelectedPeriodName = selectedPeriod.PeriodName;

        // ----------DEPARTMENTS------------------------------------------------
        Departments = await LoadDepartmentList();
        if (Departments.Count == 0)
            return Page();
        // Department List Items
        DepartmentListItems = Departments
            .Select((d, index) => new SelectListItem
            {
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
                CurrentDepartmentCode = Departments.First().DepartmentCode.ToString();
        }

        // Department Key Metrics
        var departmentKeyMetrics = await _departmentKeyMetricService
            .FindByPeriodIdAsync(SelectedPeriod.Id);

        if (departmentKeyMetrics.IsSuccess)
        {
            if (departmentKeyMetrics.Data != null)
            {
                DepartmentKeyMetrics = departmentKeyMetrics.Data
                    .Select(dkm => dkm.MapToViewModel())
                    .ToList();
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Failed to fetch department key metrics.");
        }

        // KeyKpiSubmissionConstraint 
        // (Department Key Metric Submission Constraint)
        if (CurrentDepartmentCode != null)
        {
            var keyKpiSubmissionConstraint = await _keyKpiSubmissionConstraintService
                .FindBySubmitterDepartmentAsync(
                    Guid.Parse(CurrentDepartmentCode));
            if (keyKpiSubmissionConstraint.IsSuccess)
            {
                if (keyKpiSubmissionConstraint.Data != null)
                {
                    KeyKpiSubmissionConstraints = keyKpiSubmissionConstraint.Data
                        .Select(k => k.MapToViewModel()).ToList();
                }
            }
            // ===== ASSIGNMENT Operation ==========================================
            try
            {
                if (requestBody.SubmitterDepartmentCode == Guid.Parse(CurrentDepartmentCode))
                {
                    // 1. Unassign  all keys which   does not    exist in requestBody
                    // 2. Assign    all keys which   does        exist in requestBody
                    // 3. Create new keys
                    var inputDKMs = requestBody.DKMCodes;

                    // ========== UNASSIGN =====================================
                    // 1. Unassign  all keys which   does not    exist in requestBody
                    // *get not selected items to get "to delete items" later
                    // To get not Selected Items:
                    // DepartmentKeyMetrics - User selected DepartmentKeyMetrics
                    var notSelectedDKMs = DepartmentKeyMetrics
                        .Where(dkm => !inputDKMs
                            .Select(i => Guid.Parse(i.Code))
                            .Contains(dkm.LookupId))
                        .ToList();
                    var toDeleteItems = KeyKpiSubmissionConstraints
                        .Where(c => notSelectedDKMs
                            .Select(dkms => dkms.LookupId)
                            .Contains(c.DepartmentKeyMetric.LookupId)
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
                            .Select(i => Guid.Parse(i.Code))
                            .Contains(k.LookupId))
                       .ToList();

                    // 2. Assign    all keys which   does        exist in requestBody
                    // **** need review this block
                    // **** this code run even already exist
                    //      and update and set isDelete to false (aka. unDelete)
                    var toReassignItems = KeyKpiSubmissionConstraints
                        .Where(c => toAssignItems
                            .Select(i => i.LookupId)
                            .Contains(c.DepartmentKeyMetric.LookupId)
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
                            .Select(c => c.DepartmentKeyMetric.LookupId)
                            .Contains(i.LookupId))
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
            catch (Exception ex)
            {
                // 1. Log
                _logger.LogError(ex, "Unexpected error occured while saving submission constraints. {msg}", ex.Message);
                // 2. set message
                StatusSuccess = false;
                StatusMessage = "Save failed!";
            }
        }

        return Page();
    }

    [BindProperty]
    public UserSelectionViewModel UserSelection { get; set; } = null!;

    public class UserSelectionViewModel
    {
        public Guid SubmitterDepartmentCode { get; set; }
        public List<DKMCodeViewModel> DKMCodes { get; set; } = [];
    }

    public class DKMCodeViewModel
    {
        public string Code { get; set; } = null!;
    }

    // let userSelection = {
    //         submitterDepartmentCode: "@Model.CurrentDepartmentCode?.ToString()",
    //         departmentKeyMetrics: []
    //     };


    // [TempData]
    // public MessageViewModel? TempMessage { get; set; }
    [TempData]
    public string? StatusMessage { get; set; }
    [TempData]
    public bool StatusSuccess { get; set; }


    private async Task<List<DepartmentViewModel>> LoadDepartmentList()
    {
        List<DepartmentViewModel> departmentList = [];

        var departments = await _departmentService.FindAllAsync(1, 50);

        if (departments.IsSuccess && departments.Data != null)
        {
            departmentList = departments.Data
                .Select(e => e.MapToViewModel())
                .ToList();
        }
        else
        {
            ModelState.AddModelError(string.Empty, "No Department exist.");
        }
        return departmentList;
    }

    private async Task<KpiPeriodViewModel?> LoadKpiPeriod(string periodName)
    {
        KpiPeriodViewModel? kpiPeriodModel = null;

        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "Period Name is required.");
        }
        else
        {
            var kpiPeriod = await _kpiSubmissionPeriodService.FindByKpiPeriodNameAsync(periodName);
            if (kpiPeriod != null)
            {
                kpiPeriodModel = new KpiPeriodViewModel() // ---- do we need entire KPI Period object??
                {
                    Id = kpiPeriod.Id,
                    PeriodName = kpiPeriod.PeriodName,
                    SubmissionStartDate = kpiPeriod.SubmissionStartDate,
                    SubmissionEndDate = kpiPeriod.SubmissionEndDate
                };
            }
            else
            {
                ModelState.AddModelError(string.Empty, $"Period {periodName} not found.");
            }
        }

        return kpiPeriodModel;
    }
}


