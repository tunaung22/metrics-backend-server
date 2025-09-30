using Metrics.Application.DTOs.KeyKpiSubmissionConstraints;
using Metrics.Web.Models;

namespace Metrics.Web.Common.Mappers;

public static class KeyKpiSubmissionConstraintMapper
{

    public static KeyKpiSubmissionConstraintViewModel MapToViewModel(this KeyKpiSubmissionConstraintDto e)
    {
        return new KeyKpiSubmissionConstraintViewModel
        {
            Id = e.Id,
            LookupId = e.LookupId,
            IsDeleted = e.IsDeleted,
            SubmitterDepartmentId = e.SubmitterDepartmentId,
            SubmitterDepartment = e.SubmitterDepartment.MapToViewModel(),
            DepartmentKeyMetricId = e.DepartmentKeyMetricId,
            DepartmentKeyMetric = e.DepartmentKeyMetric.MapToViewModel(),
        };
    }

}
