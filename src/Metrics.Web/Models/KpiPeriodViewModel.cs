using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Models;

public class KpiPeriodViewModel
{
    public long Id { get; set; }
    // [BindProperty]
    // [Required(ErrorMessage = "Period Name is required.")]
    public required string PeriodName { get; set; }

    // [BindProperty]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTimeOffset SubmissionStartDate { get; set; }

    // [BindProperty]
    public DateTimeOffset SubmissionEndDate { get; set; }
}
