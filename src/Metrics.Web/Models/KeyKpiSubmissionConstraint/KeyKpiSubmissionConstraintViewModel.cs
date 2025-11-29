using Metrics.Web.Models.DepartmentKeyMetric;

namespace Metrics.Web.Models.KeyKpiSubmissionConstraint;

public class KeyKpiSubmissionConstraintViewModel
{
    public long Id { get; set; }
    public Guid LookupId { get; set; }
    public bool IsDeleted { get; set; }
    public long SubmitterDepartmentId { get; set; }
    public DepartmentViewModel CandidateDepartment { get; set; } = null!;
    public long DepartmentKeyMetricId { get; set; }
    public DepartmentKeyMetricViewModel DepartmentKeyMetric { get; set; } = null!;
}
