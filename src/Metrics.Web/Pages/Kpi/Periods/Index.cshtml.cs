using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Pages.Kpi.Periods;


public class IndexModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly IKpiPeriodService _kpiPeriodService;

    public IndexModel(
        IConfiguration config,
        IKpiPeriodService kpiPeriodService)
    {
        _config = config;
        _kpiPeriodService = kpiPeriodService;
    }


    // =============== MODELS ==================================================
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
    // =========================================================================


    [BindProperty]
    public IEnumerable<KpiPeriodModel>? KpiPeriods { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; }
    public long TotalKpiPeriods { get; set; } // Count
    public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalKpiPeriods, PageSize));
    public bool ShowPrevious => CurrentPage > 1;
    public bool ShowNext => CurrentPage < TotalPages;


    // =============== HANDLERS ================================================
    public async Task<IActionResult> OnGetAsync()
    {
        // try
        // {
        PageSize = _config.GetValue<int>("Pagination:PageSize");
        TotalKpiPeriods = await _kpiPeriodService.FindCountAsync();
        var result = await _kpiPeriodService.FindAllAsync(CurrentPage, PageSize);
        KpiPeriods = result.Select(e => new KpiPeriodModel
        {
            PeriodName = e.PeriodName,
            SubmissionStartDate = e.SubmissionStartDate,
            SubmissionEndDate = e.SubmissionEndDate
        }).ToList();

        return Page();
        // catch (Exception e)
        // {
        //     ModelState.AddModelError("", e.Message);
        //     return Page();
        // }
    }

}
