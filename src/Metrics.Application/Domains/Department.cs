using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class Department : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    public Guid DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;

    // Collection Navigational Properties
    public List<ApplicationUser> Employees { get; set; } = [];
    public List<KpiSubmission> DepartmentScores { get; set; } = [];
    public List<KeyKpiSubmission> DepartmentMetricsScores { get; set; } = [];
    public List<KeyKpiSubmissionItem> KeyKpiSubmissionItems { get; set; } = [];
    public List<DepartmentKeyKpi> DepartmentKeyKpiMetrics { get; set; } = [];

}
