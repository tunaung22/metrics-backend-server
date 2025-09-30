namespace Metrics.Web.Models;

public class CaseFeedbackScoreSubmissionViewModel
{
    public long Id { get; set; }
    public Guid LookupId { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field
    public decimal NegativeScoreValue { get; set; }
    public string? Comments { get; set; } = string.Empty; // Additional Notes
    public string SubmitterId { get; set; } = null!; // Foreign Keys
    public UserViewModel SubmittedBy { get; set; } = null!; // Reference Navigational Properties
    public long CaseFeedbackId { get; set; }
    public CaseFeedbackViewModel CaseFeedback { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
