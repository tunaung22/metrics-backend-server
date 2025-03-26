using System;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Models.KpiPeriodViewModels;

public class KpiPeriodGetViewModel
{
    // [BindProperty]
    [Required(ErrorMessage = "Period Name is required.")]
    public string PeriodName { get; set; } = null!;

    // [BindProperty]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTimeOffset SubmissionStartDate { get; set; }

    // [BindProperty]
    public DateTimeOffset SubmissionEndDate { get; set; }
}
