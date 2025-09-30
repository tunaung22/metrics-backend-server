using Metrics.Application.DTOs.User;

namespace Metrics.Application.DTOs.CaseFeedbackScoreSubmission;

public record CaseFeedbackScoreSubmissionDto(
    long Id,
    Guid LookupId,
    DateTimeOffset SubmittedAt,
    DateOnly SubmissionDate,
    decimal NegativeScoreValue,
    string? Comments,
    string SubmitterId,
    UserDto SubmittedBy,
    long CaseFeedbackId,
    CaseFeedbackDto CaseFeedback,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt)
{ }
