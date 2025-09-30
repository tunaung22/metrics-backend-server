using Metrics.Application.DTOs.DepartmentKeyMetric;
using Metrics.Application.DTOs.User;

namespace Metrics.Application.DTOs.KeyKpiSubmissions;

public record KeyKpiSubmissionDto
{
    public long Id { get; init; }
    public long PeriodId { get; init; }
    public DateTimeOffset SubmittedAt { get; set; }
    public string SubmitterId { get; init; } = null!;
    public UserDto SubmittedBy { get; init; } = null!;
    public long DepartmentKeyMetricId { get; init; }
    public DepartmentKeyMetricDto DepartmentKeyMetric { get; init; } = null!;
    public decimal ScoreValue { get; init; }
    public string? Comments { get; init; } = string.Empty;
}
