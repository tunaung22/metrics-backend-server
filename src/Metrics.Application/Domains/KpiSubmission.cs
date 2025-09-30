using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class KpiSubmission : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field
    public decimal ScoreValue { get; set; }
    public string? PositiveAspects { get; set; } = string.Empty;
    public string? NegativeAspects { get; set; } = string.Empty;
    public string? Comments { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; }

    // Foreign Keys
    public long KpiSubmissionPeriodId { get; set; }
    public long DepartmentId { get; set; }
    public string ApplicationUserId { get; set; } = null!;

    // Reference Navigational Properties
    public KpiSubmissionPeriod TargetPeriod { get; set; } = null!;
    public Department TargetDepartment { get; set; } = null!;
    public ApplicationUser SubmittedBy { get; set; } = null!;
}
