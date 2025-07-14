using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class KeyKpiSubmissionConstraint : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    public Guid LookupId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; }

    // Foreign Keys
    public long DepartmentId { get; set; }
    public long DepartmentKeyMetricId { get; set; }

    // Reference Navigational Properties
    public Department Department { get; set; } = null!;
    public DepartmentKeyMetric DepartmentKeyMetric { get; set; } = null!;

    // Collection Navigational Properties

}
