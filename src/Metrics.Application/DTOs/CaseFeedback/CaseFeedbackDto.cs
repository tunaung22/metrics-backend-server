using Metrics.Application.DTOs.Department;
using Metrics.Application.DTOs.KpiPeriod;
using Metrics.Application.DTOs.User;

namespace Metrics.Application.DTOs;

public record CaseFeedbackDto(
    long Id,
    Guid LookupId,
    // long KpiSubmissionPeriodId,
    // KpiPeriodDto TargetPeriod,
    DateTimeOffset SubmittedAt,
    DateOnly SubmissionDate,
    string FeedbackSubmitterId,
    UserDto FeedbackSubmittedBy,
    // Case Info
    long CaseDepartmentId,
    DepartmentDto CaseDepartment,
    string WardName,
    string CPINumber,
    string PatientName,
    string RoomNumber,
    DateTimeOffset IncidentAt,
    // Case Info > Details
    string? Description,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt)
{ }
