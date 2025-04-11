namespace Metrics.Application.DTOs.KpiPeriodDtos;

public class KpiPeriodCreateDto
{
    public string PeriodName { get; set; } = null!;
    public DateTimeOffset SubmissionStartDate { get; set; }
    public DateTimeOffset SubmissionEndDate { get; set; }


}
