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

    public static DepartmentKeyMetric MapToEntity(this DepartmentKeyMetricCreateDto createDto)
    {
        return new DepartmentKeyMetric
        {
            // LookupId = createDto.DepartmentKeyMetricCode,
            IsDeleted = createDto.IsDeleted,

            KpiSubmissionPeriodId = createDto.SubmissionPeriodId,
            // KpiSubmissionPeriod = createDto.SubmissionPeriod.MapToEntity(),

            DepartmentId = createDto.KeyIssueDepartmentId,
            // KeyIssueDepartment = createDto.KeyIssueDepartment.MapToEntity(),

            KeyMetricId = createDto.KeyMetricId,
            // KeyMetric = createDto.KeyMetric.MapToEntity(),
        };
    }

    public static DepartmentKeyMetric MapToEntity(this DepartmentKeyMetricDto dto)
    {
        return new DepartmentKeyMetric
        {
            // LookupId = createDto.DepartmentKeyMetricCode,
            IsDeleted = dto.IsDeleted,

            KpiSubmissionPeriodId = dto.SubmissionPeriodId,
            // KpiSubmissionPeriod = dto.SubmissionPeriod.MapToEntity(),

            DepartmentId = dto.KeyIssueDepartmentId,
            // KeyIssueDepartment = dto.KeyIssueDepartment.MapToEntity(),

            KeyMetricId = dto.KeyMetricId,
            // KeyMetric = dto.KeyMetric.MapToEntity(),

        };
    }

}
