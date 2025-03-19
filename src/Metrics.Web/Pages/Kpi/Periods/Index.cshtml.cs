using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Services.IServices;
using Metrics.Web.Mappers.ViewModelMappers;
using Metrics.Web.ViewModels.KpiPeriodViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Kpi.Periods;


public class IndexModel : PageModel
{
    private readonly IKpiPeriodService _kpiPeriodService;

    public IndexModel(IKpiPeriodService kpiPeriodService)
    {
        _kpiPeriodService = kpiPeriodService;
    }

    // ========== Model ====================
    [BindProperty]
    public IEnumerable<KpiPeriodModel>? KpiPeriods { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var result = await _kpiPeriodService.FindAll_Async();
        KpiPeriods = result.Select(e => new KpiPeriodModel
        {
            PeriodName = e.PeriodName,
            SubmissionStartDate = e.SubmissionStartDate,
            SubmissionEndDate = e.SubmissionEndDate
        }).ToList();

        return Page();
    }

    public class KpiPeriodModel
    {
        [Required(ErrorMessage = "Period Name is required.")]
        public string PeriodName { get; set; } = null!;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset SubmissionStartDate { get; set; } = DateTimeOffset.UtcNow.UtcDateTime;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset SubmissionEndDate { get; set; } = DateTimeOffset.UtcNow.UtcDateTime;
    }

}
