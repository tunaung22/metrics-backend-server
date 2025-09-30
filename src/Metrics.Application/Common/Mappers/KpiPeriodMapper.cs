using Metrics.Application.Domains;
using Metrics.Application.DTOs.KpiPeriod;

namespace Metrics.Application.Common.Mappers;

public static class KpiPeriodMapper
{
    public static KpiSubmissionPeriod MapToEntity(this KpiPeriodCreateDto createDto)
    {
        // TODO: Use factory method
        return new KpiSubmissionPeriod
        {
            PeriodName = createDto.PeriodName,
            SubmissionStartDate = createDto.SubmissionStartDate,
            SubmissionEndDate = createDto.SubmissionEndDate
        };
    }

    public static KpiPeriodDto MapToDto(this KpiSubmissionPeriod e)
    {
        return new KpiPeriodDto
        {
            Id = e.Id,
            PeriodName = e.PeriodName,
            SubmissionStartDate = e.SubmissionStartDate,
            SubmissionEndDate = e.SubmissionEndDate
        };
    }
}
