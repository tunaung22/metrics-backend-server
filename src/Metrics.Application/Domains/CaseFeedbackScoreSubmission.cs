using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class CaseFeedbackScoreSubmission : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    public Guid LookupId { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field
    public decimal NegativeScoreValue { get; set; }
    public string? Comments { get; set; } = string.Empty; // Additional Notes
    public string SubmitterId { get; set; } = null!; // Foreign Keys
    public ApplicationUser SubmittedBy { get; set; } = null!; // Reference Navigational Properties
    public long CaseFeedbackId { get; set; }
    public CaseFeedback Feedback { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; }
}
