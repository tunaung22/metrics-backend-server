namespace Metrics.Application.DTOs.KeyKpiSubmissions;

public record CreateKeyKpiSubmissionDto
{
    public long PeriodId { get; init; }
    public DateTimeOffset SubmittedAt { get; set; } // accept or not?
    public string SubmitterId { get; init; } = null!;
    public long DepartmentKeyMetricId { get; init; }
    public decimal ScoreValue { get; init; }
    public string? Comments { get; init; } = string.Empty;
}
