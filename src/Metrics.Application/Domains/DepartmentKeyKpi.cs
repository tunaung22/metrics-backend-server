using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class DepartmentKeyKpi : IAuditColumn
{
    public long Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }

    // Foreign Keys
    public long DepartmentId { get; set; }
    public long KeyKpiMetricId { get; set; }

    // Reference Navigational Properties
    public Department Department { get; set; } = null!;
    public KeyKpi KeyKpi { get; set; } = null!;
}
