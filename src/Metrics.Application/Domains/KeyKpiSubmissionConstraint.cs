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
    public Department SubmitterDepartment { get; set; } = null!;
    public long DepartmentKeyMetricId { get; set; }
    public DepartmentKeyMetric DepartmentKeyMetric { get; set; } = null!;
}
