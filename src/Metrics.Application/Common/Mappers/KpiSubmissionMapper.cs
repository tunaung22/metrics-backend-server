using Metrics.Application.Domains;
using Metrics.Application.DTOs.KpiSubmissionDtos;

namespace Metrics.Application.Common.Mappers;

public static class KpiSubmissionMapper
{
    public static KpiSubmissionDto MapToDto(this KpiSubmission e)
    {
        return new KpiSubmissionDto
        {
            SubmissionTime = e.SubmittedAt.ToLocalTime(),
            SubmissionDate = e.SubmissionDate,
            ScoreValue = e.ScoreValue,
            Comments = e.Comments ?? string.Empty,
            PositiveAspects = e.PositiveAspects ?? string.Empty,
            NegativeAspects = e.NegativeAspects ?? string.Empty,
            KpiPeriodId = e.KpiSubmissionPeriodId,
            KpiPeriod = e.TargetPeriod.MapToDto(),
            DepartmentId = e.DepartmentId,
            TargetDepartment = e.TargetDepartment.MapToDto(),
            SubmitterId = e.ApplicationUserId,
            SubmittedBy = e.SubmittedBy.MapToDto(),
        };
    }
}
