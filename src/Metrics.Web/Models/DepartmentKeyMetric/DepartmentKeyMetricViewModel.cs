using Metrics.Web.Models.KeyMetric;

namespace Metrics.Web.Models.DepartmentKeyMetric;

public class DepartmentKeyMetricViewModel
{
    public long Id { get; set; }
    public Guid LookupId { get; set; }
    public bool IsDeleted { get; set; }
    public long SubmissionPeriodId { get; set; }
    public KpiPeriodViewModel SubmissionPeriod { get; set; } = null!;
    public long KeyIssueDepartmentId { get; set; }
    public DepartmentViewModel KeyIssueDepartment { get; set; } = null!;
    public long KeyMetricId { get; set; }
    public KeyMetricViewModel KeyMetric { get; set; } = null!;
}

// public class DepartmentKeyMetricViewModel
// {
//     public long Id { get; set; }
//     public Guid DepartmentKeyMetricCode { get; set; }
//     public long KpiSubmissionPeriodId { get; set; }
//     public long DepartmentId { get; set; }
//     public long KeyMetricId { get; set; }
//     // public KpiPeriodViewModel KpiSubmissionPeriod { get; set; } = null!;
//     public KpiSubmissionPeriod KpiSubmissionPeriod { get; set; } = null!;
//     // public DepartmentViewModel TargetDepartment { get; set; } = null!;
//     public Department TargetDepartment { get; set; } = null!;
//     // public KeyMetricViewModel KeyMetric { get; set; } = null!;
//     public KeyMetric KeyMetric { get; set; } = null!;
// }