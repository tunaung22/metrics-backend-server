using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Domains;
using Metrics.Web.Models.KpiPeriodViewModels;

namespace Metrics.Web.Mappers.ViewModelMappers;

public static class KpiPeriodViewModelMapper
{
    public static KpiPeriod ToEntity(this KpiPeriodCreateViewModel model)
    {
        return new KpiPeriod
        {
            PeriodCode = model.PeriodName,
            SubmissionStartDate = model.SubmissionStartDate,
            SubmissionEndDate = model.SubmissionEndDate
        };
    }

    public static IEnumerable<KpiPeriodGetViewModel> ToGetViewModel(this IEnumerable<KpiPeriod> entity)
    {
        return entity.Select(e => new KpiPeriodGetViewModel
        {
            PeriodName = e.PeriodCode,
            SubmissionStartDate = e.SubmissionStartDate,
            SubmissionEndDate = e.SubmissionEndDate,
        }).ToList();
    }

    // ========== DTO to ViewModel ==========
    public static IEnumerable<KpiPeriodGetViewModel>? ToViewModel(this IEnumerable<KpiPeriodGetDto>? dto)
    {
        if (dto == null)
            return null;

        return dto.Select(e => new KpiPeriodGetViewModel
        {
            PeriodName = e.PeriodName,
            SubmissionStartDate = e.SubmissionStartDate,
            SubmissionEndDate = e.SubmissionEndDate
        });
    }

    public static KpiPeriodGetViewModel? ToViewModel(this KpiPeriodGetDto dto)
    {
        return new KpiPeriodGetViewModel
        {

            PeriodName = dto.PeriodName,
            SubmissionStartDate = dto.SubmissionStartDate,
            SubmissionEndDate = dto.SubmissionEndDate
        };
        //return dto?.ToViewModel();
    }

    // ========== ViewModel to DTO ==========
    public static KpiPeriodCreateDto ToCreateDto(this KpiPeriodCreateViewModel viewModel)
    {
        return new KpiPeriodCreateDto
        {

            PeriodName = viewModel.PeriodName,
            SubmissionStartDate = viewModel.SubmissionStartDate.UtcDateTime,
            SubmissionEndDate = viewModel.SubmissionEndDate.UtcDateTime
        };
    }
}
