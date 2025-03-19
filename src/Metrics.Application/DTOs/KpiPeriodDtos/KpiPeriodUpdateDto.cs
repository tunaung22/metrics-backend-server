using Metrics.Common.Utils;
using System;

namespace Metrics.Application.DTOs.KpiPeriodDtos;

public class KpiPeriodUpdateDto
{
    public long Id { get; set; }
    public string PeriodName { get; set; } = null!;
    public DateTimeOffset SubmissionStartDate { get; set; }
    public DateTimeOffset SubmissionEndDate { get; set; }

}
