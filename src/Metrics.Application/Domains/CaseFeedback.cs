using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class CaseFeedback : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    public Guid LookupId { get; set; }
    public long KpiSubmissionPeriodId { get; set; }
    public KpiSubmissionPeriod TargetPeriod { get; set; } = null!;
    public DateTimeOffset SubmittedAt { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field
    // public decimal NegativeScoreValue { get; set; }
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
    // public string? Comments { get; set; } = string.Empty; // Additional Notes

    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; }

    // Collection Navigational Properties
    public List<CaseFeedbackScoreSubmission> Submissions { get; set; } = [];


    private CaseFeedback() { }

    public CaseFeedback(
        long kpiSubmissionPeriodId,
        DateTimeOffset submittedAt,
        string submitterId,
        long caseDepartmentId,
        string wardName,
        string cPINumber,
        string patientName,
        string roomNumber,
        DateTimeOffset incidentAt,
        string? description,
        bool isDeleted)
    {
        KpiSubmissionPeriodId = kpiSubmissionPeriodId;
        SubmittedAt = submittedAt;
        SubmitterId = submitterId;
        CaseDepartmentId = caseDepartmentId;
        WardName = wardName;
        CPINumber = cPINumber;
        PatientName = patientName;
        RoomNumber = roomNumber;
        IncidentAt = incidentAt;
        Description = description;
        IsDeleted = isDeleted;
    }

    public static CaseFeedback Create(
        long kpiSubmissionPeriodId,
        DateTimeOffset submittedAt,
        string submitterId,
        long caseDepartmentId,
        string wardName,
        string cPINumber,
        string patientName,
        string roomNumber,
        DateTimeOffset incidentAt,
        string? description,
        bool isDeleted)
    {
        // domain validations
        if (string.IsNullOrEmpty(submitterId))
            throw new ArgumentException("Submitter ID is required or invalid.", nameof(submitterId));
        if (kpiSubmissionPeriodId == 0)
            throw new ArgumentException("Kpi Submission Period ID is required or invalid.", nameof(kpiSubmissionPeriodId));
        if (caseDepartmentId == 0)
            throw new ArgumentException("Case Department ID is required or invalid.", nameof(caseDepartmentId));
        if (string.IsNullOrEmpty(wardName))
            throw new ArgumentException("Ward name is required.", nameof(wardName));
        if (string.IsNullOrEmpty(cPINumber))
            throw new ArgumentException("CPINumber is required.", nameof(cPINumber));
        if (string.IsNullOrEmpty(roomNumber))
            throw new ArgumentException("Room number is required.", nameof(roomNumber));
        if (string.IsNullOrEmpty(patientName))
            throw new ArgumentException("Patient name is required.", nameof(patientName));

        return new CaseFeedback
        {
            KpiSubmissionPeriodId = kpiSubmissionPeriodId,
            SubmittedAt = submittedAt,
            SubmitterId = submitterId,
            CaseDepartmentId = caseDepartmentId,
            WardName = wardName,
            CPINumber = cPINumber,
            PatientName = patientName,
            RoomNumber = roomNumber,
            IncidentAt = incidentAt,
            Description = description,
            IsDeleted = isDeleted
        };
    }
}
