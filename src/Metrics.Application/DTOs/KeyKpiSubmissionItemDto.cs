namespace Metrics.Application.DTOs;

public class KeyKpiSubmissionItemDto
{
    public long DepartmentKeyMetricId { get; set; }
    public decimal ScoreValue { get; set; }
    public string? Comments { get; set; } = string.Empty;
}
