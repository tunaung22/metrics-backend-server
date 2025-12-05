using Metrics.Application.Domains;
using Metrics.Application.DTOs.KeyKpiSubmissionConstraints;

namespace Metrics.Application.Common.Mappers;

public static class KeyKpiSubmissionConstraintMapper
{
    public static KeyKpiSubmissionConstraintDto MapToDto(this KeyKpiSubmissionConstraint e)
    {
        return new KeyKpiSubmissionConstraintDto
        {
            Id = e.Id,
            LookupId = e.LookupId,
            IsDeleted = e.IsDeleted,
            CandidateDepartmentId = e.CandidateDepartmentId,
            CandidateDepartment = e.CandidateDepartment.MapToDto(),
            DepartmentKeyMetricId = e.DepartmentKeyMetricId,
            DepartmentKeyMetric = e.DepartmentKeyMetric.MapToDto(),
        };
    }
}
