using Metrics.Application.Domains;
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

    public static DepartmentKeyMetricCreateDto MapToCreateDto(this DepartmentKeyMetricViewModel model)
    {
        return new DepartmentKeyMetricCreateDto
        {
            // Id = dto.Id,
            // DepartmentKeyMetricCode = model.LookupId,
            IsDeleted = model.IsDeleted,
            SubmissionPeriodId = model.SubmissionPeriodId,
            // SubmissionPeriod = model.SubmissionPeriod.MapToEntity(),
            KeyIssueDepartmentId = model.KeyIssueDepartmentId,
            // DepartmentDto = model.KeyIssueDepartment.MapToEntity(),
            KeyMetricId = model.KeyMetricId,
            // KeyMetricDto = model.KeyMetric.MapToEntity(),
        };
    }

    public static DepartmentKeyMetricDto MapToDto(this DepartmentKeyMetricViewModel model)
    {
        return new DepartmentKeyMetricDto
        {
            Id = model.Id,
            LookupId = model.LookupId,
            IsDeleted = model.IsDeleted,
            SubmissionPeriodId = model.SubmissionPeriodId,
            // SubmissionPeriod = model.SubmissionPeriod.MapToEntity(),
            KeyIssueDepartmentId = model.KeyIssueDepartmentId,
            // DepartmentDto = model.KeyIssueDepartment.MapToEntity(),
            KeyMetricId = model.KeyMetricId,
            // KeyMetricDto = model.KeyMetric.MapToEntity(),
        };
    }


    // public static DepartmentKeyMetric MapToEntity(this DepartmentKeyMetricViewModel model)
    // {
    //     return new DepartmentKeyMetric
    //     {
    //         // Id = dto.Id,
    //         DepartmentKeyMetricCode = model.LookupId,
    //         IsDeleted = model.IsDeleted,
    //         KpiSubmissionPeriodId = model.SubmissionPeriodId,
    //         // KpiSubmissionPeriod = model.SubmissionPeriod.MapToEntity(),
    //         DepartmentId = model.KeyIssueDepartmentId,
    //         // KeyIssueDepartment = model.KeyIssueDepartment.MapToEntity(),
    //         KeyMetricId = model.KeyMetricId,
    //         // KeyMetric = model.KeyMetric.MapToEntity(),
    //     };
    // }
}
