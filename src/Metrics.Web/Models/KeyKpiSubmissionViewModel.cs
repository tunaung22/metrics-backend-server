using Metrics.Web.Models.DepartmentKeyMetric;

namespace Metrics.Web.Models;

public class KeyKpiSubmissionViewModel
{
    public long Id { get; init; }
    public long PeriodId { get; init; }
    public string SubmitterId { get; init; } = null!;
    public UserViewModel SubmittedBy { get; init; } = null!;
    public long DepartmentKeyMetricId { get; init; }
    public DepartmentKeyMetricViewModel DepartmentKeyMetric { get; init; } = null!;
    public decimal ScoreValue { get; init; }
    public string? Comments { get; init; } = string.Empty;
}
