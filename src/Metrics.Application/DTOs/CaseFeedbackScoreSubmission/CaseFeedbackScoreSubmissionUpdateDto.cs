namespace Metrics.Application.DTOs.CaseFeedbackScoreSubmission;

public record CaseFeedbackScoreSubmissionUpdateDto(
    long Id,
    decimal NegativeScoreValue,
    string? Comments)
{ }
