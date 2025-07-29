namespace Metrics.Web.Models;

public class DepartmentKeyMetricViewModel
{
    public long Id { get; set; }
    public Guid LookupId { get; set; }
    public long KpiSubmissionPeriodId { get; set; }
    public long DepartmentId { get; set; }
    public DepartmentViewModel KeyIssuer { get; set; } = null!;
    public long KeyMetricId { get; set; }
    public KeyMetricViewModel KeyMetric { get; set; } = null!;
    public bool IsDeleted { get; set; }
}
