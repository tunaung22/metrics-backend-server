
namespace Metrics.Application.DTOs;

public class KeyKpiSubmissionCreateDto
{
    // public DateTimeOffset SubmittedAt { get; set; }
    public long ScoreSubmissionPeriodId { get; set; }
    public long DepartmentId { get; set; }
    public string ApplicationUserId { get; set; } = null!;
    public List<KeyKpiSubmissionItemDto> KeyKpiSubmissionItemDtos { get; set; } = [];
}
