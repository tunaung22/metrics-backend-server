using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class CaseFeedbackSubmission : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field
    public decimal NegativeScoreValue { get; set; }
    public string SubmitterId { get; set; } = null!; // Foreign Keys
    public ApplicationUser SubmittedBy { get; set; } = null!; // Reference Navigational Properties
    // **get SubmitterDepartment, PhoneNumber from Submitter 
    // public long SubmitterDepartmentId { get; set; } // Foreign Keys
    // public Department SubmitterDepartment { get; set; } = null!; // Reference Navigational Properties
    // public string? PhoneNumber { get; set; } = string.Empty;

    // Case Info
    public long CaseDepartmentId { get; set; } // Foreign Keys
    public Department CaseDepartment { get; set; } = null!; // Reference Navigational Properties
    public string WardName { get; set; } = null!;
    public string CPINumber { get; set; } = null!; // Common Patient Identifier or Central/Clinical Patient Index
    public string PatientName { get; set; } = null!;
    public string RoomNumber { get; set; } = null!;
    public DateTimeOffset IncidentAt { get; set; }
    // Case Info > Details
    public string? Description { get; set; } = string.Empty; // Case Details
    public string? Comments { get; set; } = string.Empty; // Additional Notes

    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }

    // Collection Navigational Properties
}
