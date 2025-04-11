using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Entities;

namespace Metrics.Application.Mappers.DtoMappers;

public static class KpiPeriodDtoMapper
{
    // ========== Entity to DTO ==========
    // public static List<KpiPeriodGetDto> ToGetDto(this List<KpiPeriod> entities) { }
    public static List<KpiPeriodGetDto> ToGetDto(this IEnumerable<KpiPeriod> entities)
    {
        if (entities == null) return [];

        return entities.Select(e => new KpiPeriodGetDto
        {
            PeriodName = e.PeriodName,
            SubmissionStartDate = e.SubmissionStartDate,
            SubmissionEndDate = e.SubmissionEndDate
        }).ToList();
    }

    public static KpiPeriodGetDto ToGetDto(this KpiPeriod entity)
    {
        return new KpiPeriodGetDto
        {
            PeriodName = entity.PeriodName,
            SubmissionStartDate = entity.SubmissionStartDate,
            SubmissionEndDate = entity.SubmissionEndDate
        };
    }

    // ========== DTO to Entity ==========
    public static KpiPeriod ToEntity(this KpiPeriodCreateDto dto)
    {
        return new KpiPeriod
        {
            PeriodName = dto.PeriodName,
            SubmissionStartDate = dto.SubmissionStartDate,
            SubmissionEndDate = dto.SubmissionEndDate
        };
    }

    // public static KpiPeriod ToEntity(this KpiPeriodUpdateDto dto)
    // {
    //     return new KpiPeriod
    //     {
    //         SubmissionStartDate = dto.SubmissionStartDate,
    //         SubmissionEndDate = dto.SubmissionEndDate
    //     };
    // }

    // ========== DTO to DTO ==========
    public static KpiPeriodGetDto ToGetDto(this KpiPeriodCreateDto dto)
    {
        return new KpiPeriodGetDto
        {
            SubmissionStartDate = dto.SubmissionStartDate,
            SubmissionEndDate = dto.SubmissionEndDate
        };
    }
}
