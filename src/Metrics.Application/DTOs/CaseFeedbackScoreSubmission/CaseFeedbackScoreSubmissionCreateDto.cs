namespace Metrics.Application.DTOs.CaseFeedbackScoreSubmission;

public record CaseFeedbackScoreSubmissionCreateDto(
    string ScoreSubmitterId,
    DateTimeOffset SubmittedAt,
    long CaseFeedbackId,
    decimal NegativeScoreValue,
    string? Comments)
{ }
