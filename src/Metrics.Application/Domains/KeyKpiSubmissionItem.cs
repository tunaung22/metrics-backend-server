using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class KeyKpiSubmissionItem : IAuditColumn
{
    public long Id { get; set; }
    public decimal ScoreValue { get; set; }
    public string? Comments { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;

    // Foreign Keys
    public long KeyKpiSubmissionId { get; set; }
    public long DepartmentId { get; set; }
    public long KeyKpiMetricsId { get; set; }

    // Reference Navigational Properties
    public KeyKpiSubmission ParentSubmission { get; set; } = null!;
    public Department TargetDepartment { get; set; } = null!;
    public KeyKpi TargetMetric { get; set; } = null!;
}
