using Metrics.Application.Domains;
using Metrics.Application.DTOs.KpiPeriod;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

public interface IKpiSubmissionPeriodService
{
    // ========== Return Entity ================================================
    // Task<KpiSubmissionPeriod> CreateAsync(KpiSubmissionPeriod entity);
    // Task<KpiSubmissionPeriod> UpdateAsync(string periodName, KpiSubmissionPeriod entity);
    // Task<bool> DeleteAsync(string periodName);
    // Task<KpiSubmissionPeriod> FindByIdAsync(long id);
    Task<KpiSubmissionPeriod?> FindByKpiPeriodNameAsync(string periodName);
    // Task<long> FindIdByKpiPeriodNameAsync(string periodName);
    Task<IEnumerable<KpiSubmissionPeriod>> FindAllAsync();
    Task<IEnumerable<KpiSubmissionPeriod>> FindAllAsync(int pageNumber = 1, int pageSize = 20);
    // Task<IEnumerable<KpiPeriod>> FindAllByDateAsync(int pageNumber, int pageSize, DateTimeOffset todayDate);
    Task<IEnumerable<KpiSubmissionPeriod>> FindAllByDateAsync(DateTimeOffset todayDate);

    Task<bool> KpiPeriodNameExistsAsync(string kpiName);
    Task<long> FindCountAsync();

    // ========== Result Pattern =====================================
    Task<Result> CreateAsync(KpiPeriodCreateDto createDto);
    Task<Result> UpdateAsync(string periodName, KpiPeriodUpdateDto updateDto);
    Task<Result> DeleteAsync(string periodName);
    Task<ResultT<List<KpiPeriodDto>>> FindAll_Async();

    // TODO: Migration to Result Pattern
    // Task<ResultT<KpiPeriodDto>> FindByIdAsync(long id);
    // Task<ResultT<KpiPeriodDto>> FindAllAsync(int pageNumber = 1, int pageSize = 20);
    // Task<ResultT<KpiPeriodDto>> FindAllAsync(int pageNumber = 1, int pageSize = 20);
    // Task<ResultT<KpiPeriodDto>> FindAllByDateAsync(DateTimeOffset todayDate);
    // Task<ResultT<KpiPeriodDto>> FindAllByDateAsync(DateTimeOffset todayDate);
    // Task<ResultT<bool>> KpiPeriodNameExistsAsync(string kpiName);
    // Task<ResultT<long>> FindCountAsync();
}
