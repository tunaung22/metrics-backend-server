using Metrics.Application.DTOs.Department;
using Metrics.Application.DTOs.DepartmentKeyMetric;

namespace Metrics.Application.DTOs.KeyKpiSubmissionConstraints;

public record KeyKpiSubmissionConstraintDto
{
    public long Id { get; init; }
    public Guid LookupId { get; init; }
    public bool IsDeleted { get; init; }
    // public DateTimeOffset CreatedAt { get; init; }
    // public DateTimeOffset ModifiedAt { get; init; }
    public long SubmitterDepartmentId { get; init; }
    public DepartmentDto SubmitterDepartment { get; init; } = null!;
    public long DepartmentKeyMetricId { get; init; }
    public DepartmentKeyMetricDto DepartmentKeyMetric { get; init; } = null!;
}
