using Metrics.Application.DTOs.KpiPeriod;
using Metrics.Web.Models;

namespace Metrics.Web.Common.Mappers;

public static class KpiPeriodMapper
{
    public static KpiPeriodDto MapToDto(this KpiPeriodViewModel model)
    {
        return new KpiPeriodDto
        {
            Id = model.Id,
            PeriodName = model.PeriodName,
            SubmissionStartDate = model.SubmissionStartDate,
            SubmissionEndDate = model.SubmissionEndDate
        };
    }

    public static KpiPeriodViewModel MapToViewModel(this KpiPeriodDto dto)
    {
        return new KpiPeriodViewModel
        {
            Id = dto.Id,
            PeriodName = dto.PeriodName,
            SubmissionStartDate = dto.SubmissionStartDate,
            SubmissionEndDate = dto.SubmissionEndDate
        };
    }
}

