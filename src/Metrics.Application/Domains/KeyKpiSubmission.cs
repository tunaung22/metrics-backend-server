using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class KeyKpiSubmission : IAuditColumn
{
    public long Id { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; }

    // Foreign Keys
    public long ScoreSubmissionPeriodId { get; set; }
    public long DepartmentId { get; set; }
    public string ApplicationUserId { get; set; } = null!;

    // Reference Navigational Properties
    public KpiSubmissionPeriod TargetPeriod { get; set; } = null!;
    public Department TargetDepartment { get; set; } = null!;
    public ApplicationUser SubmittedBy { get; set; } = null!;

    // Collection Navigational Properties
    public List<KeyKpiSubmissionItem> KeyKpiSubmissionItems { get; set; } = [];
}
