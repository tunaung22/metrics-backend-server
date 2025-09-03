namespace Metrics.Application.DTOs.CaseFeedbackScoreSubmission;

public record CaseFeedbackScoreSubmissionUpsertDto(
    long Id,
    Guid? LookupId,
    string ScoreSubmitterId,
    DateTimeOffset SubmittedAt,
    long CaseFeedbackId,
    decimal NegativeScoreValue,
    string? Comments)
{ }
