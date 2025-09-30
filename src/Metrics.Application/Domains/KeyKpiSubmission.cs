using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class KeyKpiSubmission : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    // public long ScoreSubmissionPeriodId { get; set; }
    public long DepartmentKeyMetricId { get; set; }
    public decimal ScoreValue { get; set; }
    public string? Comments { get; set; } = string.Empty;
    public string SubmitterId { get; set; } = null!;

    public DateTimeOffset SubmittedAt { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; }

    // Reference Navigational Properties
    // public KpiSubmissionPeriod TargetPeriod { get; set; } = null!;
    public DepartmentKeyMetric DepartmentKeyMetric { get; set; } = null!;
    public ApplicationUser SubmittedBy { get; set; } = null!;
}
