using System;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Models.KpiPeriodViewModels;

public class KpiPeriodCreateViewModel
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
