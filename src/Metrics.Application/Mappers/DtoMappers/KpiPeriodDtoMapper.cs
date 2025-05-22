using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Domains;

namespace Metrics.Application.Mappers.DtoMappers;

public static class KpiPeriodDtoMapper
{
    // ========== Entity to DTO ==========
    // public static List<KpiPeriodGetDto> ToGetDto(this List<KpiPeriod> entities) { }
    public static List<KpiPeriodGetDto> ToGetDto(this IEnumerable<KpiSubmissionPeriod> entities)
    {
        if (entities == null) return [];

        return entities.Select(e => new KpiPeriodGetDto
        {
            PeriodName = e.PeriodName,
            SubmissionStartDate = e.SubmissionStartDate,
            SubmissionEndDate = e.SubmissionEndDate
        }).ToList();
    }

    public static KpiPeriodGetDto ToGetDto(this KpiSubmissionPeriod entity)
    {
        return new KpiPeriodGetDto
        {
            PeriodName = entity.PeriodName,
            SubmissionStartDate = entity.SubmissionStartDate,
            SubmissionEndDate = entity.SubmissionEndDate
        };
    }

    // ========== DTO to Entity ==========
    public static KpiSubmissionPeriod ToEntity(this KpiPeriodCreateDto dto)
    {
        return new KpiSubmissionPeriod
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
