using System;

namespace Metrics.Web.Mappers;

public class EmployeeModelMapper
{
    // // ========== DTO to ViewModel ==========
    // public static IEnumerable<KpiPeriodGetViewModel>? ToViewModel(this IEnumerable<KpiPeriodGetDto>? dto)
    // {
    //     if (dto == null)
    //         return null;

    //     return dto.Select(e => new KpiPeriodGetViewModel
    //     {
    //         PeriodName = e.PeriodName,
    //         SubmissionStartDate = e.SubmissionStartDate,
    //         SubmissionEndDate = e.SubmissionEndDate
    //     });
    // }

    // public static KpiPeriodGetViewModel? ToViewModel(this KpiPeriodGetDto dto)
    // {
    //     return new KpiPeriodGetViewModel
    //     {

    //         PeriodName = dto.PeriodName,
    //         SubmissionStartDate = dto.SubmissionStartDate,
    //         SubmissionEndDate = dto.SubmissionEndDate
    //     };
    //     //return dto?.ToViewModel();
    // }

    // // ========== ViewModel to DTO ==========
    // public static KpiPeriodCreateDto ToCreateDto(this KpiPeriodCreateViewModel viewModel)
    // {
    //     return new KpiPeriodCreateDto
    //     {

    //         PeriodName = viewModel.PeriodName,
    //         SubmissionStartDate = viewModel.SubmissionStartDate,
    //         SubmissionEndDate = viewModel.SubmissionEndDate
    //     };
    // }
}
