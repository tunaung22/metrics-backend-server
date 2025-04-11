using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Entities;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

public interface IKpiPeriodService
{
    // ========== Return Entity ================================================
    Task<KpiPeriod> CreateAsync(KpiPeriod entity);
    Task<KpiPeriod> UpdateAsync(string periodName, KpiPeriod entity);
    Task<bool> DeleteAsync(string periodName);
    Task<KpiPeriod> FindByIdAsync(long id);
    Task<KpiPeriod> FindByKpiPeriodNameAsync(string periodName);
    Task<long> FindIdByKpiPeriodNameAsync(string periodName);
    Task<IEnumerable<KpiPeriod>> FindAllAsync(int pageNumber, int pageSize);
    // Task<IEnumerable<KpiPeriod>> FindAllByDateAsync(int pageNumber, int pageSize, DateTimeOffset todayDate);
    Task<IEnumerable<KpiPeriod>> FindAllByDateAsync(DateTimeOffset todayDate);

    Task<long> FindCountAsync();

    // ========== Operation Result Pattern =====================================
    // Task<Result<KpiPeriod>> FindByKpiPeriodName_ResultAsync(string periodName);
    // Task<Result<KpiPeriod>> FindById_ResultAsync(long id);
    // Task<Result<KpiPeriod>> Create_ResultAsync(KpiPeriod entity);
    // Task<Result<KpiPeriod>> Update_ResultAsync(KpiPeriod entity);
    // Task<Result<bool>> Delete_ResultAsync(string periodName);
    // Task<Result<IEnumerable<KpiPeriod>>> FindAll_ResultAsync();

    // ========== Return DTO ===================================================
    // Task<long> FindIdByKpiPeriodName_Async(string periodName);
    // Task<KpiPeriodGetDto> FindByKpiPeriodName_Async(string periodName);
    // Task<KpiPeriodGetDto> FindById_Async(long id);
    // Task<KpiPeriodGetDto> Create_Async(KpiPeriodCreateDto createDto);
    // Task<KpiPeriodGetDto> Update_Async(KpiPeriodUpdateDto updateDto);
    // Task<bool> Delete_Async(string periodName);
    // Task<IEnumerable<KpiPeriodGetDto>> FindAll_Async();
    // Task<IEnumerable<KpiPeriodDto>> FindAllInsecure_Async();
    // Task<IEnumerable<KpiPeriodDto>> FindAllByValidDate_Async(DateTimeOffset todayDate);
}
