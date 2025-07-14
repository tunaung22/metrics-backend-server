using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IServices;

public interface IKpiSubmissionPeriodService
{
    // ========== Return Entity ================================================
    Task<KpiSubmissionPeriod> CreateAsync(KpiSubmissionPeriod entity);
    Task<KpiSubmissionPeriod> UpdateAsync(string periodName, KpiSubmissionPeriod entity);
    Task<bool> DeleteAsync(string periodName);
    Task<KpiSubmissionPeriod> FindByIdAsync(long id);
    Task<KpiSubmissionPeriod?> FindByKpiPeriodNameAsync(string periodName);
    Task<long> FindIdByKpiPeriodNameAsync(string periodName);
    Task<IEnumerable<KpiSubmissionPeriod>> FindAllAsync();
    Task<IEnumerable<KpiSubmissionPeriod>> FindAllAsync(int pageNumber = 1, int pageSize = 20);
    // Task<IEnumerable<KpiPeriod>> FindAllByDateAsync(int pageNumber, int pageSize, DateTimeOffset todayDate);
    Task<IEnumerable<KpiSubmissionPeriod>> FindAllByDateAsync(DateTimeOffset todayDate);
    Task<bool> KpiPeriodNameExistsAsync(string kpiName);
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
