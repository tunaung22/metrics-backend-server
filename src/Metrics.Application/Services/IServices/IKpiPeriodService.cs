using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Results;
using Metrics.Domain.Entities;
using System;

namespace Metrics.Application.Services.IServices;

public interface IKpiPeriodService
{
    Task<Result<KpiPeriod>> FindByKpiPeriodNameAsync(string periodName);
    Task<Result<KpiPeriod>> FindByIdAsync(long id);
    Task<Result<KpiPeriod>> CreateAsync(KpiPeriod entity);
    Task<Result<KpiPeriod>> UpdateAsync(KpiPeriod entity);
    Task<Result<bool>> DeleteAsync(string periodName);
    Task<Result<IEnumerable<KpiPeriod>>> FindAllAsync();

    Task<KpiPeriodGetDto> FindByKpiPeriodName_Async(string periodName);
    Task<KpiPeriodGetDto> FindById_Async(long id);
    Task<KpiPeriodGetDto> Create_Async(KpiPeriodCreateDto createDto);
    Task<KpiPeriodGetDto> Update_Async(KpiPeriodUpdateDto updateDto);
    Task<bool> Delete_Async(string periodName);
    Task<IEnumerable<KpiPeriodGetDto>> FindAll_Async();
    Task<IEnumerable<KpiPeriodDto>> FindAllInsecure_Async();
    Task<IEnumerable<KpiPeriodDto>> FindAllByValidDate_Async(DateTimeOffset todayDate);

    // Task<KpiPeriodGetDto> GetByKpiPeriodNameAsync(string periodName);
    // Task<IEnumerable<KpiPeriodGetDto>> GetAllAsync();
    // Task<KpiPeriodGetDto> CreateAsync(KpiPeriodCreateDto dto);
    // Task<KpiPeriodGetDto> UpdateAsync(KpiPeriodUpdateDto dto);
    // Task<bool> DeleteAsync(string periodName);
}
