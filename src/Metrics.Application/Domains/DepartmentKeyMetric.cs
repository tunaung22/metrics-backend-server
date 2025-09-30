using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class DepartmentKeyMetric : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    public Guid DepartmentKeyMetricCode { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; }

    // Foreign Keys
    public long KpiSubmissionPeriodId { get; set; }
    public long DepartmentId { get; set; }
    public long KeyMetricId { get; set; }

    // Reference Navigational Properties
    public KpiSubmissionPeriod KpiSubmissionPeriod { get; set; } = null!;
    public Department KeyIssueDepartment { get; set; } = null!;
    public KeyMetric KeyMetric { get; set; } = null!;

    // Collection Navigational Properties
    public List<KeyKpiSubmission> KeyKpiSubmissions { get; set; } = [];
    public List<KeyKpiSubmissionConstraint> KeyKpiSubmissionConstraints { get; set; } = [];
}
