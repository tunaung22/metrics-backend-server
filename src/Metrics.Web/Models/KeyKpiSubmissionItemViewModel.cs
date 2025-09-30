using Metrics.Web.Models.DepartmentKeyMetric;

namespace Metrics.Web.Models;

public class KeyKpiSubmissionItemViewModel
{
    public long Id { get; set; }
    public long KeyKpiSubmissionId { get; set; }
    public long DepartmentKeyMetricId { get; set; }
    public DepartmentKeyMetricViewModel DepartmentKeyMetric { get; set; } = null!;
    public decimal ScoreValue { get; set; }
    public string? Comments { get; set; } = string.Empty;
}
