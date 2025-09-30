using Metrics.Application.DTOs.KeyKpiSubmissions;
using Metrics.Web.Models;

namespace Metrics.Web.Common.Mappers;

public static class KeyKpiSubmissionMapper
{
    public static KeyKpiSubmissionViewModel MapToViewModel(this KeyKpiSubmissionDto dto)
    {
        return new KeyKpiSubmissionViewModel
        {
            Id = dto.Id,
            PeriodId = dto.PeriodId,
            SubmitterId = dto.SubmitterId,
            SubmittedBy = dto.SubmittedBy.MapToViewModel(),
            DepartmentKeyMetricId = dto.DepartmentKeyMetricId,
            DepartmentKeyMetric = dto.DepartmentKeyMetric.MapToViewModel(),
            ScoreValue = dto.ScoreValue,
            Comments = dto.Comments,
        };
    }
}
