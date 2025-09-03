namespace Metrics.Web.Models;

public class CaseFeedbackViewModel
{
    public long Id { get; set; }
    public Guid LookupId { get; set; }
    public long KpiSubmissionPeriodId { get; set; }
    public KpiPeriodViewModel TargetPeriod { get; set; } = null!;
    public DateTimeOffset SubmittedAt { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field
    public string FeedbackSubmitterId { get; set; } = null!;
    public UserViewModel FeedbackSubmittedBy { get; set; } = null!;
    // Case Info
    public long CaseDepartmentId { get; set; }
    public DepartmentViewModel CaseDepartment { get; set; } = null!;
    public string WardName { get; set; } = null!;
    public string CPINumber { get; set; } = null!;
    public string PatientName { get; set; } = null!;
    public string RoomNumber { get; set; } = null!;
    public DateTimeOffset IncidentAt { get; set; }
    // Case Info > Details
    public string? Description { get; set; } = string.Empty; // Case Details
    // public string? Comments { get; set; } = string.Empty; // Additional Notes

    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }

    // Collection Navigational Properties
    // public List<CaseFeedbackScoreSubmissionViewModel> Submissions { get; set; } = [];
}
