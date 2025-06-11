using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metrics.Web.Pages.Manage.Submissions.DepartmentKeyMetrics;

public class CopyModel : PageModel
{
    private readonly IDepartmentService _departmentService;
    private readonly IKpiSubmissionPeriodService _kpiPeriodService;
    private readonly IDepartmentKeyMetricService _departmentKeyMetricService;


    public CopyModel(
    IDepartmentService departmentService,
    IKpiSubmissionPeriodService kpiPeriodService,
        IDepartmentKeyMetricService departmentKeyMetricService
        )
    {
        _departmentService = departmentService;
        _kpiPeriodService = kpiPeriodService;
        _departmentKeyMetricService = departmentKeyMetricService;
    }


    // =============== MODELS ==================================================
    // public List<SelectListItem> SourcePeriodListItems { get; set; } = [];
    public List<SelectListItem> DestinationPeriodListItems { get; set; } = [];

    public KpiPeriodViewModel SourcePeriod { get; set; }
    public string SourcePeriodName { get; set; }
    public string DestinationPeriodName { get; set; }

    public class DepartmentKeyMetricViewModel
    {
        public long Id { get; set; }
        public Guid DepartmentKeyMetricCode { get; set; }
        public bool IsDeleted { get; set; }
        public long KpiSubmissionPeriodId { get; set; }
        public long DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public long KeyMetricId { get; set; }
        public string? KeyMetricTitle { get; set; }
    }

    public List<DepartmentKeyMetricViewModel> DepartmentKeyMetrics { get; set; } = [];

    [BindProperty]
    public List<SelectListItem> DepartmentListItems { get; set; } = [];

    [BindProperty(Name = "Department", SupportsGet = true)]
    public string? SelectedDepartmentCode { get; set; } // QueryString => Current Department Code

    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync([FromRoute] string periodName)
    {
        if (string.IsNullOrEmpty(periodName))
        {
            ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
            return Page();
        }
        SourcePeriodName = periodName;

        var allPeriods = await _kpiPeriodService.FindAllAsync();
        var sourcePeriod = allPeriods.Where(p => p.PeriodName == SourcePeriodName).FirstOrDefault();
        if (sourcePeriod != null)
        {
            SourcePeriod = new KpiPeriodViewModel
            {
                Id = sourcePeriod.Id,
                PeriodName = sourcePeriod.PeriodName,
                SubmissionStartDate = sourcePeriod.SubmissionStartDate,
                SubmissionEndDate = sourcePeriod.SubmissionEndDate
            };
        }
        ;
        var destinationPeriods = allPeriods
            .Where(e => e.PeriodName != SourcePeriodName)
            .ToList();
        if (!periodName.Any())
        {
            ModelState.AddModelError(string.Empty, "No Submission Period exist.");
            return Page();
        }
        if (string.IsNullOrEmpty(DestinationPeriodName))
        {
            // if (string.IsNullOrEmpty(destinationPeriod))
            // {
            //     ModelState.AddModelError(string.Empty, "A valid Period Name is required.");
            //     return Page();
            // }
            DestinationPeriodName = destinationPeriods.First().PeriodName.ToString();
        }
        DestinationPeriodListItems = destinationPeriods.Select(p => new SelectListItem
        {
            Value = p.PeriodName,
            Text = p.PeriodName
        }).ToList();



        // ----------Approach 1-------------------------------------------------
        // var departmentKeyMetrics = await _departmentKeyMetricService
        //        .FindAllByPeriodNameAsync(SourcePeriodName);
        // if (departmentKeyMetrics.Any())
        // {
        //     DepartmentKeyMetrics = departmentKeyMetrics
        //         .Select(k => new DepartmentKeyMetricViewModel
        //         {
        //             Id = k.Id,
        //             DepartmentKeyMetricCode = k.DepartmentKeyMetricCode,
        //             KpiSubmissionPeriodId = k.KpiSubmissionPeriodId,
        //             DepartmentId = k.DepartmentId,
        //             DepartmentName = k.TargetDepartment.DepartmentName,
        //             KeyMetricId = k.KeyMetricId,
        //             KeyMetricTitle = k.KeyMetric.MetricTitle,
        //             IsDeleted = k.IsDeleted
        //         }).ToList();
        // }

        // ----------Approach 2-------------------------------------------------
        // Departments
        var departments = await _departmentService.FindAllAsync();
        if (!departments.Any())
        {
            ModelState.AddModelError(string.Empty, "No Department exist.");
            return Page();
        }
        if (string.IsNullOrEmpty(SelectedDepartmentCode))
        {
            SelectedDepartmentCode = departments.First().DepartmentCode.ToString();
        }

        //

        DepartmentListItems = departments.Select((d, index) => new SelectListItem
        {
            Value = d.DepartmentCode.ToString(),
            Text = $"{(index + 1).ToString("00")}. {d.DepartmentName} {d.DepartmentKeyMetrics.Where(e => e.DepartmentId == d.Id).Count()}"
        }).ToList();

        if (SelectedDepartmentCode != null)
        {
            var result = await _departmentKeyMetricService
                // .FindAllByPeriodNameAsync(CurrentPeriodName);
                .FindAllByPeriodAndDepartmentAsync(
                    SourcePeriodName,
                    Guid.Parse(SelectedDepartmentCode));
            if (result.Any())
            {
                DepartmentKeyMetrics = result
                    .Select(k => new DepartmentKeyMetricViewModel
                    {
                        Id = k.Id,
                        DepartmentKeyMetricCode = k.DepartmentKeyMetricCode,
                        KpiSubmissionPeriodId = k.KpiSubmissionPeriodId,
                        DepartmentId = k.DepartmentId,
                        DepartmentName = k.TargetDepartment.DepartmentName,
                        KeyMetricId = k.KeyMetricId,
                        KeyMetricTitle = k.KeyMetric.MetricTitle,
                        IsDeleted = k.IsDeleted
                    }).ToList();
            }
        }


        return Page();
    }


    // =============== HANDLERS ================================================

}
