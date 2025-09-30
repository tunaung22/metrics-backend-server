using Metrics.Application.DTOs.DepartmentKeyMetric;
using Metrics.Web.Models.DepartmentKeyMetric;

namespace Metrics.Web.Common.Mappers;

public static class DepartmentKeyMetricMapper
{
    public static DepartmentKeyMetricViewModel MapToViewModel(this DepartmentKeyMetricDto dto)
    {
        return new DepartmentKeyMetricViewModel
        {
            Id = dto.Id,
            LookupId = dto.LookupId,
            IsDeleted = dto.IsDeleted,
            SubmissionPeriodId = dto.SubmissionPeriodId,
            SubmissionPeriod = dto.SubmissionPeriod.MapToViewModel(),
            KeyIssueDepartmentId = dto.KeyIssueDepartmentId,
            KeyIssueDepartment = dto.KeyIssueDepartment.MapToViewModel(),
            KeyMetricId = dto.KeyMetricId,
            KeyMetric = dto.KeyMetric.MapToViewModel(),
        };
    }
}
