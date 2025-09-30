using Metrics.Application.Domains;
using Metrics.Application.DTOs.DepartmentKeyMetric;

namespace Metrics.Application.Common.Mappers;

public static class DepartmentKeyMetricMapper
{
    public static DepartmentKeyMetricDto MapToDto(this DepartmentKeyMetric e)
    {
        return new DepartmentKeyMetricDto
        {
            Id = e.Id,
            LookupId = e.DepartmentKeyMetricCode,
            IsDeleted = e.IsDeleted,
            SubmissionPeriodId = e.KpiSubmissionPeriodId,
            SubmissionPeriod = e.KpiSubmissionPeriod.MapToDto(),
            KeyIssueDepartmentId = e.DepartmentId,
            KeyIssueDepartment = e.KeyIssueDepartment.MapToDto(),
            KeyMetricId = e.KeyMetricId,
            KeyMetric = e.KeyMetric.MapToDto(),
        };
    }
}
