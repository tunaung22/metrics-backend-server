using Metrics.Application.Domains;
using Metrics.Application.DTOs.KpiPeriod;

namespace Metrics.Application.Common.Mappers;

public static class KpiPeriodMapper
{
    public static KpiSubmissionPeriod MapToEntity(this KpiPeriodDto dto)
    {
        // TODO: Use factory method
        return new KpiSubmissionPeriod
        {
            PeriodName = dto.PeriodName,
            SubmissionStartDate = dto.SubmissionStartDate.UtcDateTime,
            SubmissionEndDate = dto.SubmissionEndDate.UtcDateTime
        };
    }

    public static KpiSubmissionPeriod MapToEntity(this KpiPeriodCreateDto createDto)
    {
        // TODO: Use factory method
        return new KpiSubmissionPeriod
        {
            PeriodName = createDto.PeriodName,
            SubmissionStartDate = createDto.SubmissionStartDate.UtcDateTime,
            SubmissionEndDate = createDto.SubmissionEndDate.UtcDateTime
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
