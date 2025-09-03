namespace Metrics.Application.DTOs.KpiPeriod;

public class KpiPeriodCreateDto
{
    public string PeriodName { get; set; } = null!;
    public DateTimeOffset SubmissionStartDate { get; set; }
    public DateTimeOffset SubmissionEndDate { get; set; }


}
