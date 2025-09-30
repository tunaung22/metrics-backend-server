using Metrics.Application.Domains;
using Metrics.Application.DTOs.KeyKpiSubmissions;

namespace Metrics.Application.Common.Mappers;

public static class KeyKpiSubmissionMapper
{
    public static KeyKpiSubmission MapToEntity(this CreateKeyKpiSubmissionDto createDto)
    {
        return new KeyKpiSubmission
        {
            SubmitterId = createDto.SubmitterId,
            DepartmentKeyMetricId = createDto.DepartmentKeyMetricId,
            ScoreValue = createDto.ScoreValue,
            Comments = createDto.Comments
        };
    }

    public static KeyKpiSubmissionDto MapToDto(this KeyKpiSubmission e)
    {
        return new KeyKpiSubmissionDto
        {
            Id = e.Id,
            PeriodId = e.DepartmentKeyMetric.KpiSubmissionPeriodId,
            SubmittedAt = e.SubmittedAt,
            SubmitterId = e.SubmitterId,
            SubmittedBy = e.SubmittedBy.MapToDto(),
            DepartmentKeyMetricId = e.DepartmentKeyMetricId,
            DepartmentKeyMetric = e.DepartmentKeyMetric.MapToDto(),
            ScoreValue = e.ScoreValue,
            Comments = e.Comments,
        };
    }
}
