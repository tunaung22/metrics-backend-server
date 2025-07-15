using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class Department : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    public Guid DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; }

    // Collection Navigational Properties
    public List<ApplicationUser> ApplicationUsers { get; set; } = [];
    public List<KpiSubmission> DepartmentScores { get; set; } = [];
    public List<KeyKpiSubmission> KeyKpiSubmissions { get; set; } = [];
    public List<DepartmentKeyMetric> DepartmentKeyMetrics { get; set; } = [];
    public List<CaseFeedbackSubmission> CaseFeedbackSubmissions { get; set; } = [];
    public List<KeyKpiSubmissionConstraint> KeyKpiSubmissionConstraints { get; set; } = [];
}
