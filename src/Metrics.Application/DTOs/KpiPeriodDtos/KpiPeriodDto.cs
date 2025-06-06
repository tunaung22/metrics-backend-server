namespace Metrics.Application.DTOs.KpiPeriodDtos;

public class KpiPeriodDto
{
    public long Id { get; set; }
    public string PeriodName { get; set; } = null!;
    public DateTimeOffset SubmissionStartDate { get; set; }
    public DateTimeOffset SubmissionEndDate { get; set; }

}
