using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class KeyKpiSubmissionItem : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    public decimal ScoreValue { get; set; }
    public string? Comments { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; }

    // Foreign Keys
    public long KeyKpiSubmissionId { get; set; }
    public long DepartmentKeyMetricId { get; set; }

    // Reference Navigational Properties
    public KeyKpiSubmission ParentSubmission { get; set; } = null!;
    public DepartmentKeyMetric DepartmentKeyMetric { get; set; } = null!;

}
