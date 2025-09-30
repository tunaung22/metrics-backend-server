using Metrics.Application.DTOs.Department;
using Metrics.Application.DTOs.KeyMetric;
using Metrics.Application.DTOs.KpiPeriod;

namespace Metrics.Application.DTOs.DepartmentKeyMetric;

public record DepartmentKeyMetricDto
{
    public long Id { get; init; }
    public Guid LookupId { get; init; }
    public bool IsDeleted { get; init; }
    // public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    // public DateTimeOffset ModifiedAt { get; init; }
    public long SubmissionPeriodId { get; init; }
    public KpiPeriodDto SubmissionPeriod { get; init; } = null!;
    public long KeyIssueDepartmentId { get; init; }
    public DepartmentDto KeyIssueDepartment { get; init; } = null!;
    public long KeyMetricId { get; init; }
    public KeyMetricDto KeyMetric { get; init; } = null!;
}
